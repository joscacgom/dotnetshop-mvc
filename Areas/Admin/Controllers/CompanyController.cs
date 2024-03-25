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
    public class CompanyController : Controller
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyController(ICompanyRepository CompanyRepository)
        {
            _companyRepository = CompanyRepository;
        }

        public IActionResult Index()
        {
            List<Company> companies = _companyRepository.GetAll().ToList();

            return View(companies);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, ActionName("Create")]
        public IActionResult CreateCompany(Company company)
        {
            if (ModelState.IsValid)
            {
                _companyRepository.Insert(company);
                _companyRepository.Save();
                TempData["message"] = "Company has been added successfully";
                return RedirectToAction("Index");
            }
            else
            {

                return View(company);
            }
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Company? company = _companyRepository.GetById((int)id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        [HttpPost]
        public IActionResult Edit(Company company)
        {
            if (ModelState.IsValid)
            {
                _companyRepository.Update(company);
                _companyRepository.Save();
                TempData["message"] = "Company has been updated successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View(company);
            }
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> companies = _companyRepository.GetAll().ToList();
            return Json(new { data = companies });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            Company? company = _companyRepository.GetById(id);
            if (company == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _companyRepository.Delete(company);
            _companyRepository.Save();
            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion

    }

}