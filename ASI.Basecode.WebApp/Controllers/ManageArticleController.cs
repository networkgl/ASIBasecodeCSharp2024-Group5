using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
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
        public IActionResult Index(string searchTerm)
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
            var articles = _articleRepo.GetAll().ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                articles = articles.Where(a => a.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || a.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                
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
            TempData["temp"] = "create";
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
        [Authorize(Policy = "AdminAndAgentPolicy")]
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
