using Microsoft.AspNetCore.Mvc;
using dotnetshop.Models;
using dotnetshop.Repositories;
using dotnetshop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using dotnetshop.Utils;
using System.Security.Claims;
using Stripe;
using Stripe.Checkout;

namespace dotnetshop.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IOrderDetailsRepository _orderDetailsRepository;
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IOrderHeaderRepository OrderHeaderRepository, IOrderDetailsRepository OrderDetailsRepository)
        {
            _orderHeaderRepository = OrderHeaderRepository;
            _orderDetailsRepository = OrderDetailsRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                OrderHeader = _orderHeaderRepository.Get(u => u.OrderHeaderId == orderId, includeProperties: "User"),
                OrderDetails = _orderDetailsRepository.GetAll(o => o.OrderHeaderId == orderId, includeProperties: "Product")

            };
            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateDetails()
        {
            var orderHeader = _orderHeaderRepository.Get(u => u.OrderHeaderId == OrderVM.OrderHeader.OrderHeaderId);
            orderHeader.Name = OrderVM.OrderHeader.Name;
            orderHeader.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeader.Address = OrderVM.OrderHeader.Address;
            orderHeader.City = OrderVM.OrderHeader.City;
            orderHeader.State = OrderVM.OrderHeader.State;
            orderHeader.PostalCode = OrderVM.OrderHeader.PostalCode;

            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }

            _orderHeaderRepository.Update(orderHeader);
            _orderHeaderRepository.Save();

            TempData["Message"] = "Order Details Updated Successfully";

            return RedirectToAction("Details", new { orderId = orderHeader.OrderHeaderId });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _orderHeaderRepository.UpdateStatus(OrderVM.OrderHeader.OrderHeaderId, SD.StatusInProcess);
            _orderHeaderRepository.Save();

            TempData["Message"] = "Order Processed Successfully";

            return RedirectToAction("Details", new { orderId = OrderVM.OrderHeader.OrderHeaderId });
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeader = _orderHeaderRepository.Get(u => u.OrderHeaderId == OrderVM.OrderHeader.OrderHeaderId);
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.ShippingDate = System.DateTime.Now;
            orderHeader.OrderStatus = SD.StatusShipped;

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(System.DateTime.Now.AddDays(30));
            }
            _orderHeaderRepository.Update(orderHeader);
            _orderHeaderRepository.Save();

            TempData["Message"] = "Order Shipped Successfully";

            return RedirectToAction("Details", new { orderId = OrderVM.OrderHeader.OrderHeaderId });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeadder = _orderHeaderRepository.Get(u => u.OrderHeaderId == OrderVM.OrderHeader.OrderHeaderId);
            if (orderHeadder.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeadder.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);
                _orderHeaderRepository.UpdateStatus(orderHeadder.OrderHeaderId, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _orderHeaderRepository.UpdateStatus(OrderVM.OrderHeader.OrderHeaderId, SD.StatusCancelled, SD.StatusCancelled);
            }
            _orderHeaderRepository.Save();

            TempData["Message"] = "Order Cancelled Successfully";

            return RedirectToAction("Details", new { orderId = OrderVM.OrderHeader.OrderHeaderId });
        }

        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            OrderVM.OrderHeader = _orderHeaderRepository.Get(u => u.OrderHeaderId == OrderVM.OrderHeader.OrderHeaderId, includeProperties: "User");
            OrderVM.OrderDetails = _orderDetailsRepository.GetAll(o => o.OrderHeaderId == OrderVM.OrderHeader.OrderHeaderId, includeProperties: "Product");

            var options = new SessionCreateOptions
            {
                SuccessUrl = "http://localhost:5136/Admin/Order/PaymentConfirmation?orderHeaderid=" + OrderVM.OrderHeader.OrderHeaderId,
                CancelUrl = "http://localhost:5136/Admin/Order/details?orderId=" + OrderVM.OrderHeader.OrderHeaderId,
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",

            };
            foreach (var cart in OrderVM.OrderDetails)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)cart.Price * 100,
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = cart.Product.Title,
                        }
                    },
                    Quantity = cart.Quantity
                });
            }
            var service = new SessionService();
            Session session = service.Create(options);
            _orderHeaderRepository.UpdateStripePaymentId(OrderVM.OrderHeader.OrderHeaderId, session.Id, session.PaymentIntentId);
            _orderHeaderRepository.Save();

            return Redirect(session.Url);

        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {

            OrderHeader orderHeader = _orderHeaderRepository.Get(u => u.OrderHeaderId == orderHeaderId);
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                //this is an order by company

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _orderHeaderRepository.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _orderHeaderRepository.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _orderHeaderRepository.Save();
                }


            }

            return View(orderHeaderId);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orders;

            if (User.IsInRole(SD.Role_Employee) || User.IsInRole(SD.Role_Admin))
            {
                orders = _orderHeaderRepository.GetAll(includeProperties: "User").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                orders = _orderHeaderRepository.GetAll(u => u.UserId == userId, includeProperties: "User");
            }

            switch (status)
            {
                case "pending":
                    orders = orders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orders = orders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orders = orders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orders = orders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;

            }
            return Json(new { data = orders });
        }

        #endregion

    }

}