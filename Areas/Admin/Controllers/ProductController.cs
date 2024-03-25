using Microsoft.AspNetCore.Mvc;
using dotnetshop.Models;
using dotnetshop.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using dotnetshop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using dotnetshop.Utils;

namespace dotnetshop.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository ProductRepository, ICategoryRepository CategoryRepository)
        {
            _productRepository = ProductRepository;
            _categoryRepository = CategoryRepository;
        }

        public IActionResult Index()
        {
            List<Product> products = _productRepository.GetAll(includeProperties: "Category").ToList();

            return View(products);
        }

        public IActionResult Create()
        {
            IEnumerable<SelectListItem> categoryList = _categoryRepository.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.CategoryId.ToString()
            });
            ProductVM ProductVM = new ProductVM
            {
                Product = new Product(),
                CategoryList = categoryList
            };
            return View(ProductVM);
        }

        [HttpPost, ActionName("Create")]
        public IActionResult CreateProduct(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                Category? c = _categoryRepository.GetById(productVM.Product.CategoryId);
                _productRepository.Insert(productVM.Product);
                _productRepository.Save();
                TempData["message"] = "Product has been added successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _categoryRepository.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.CategoryId.ToString()
                });

                return View(productVM);
            }
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product? product = _productRepository.GetById((int)id);
            if (product == null)
            {
                return NotFound();
            }
            IEnumerable<SelectListItem> categoryList = _categoryRepository.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.CategoryId.ToString()
            });
            ProductVM productVM = new ProductVM
            {
                Product = product,
                CategoryList = categoryList
            };
            return View(productVM);
        }

        [HttpPost]
        public IActionResult Edit(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                _productRepository.Update(productVM.Product);
                _productRepository.Save();
                TempData["message"] = "Product has been updated successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _categoryRepository.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.CategoryId.ToString()
                });

                return View(productVM);
            }
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> products = _productRepository.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = products });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            Product? product = _productRepository.GetById(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _productRepository.Delete(product);
            _productRepository.Save();
            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion

    }

}