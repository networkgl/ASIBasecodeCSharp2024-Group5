using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.Services.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Controllers
{
    public class ManageArticleController : BaseController
    {
        public ManageArticleController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }
        [Authorize(Policy = "AllRoleTypePolicy")]
        public IActionResult Index(string searchTerm, string sortBy)
        {
            if (TempData["temp"] is not null)
            {
                if ((string)TempData["temp"] == "create")
                {
                    var resMsg = JsonConvert.DeserializeObject<AlertMessageContent>(TempData["ResMsg"].ToString());

                    if (resMsg is not null)
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                            {
                                Status = resMsg.Status,
                                Message = resMsg.Message
                            });
                        }
                    }
                }
                if ((string)TempData["temp"] == "delete")
                {
                    if (TempData["status"] as int? == 0)
                    {
                        TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                        {
                            Status = ErrorCode.Success,
                            Message = "A ticket has deleted successfully!"
                        });
                    }
                    else
                    {
                        TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                        {
                            Status = ErrorCode.Error,
                            Message = "An error has occured upon deleting the ticket."
                        });
                    }
                }

                if ((string)TempData["temp"] == "update")
                {
                    var resMsg = JsonConvert.DeserializeObject<AlertMessageContent>(TempData["ResMsg"].ToString());

                    if (resMsg is not null)
                    {
                        TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                        {
                            Status = resMsg.Status,
                            Message = resMsg.Message
                        });
                    }
                }
            }
            ViewData["TitleSortParm"] = String.IsNullOrEmpty(sortBy) ? "title_desc" : "";
            var articles = _articleRepo.GetAll().ToList();
            switch (sortBy)
            {
                case "title_desc":
                    articles = articles.OrderByDescending(u => u.Title).ToList();
                    break;
                default:
                    articles = articles.OrderBy(u => u.Title).ToList();
                    break;

            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                articles = articles.Where(a => a.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || a.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).OrderByDescending(m => m.ArticleId).ToList();
            }
            return View(articles);
        }

        [HttpGet]
        [Authorize(Policy = "AdminAndAgentPolicy")]
        public IActionResult Create()
        {
            return View(new Article());
        }

        [HttpPost]
        [Authorize(Policy = "AdminAndAgentPolicy")]
        public IActionResult Create(Article article)
        {
            article.UserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (string.IsNullOrEmpty(article.Content))
            {
                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Please provide the article content."
                });
                return View(article);
            }

            if (article == null)
            {
                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "An error has occurred upon creating the article."
                });
                return View(article);
            }

            var result = _articleRepo.Create(article);

            TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
            {
                Status = result == ErrorCode.Success ? ErrorCode.Success : ErrorCode.Error,
                Message = result == ErrorCode.Success ? "Article created successfully!" : "An error has occurred upon creating the article."
            });

            return RedirectToAction("Index");
        }

        [Authorize(Policy = "AdminAndAgentPolicy")]
        public IActionResult Edit(int id)
        {
            var article = _articleRepo.Get(id);

            if (article == null)
            {
                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Article not found."
                });
                return RedirectToAction("Index");
            }

            return View(article);
        }

        [HttpPost]
        [Authorize(Policy = "AdminAndAgentPolicy")]
        public IActionResult Edit(Article article)
        {
            TempData["temp"] = "update";
            if (!ModelState.IsValid)
            {
                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Message = "There was an error in your submission.",
                    Status = ErrorCode.Error
                });
                return View(article);
            }

            var result = _articleRepo.Update(article.ArticleId, article);

            if(result == ErrorCode.Success)
            {
                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Message = "Updated Successfully!",
                    Status = ErrorCode.Success
                });
            } else
            {
                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Message = "An error occurred while updating the article.",
                    Status = ErrorCode.Error
                });
            }

            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            var article = _articleRepo.Get(id);
            return View(article);
        }

        [Authorize(Policy = "AdminAndAgentPolicy")]
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
