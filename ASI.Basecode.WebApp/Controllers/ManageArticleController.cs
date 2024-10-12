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
    [Authorize(Policy = "AdminAndAgentPolicy")]
    public class ManageArticleController : BaseController
    {
        public ManageArticleController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }
        public IActionResult Index(string searchTerm)
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
                articles = articles.Where(a => a.Content.Contains(searchTerm)).ToList();
            }
            return View(articles);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Article());
        }

        [HttpPost]
        public IActionResult Create(Article article)
        {

            article.UserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (string.IsNullOrEmpty(article.Content))
            {
                TempData["ResMsg"] = "Please provide the article content.";
                TempData["ResStatus"] = "Error";
                return View(article);
            }

            if (article == null)
            {
                TempData["ResMsg"] = "An error has occurred upon creating the article.";
                TempData["ResStatus"] = "Error";
                return View(article);
            }

            var result = _articleRepo.Create(article);
            TempData["ResMsg"] = result == ErrorCode.Success ? "Article created successfully!" : "An error occurred while creating the article.";
            TempData["ResStatus"] = result == ErrorCode.Success ? "Success" : "Error";

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var article = _articleRepo.Get(id);

            if (article == null)
            {

                TempData["ResMsg"] = new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Article not found."
                };
                return RedirectToAction("Index");
            }

            return View(article);
        }

        [HttpPost]
        public IActionResult Edit(Article article)
        {
            if (!ModelState.IsValid)
            {
                TempData["ResMsg"] = "There was an error in your submission.";
                TempData["ResStatus"] = ErrorCode.Error.ToString();
                return View(article);
            }

            var result = _articleRepo.Update(article.ArticleId, article);
            TempData["ResMsg"] = result == ErrorCode.Success ? "Updated Successfully!" : "An error occurred while updating the article.";
            TempData["ResStatus"] = result == ErrorCode.Success ? ErrorCode.Success.ToString() : ErrorCode.Error.ToString();
            return RedirectToAction("Index");
        }


        public IActionResult Details(int id)
        {
            var article = _articleRepo.Get(id);
            return View(article);
        }

        public IActionResult Delete(int id)
        {
            var result = _articleRepo.Delete(id);


            if (result == ErrorCode.Success)
            {
                TempData["ResMsg"] = "Article deleted successfully!";
                TempData["ResStatus"] = "success";
            }
            else
            {
                TempData["ResMsg"] = "Failed to delete the article. Please try again.";
                TempData["ResStatus"] = "error";
            }

            return RedirectToAction("Index");
        }
    }
}
