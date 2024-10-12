using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Policy = "UserPolicy")]
    public class ArticleController : BaseController
    {
        public ArticleController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }
        public IActionResult List(string searchTerm)
        {
            if (TempData["temp"] != null)
            {
                if (TempData["status"] as int? == 0)
                {
                    TempData["ResMsg"] = new AlertMessageContent()
                    {
                        Status = ErrorCode.Success,
                        Message = "An article has been deleted successfully!"
                    };
                }
                else
                {
                    TempData["ResMsg"] = new AlertMessageContent()
                    {
                        Status = ErrorCode.Error,
                        Message = "An error has occurred upon deleting the article."
                    };
                }
            }
            var articles = _articleRepo.GetAll().ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                articles = articles.Where(a => a.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || a.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            return View(articles);
        }

    }
}
