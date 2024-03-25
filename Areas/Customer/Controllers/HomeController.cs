using Microsoft.AspNetCore.Mvc;
using dotnetshop.Models;
using dotnetshop.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using dotnetshop.Utils;

namespace dotnetshop.Controllers;

[Area("Customer")]
public class HomeController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly IShoppingCartRepository _shoppingCartRepository;

    public HomeController(IProductRepository productRepository, IShoppingCartRepository shoppingCartRepository)
    {
        _productRepository = productRepository;
        _shoppingCartRepository = shoppingCartRepository;
    }

    public IActionResult Index()
    {
        IEnumerable<Product> products = _productRepository.GetAll(includeProperties: "Category");
        return View(products);
    }

    public IActionResult Details(int id)
    {
        ShoppingCart cart = new ShoppingCart();
        Product? product = _productRepository.Get(i => i.ProductId == id, includeProperties: "Category");
        if (product == null)
        {
            return NotFound();
        }
        cart.Product = product;
        cart.ProductId = product.ProductId;
        cart.Quantity = 1;
        return View(cart);
    }

    [HttpPost]
    [Authorize]
    public IActionResult Details(ShoppingCart cart)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCart? cartFromDb = _shoppingCartRepository.Get(
            i => i.ProductId == cart.ProductId && i.UserId == userId);
        cart.UserId = userId;
        if (cartFromDb == null)
        {
            _shoppingCartRepository.Insert(cart);
            _shoppingCartRepository.Save();
            HttpContext.Session.SetInt32(SD.SessionCart, _shoppingCartRepository.GetAll(i => i.UserId == userId).Count());
        }
        else
        {
            cartFromDb.Quantity += cart.Quantity;
            _shoppingCartRepository.Update(cartFromDb);
            _shoppingCartRepository.Save();
        }
        TempData["Message"] = "Item added to cart successfully";

        return RedirectToAction("Index");
    }

    public IActionResult Privacy()
    {
        return View();
    }

}
