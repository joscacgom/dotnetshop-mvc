using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using dotnetshop.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace dotnetshop.ViewModels
{
    public class ProductVM
    {
      public Product Product { get; set; }
      [ValidateNever]
      public IEnumerable<SelectListItem> CategoryList { get; set; }

    }
}
