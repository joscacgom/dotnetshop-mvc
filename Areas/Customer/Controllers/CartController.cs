using System.Security.Claims;
using dotnetshop.Models;
using dotnetshop.Repositories;
using dotnetshop.Utils;
using dotnetshop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace dotnetshop.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IAppUserRepository _appUserRepository;
        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IOrderDetailsRepository _orderDetailsRepository;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }


        public CartController(IProductRepository productRepository, IShoppingCartRepository shoppingCartRepository, IAppUserRepository appUserRepository, IOrderHeaderRepository orderHeaderRepository, IOrderDetailsRepository orderDetailsRepository)
        {
            _productRepository = productRepository;
            _shoppingCartRepository = shoppingCartRepository;
            _appUserRepository = appUserRepository;
            _orderHeaderRepository = orderHeaderRepository;
            _orderDetailsRepository = orderDetailsRepository;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new ShoppingCartVM()
            {
                List = _shoppingCartRepository.GetAll(i => i.UserId == userId, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };

            foreach (var item in ShoppingCartVM.List)
            {
                item.Price = GetTotal(item);
                ShoppingCartVM.OrderHeader.OrderTotal += item.Price * item.Quantity;
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                List = _shoppingCartRepository.GetAll(u => u.UserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.User = _appUserRepository.Get(u => u.Id == userId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.User.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.User.PhoneNumber;
            ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.User.Address;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.User.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.User.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.User.PostalCode;



            foreach (var cart in ShoppingCartVM.List)
            {
                cart.Price = GetTotal(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Quantity;
            }
            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.List = _shoppingCartRepository.GetAll(u => u.UserId == userId,
                includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.UserId = userId;

            AppUser user = _appUserRepository.Get(u => u.Id == userId);


            foreach (var cart in ShoppingCartVM.List)
            {
                cart.Price = GetTotal(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Quantity;
            }

            if (user.CompanyId.GetValueOrDefault() == 0)
            {
                //it is a regular customer account and we need to capture payment
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                //it is a company user
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }
            _orderHeaderRepository.Insert(ShoppingCartVM.OrderHeader);
            _orderHeaderRepository.Save();
            foreach (var cart in ShoppingCartVM.List)
            {
                OrderDetails orderDetails = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.OrderHeaderId,
                    Price = cart.Price,
                    Quantity = cart.Quantity
                };
                _orderDetailsRepository.Insert(orderDetails);
                _orderDetailsRepository.Save();
            }

            if (user.CompanyId.GetValueOrDefault() == 0)
            {
                var options = new SessionCreateOptions
                {
                    SuccessUrl = "http://localhost:5136/Customer/Cart/OrderConfirmation?id=" + ShoppingCartVM.OrderHeader.OrderHeaderId,
                    CancelUrl = "http://localhost:5136/Customer/Cart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",

                };
                foreach (var cart in ShoppingCartVM.List)
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
                _orderHeaderRepository.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.OrderHeaderId, session.Id, session.PaymentIntentId);
                _orderHeaderRepository.Save();

                return Redirect(session.Url);


            }

            return RedirectToAction("OrderConfirmation", new { id = ShoppingCartVM.OrderHeader.OrderHeaderId });
        }

        public IActionResult OrderConfirmation(int id)
        {

            OrderHeader orderHeader = _orderHeaderRepository.Get(u => u.OrderHeaderId == id, includeProperties: "User");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                //this is an order by customer

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _orderHeaderRepository.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _orderHeaderRepository.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _orderHeaderRepository.Save();
                }

                HttpContext.Session.Clear();


            }

            List<ShoppingCart> shoppingCarts = _shoppingCartRepository
                .GetAll(u => u.UserId == orderHeader.UserId).ToList();

            _shoppingCartRepository.DeleteRange(shoppingCarts);
            _shoppingCartRepository.Save();
            return View(id);
        }


        public IActionResult Plus(int cartId)
        {
            ShoppingCart? cart = _shoppingCartRepository.Get(i => i.ShoppingCartId == cartId);
            cart.Quantity += 1;
            _shoppingCartRepository.Update(cart);
            _shoppingCartRepository.Save();
            TempData["Message"] = "Item added to cart successfully";
            return RedirectToAction("Index");
        }

        public IActionResult Minus(int cartId)
        {
            ShoppingCart? cart = _shoppingCartRepository.Get(i => i.ShoppingCartId == cartId);
            if (cart.Quantity == 1)
            {
                HttpContext.Session.SetInt32(SD.SessionCart, _shoppingCartRepository.GetAll(u => u.UserId == cart.UserId).Count() - 1);
                _shoppingCartRepository.Delete(cart);
                _shoppingCartRepository.Save();
                TempData["Message"] = "Item removed from cart successfully";
            }
            else
            {
                cart.Quantity -= 1;
                _shoppingCartRepository.Update(cart);
                _shoppingCartRepository.Save();
                TempData["Message"] = "Item removed from cart successfully";
            }
            return RedirectToAction("Index");
        }


        public IActionResult Remove(int cartId)
        {
            ShoppingCart? cart = _shoppingCartRepository.Get(i => i.ShoppingCartId == cartId);
            HttpContext.Session.SetInt32(SD.SessionCart, _shoppingCartRepository.GetAll(u => u.UserId == cart.UserId).Count() - 1);
            _shoppingCartRepository.Delete(cart);
            _shoppingCartRepository.Save();
            TempData["Message"] = "Item removed from cart successfully";
            return RedirectToAction("Index");
        }

        private double GetTotal(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Quantity <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else if (shoppingCart.Quantity > 50 && shoppingCart.Quantity <= 100)
            {
                return shoppingCart.Product.Price50;
            }
            else
            {
                return shoppingCart.Product.Price100;
            }
        }
    }
}