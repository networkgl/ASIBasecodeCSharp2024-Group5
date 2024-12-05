using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Controllers;
//using ASI.Basecode.WebApp.Data.Models;
using ASI.Basecode.WebApp.Utils;

//using ASI.Basecode.WebApp.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    public class ManageArticleController : BaseController
    {
        public ManageArticleController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }
        [Authorize(Policy = "AllRoleTypePolicy")]
        public IActionResult Index(string searchTerm, string sortBy, int? ArticleId)
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
        
            var articles = _db.VwApprovedArticles.ToList();
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
            else if(ArticleId != null)
            {
                var getArticleId = _articleRepo.Get(ArticleId);
                if (getArticleId != null)
                {
                    articles = articles.Where(m => m.ArticleId == ArticleId).ToList();
                }
            }

            var pendingList = _db.VwNeedApprovalArticles.Count();

            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst("UserRole")?.Value;
            if (userRole == "support agent")
            {
                pendingList = _db.VwNeedApprovalArticles.Where(a => a.UserId == userId).Count();
            }


            ViewData["PendingList"] = pendingList;

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

            article.DateCreated = DateTimeToday();

            var userRole = User.FindFirst("UserRole")?.Value;
            if (userRole == "administrator")
            {
                article.Approved = "Yes";
            }
            else
            {
                article.Approved = "No";
            }

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

            //Notification Here...
            if (result == ErrorCode.Success)
            {
                if (User.IsInRole("administrator")) //if admin then just send notif to his/her self only
                {
                    //Notify Administrator who created...
                    var notifAdmin = new Notification()
                    {
                        ToUserId = article.UserId,
                        ArticleId = article.ArticleId,
                        Content = $"You have created a new article. Article ID: {article.ArticleId}. User is able to view it already.",
                        CreatedAt = DateTimeToday()
                    };


                    if (_notifRepo.Create(notifAdmin) == ErrorCode.Error)
                    {
                        TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                        {
                            Status = ErrorCode.Error,
                            Message = "An error occured while inserting notification for notify admin create article"
                        });
                    }
                }
                else if(User.IsInRole("support agent"))
                {
                    int[] getAllAdminId = null;

                    using (var db = new AssisthubDBContext())
                    {
                        //getting all userId for Adminstrator accounts..
                        //anyone can perform approval..
                        getAllAdminId = db.VwUserRoleViews.Where(m => m.RoleId == (byte)RoleType.Administrator).Select(m => m.UserId).ToArray();
                    }

                    int i = 0;

                    do
                    {
                        //Notify administrator...
                        var notifAdmin = new Notification()
                        {
                            FromUserId = article.UserId,
                            ToUserId = getAllAdminId[i],
                            ArticleId = article.ArticleId,
                            Content = $"Support Agent created a new article. Article ID: {article.ArticleId}. Please review it and take necessarry actions.",
                            CreatedAt = DateTimeToday()
                        };

                        if (_notifRepo.Create(notifAdmin) == ErrorCode.Success)
                        {
                            i++;
                        }
                        else
                        {
                            TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                            {
                                Status = ErrorCode.Error,
                                Message = "An error occured while inserting notification for notify supp agent create article"
                            });

                            return RedirectToAction("Index");
                        }

                        if ((i + 1) == getAllAdminId.Length)
                        {
                            break;
                        }

                    } while (true);


                    //Notify Supp Agent who created...
                    var notifSuppAgent = new Notification()
                    {
                        ToUserId = article.UserId,
                        ArticleId = article.ArticleId,
                        Content = $"You have created a new article. Administrator will review it shortly. Article ID: {article.ArticleId}. You'll be notified once your created article is approved.",
                        CreatedAt = DateTimeToday()
                    };

                    if (_notifRepo.Create(notifSuppAgent) == ErrorCode.Error)
                    {
                        TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                        {
                            Status = ErrorCode.Error,
                            Message = "An error occured while inserting notification for notify supp agent create article"
                        });
                    }
                }


                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = result == ErrorCode.Success ? ErrorCode.Success : ErrorCode.Error,
                    Message = result == ErrorCode.Success ? "Article created successfully!" : "An error has occurred upon creating the article."
                });
            }

            //TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
            //{
            //    Status = result == ErrorCode.Success ? ErrorCode.Success : ErrorCode.Error,
            //    Message = result == ErrorCode.Success ? "Article created successfully!" : "An error has occurred upon creating the article."
            //});


            return RedirectToAction("Index");
        }

        [Authorize(Policy = "AdminAndAgentPolicy")]
        public IActionResult PendingList(string searchTerm, string sortBy, int? ArticleId, int? NotificationId)
        {
            if(TempData["ResMsg"] is not null)
            {
                var resMsg = TempData["ResMsg"] as string;
                var alertMessage = JsonConvert.DeserializeObject<AlertMessageContent>(resMsg);

                ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = alertMessage.Status,
                    Message = alertMessage.Message,
                });
            }

            ViewData["Title"] = "Pending Articles";

            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var articles = _db.VwNeedApprovalArticles.ToList();

            var userRole = User.FindFirst("UserRole")?.Value;
            if (userRole == "support agent")
            {
                articles = articles.Where(a => a.UserId == userId).ToList();
            }

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
            else if (ArticleId != null && NotificationId != null) //For redirection from notification
            {
                articles = articles.Where(m => m.ArticleId == ArticleId).ToList();

                if (articles != null)
                {
                    //update notif mark as read if this route is visited from notification view
                    if (NotificationId != null)
                    {
                        var getNotifById = _notifRepo.Get(NotificationId);

                        if (getNotifById != null)
                        {
                            getNotifById.IsRead = (byte)Enums.NotifStatus.HasRead;

                            if (_notifRepo.Update(getNotifById.NotificationId, getNotifById) == ErrorCode.Error)
                            {
                                //Possible error internal upon updating if there is
                                return BadRequest();//temporary return...
                            };
                        }

                    }
                }
            }

            

            return View(articles);
        }


        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Approve(int id)
        {
            var article = _articleRepo.Get(id);

            if (article == null)
            {
                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Article not found."
                });
                return RedirectToAction("PendingList");
            }

            article.Approved = "Yes";

            var result = _articleRepo.Update(id, article);
            int? toUserId = null;
            string content = null;
            if (article.UserId == null)  // Always basing on the last updated if not null
            {
                toUserId = article.UpdatedBy;
                content = $"The article you've been updated has been approved by the administrator and already visible to users. Article ID: {article.ArticleId}.";
            }
            else
            {
                toUserId = article.UserId;
                content = $"Your article has been approved by the administrator and already visible to users. Article ID: {article.ArticleId}.";
            }

            if (result == ErrorCode.Success)
            {
                //Notify Supp Agent who trigger the article either being updated or just newly created...
                var notifSuppAgent = new Notification()
                {
                    ToUserId = toUserId,
                    ArticleId = article.ArticleId,
                    Content = content,
                    CreatedAt = DateTimeToday()
                };

                if (_notifRepo.Create(notifSuppAgent) == ErrorCode.Error)
                {
                    TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                    {
                        Status = ErrorCode.Error,
                        Message = "An error occured while inserting notification for notify supp agent approved article"
                    });
                }

                //Holds the userId of Admin who currently login
                int.TryParse(User.FindFirstValue("UserId"), out int userId);

                //Notify Administrator who approves the article ...
                var notifAdministrator = new Notification()
                {
                    ToUserId = userId,
                    ArticleId = article.ArticleId,
                    Content = $"You have successfully approved an article. Article ID: {article.ArticleId}. It's already visible to user now.",
                    CreatedAt = DateTimeToday()
                };

                if (_notifRepo.Create(notifAdministrator) == ErrorCode.Error)
                {
                    TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                    {
                        Status = ErrorCode.Error,
                        Message = "An error occured while inserting notification for notify supp agent approved article"
                    });
                }



                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Success,
                    Message = "Article approved successfully!"
                });
            }
            else
            {
                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "An error occurred while approving the article."
                });
            }

            return RedirectToAction("PendingList");
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
            article.DateUpdated = DateTimeToday();
            var userRole = User.FindFirst("UserRole")?.Value;
            if (userRole == "administrator")
            {
                article.Approved = "Yes";
            }
            else
            {
                article.Approved = "No";
            }

            article.UpdatedBy = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);


            if (User.IsInRole("support agent"))
            {
                int[] getAllAdminId = null;

                using (var db = new AssisthubDBContext())
                {
                    //getting all userId for Adminstrator accounts..
                    //anyone can perform approval..
                    getAllAdminId = db.VwUserRoleViews.Where(m => m.RoleId == (byte)RoleType.Administrator).Select(m => m.UserId).ToArray();
                }

                int i = 0;

                do
                {
                    //Notify administrator...
                    var notifAdmin = new Notification()
                    {
                        FromUserId = article.UserId,
                        ToUserId = getAllAdminId[i],
                        ArticleId = article.ArticleId,
                        Content = $"Support Agent updated an existing article. Article ID: {article.ArticleId}. Please review it and take necessarry actions.",
                        CreatedAt = DateTimeToday()
                    };

                    if (_notifRepo.Create(notifAdmin) == ErrorCode.Success)
                    {
                        i++;
                    }
                    else
                    {
                        TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                        {
                            Status = ErrorCode.Error,
                            Message = "An error occured while inserting notification for notify supp agent create article"
                        });

                        return RedirectToAction("Index");
                    }

                    if ((i + 1) == getAllAdminId.Length)
                    {
                        break;
                    }

                } while (true);

                //Notify Supp Agent who trigger the article either being updated or just newly created...
                var notifSuppAgent = new Notification()
                {
                    ToUserId = article.UpdatedBy,
                    ArticleId = article.ArticleId,
                    Content = $"You are trying to update an existing article. Article ID: {article.ArticleId}. Administrator will review it shortly. You will be notified for the update.",
                    CreatedAt = DateTimeToday()
                };

                if (_notifRepo.Create(notifSuppAgent) == ErrorCode.Error)
                {
                    TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                    {
                        Status = ErrorCode.Error,
                        Message = "An error occured while inserting notification for notify supp agent update article"
                    });
                }
            }


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
            } 
            else
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

            var currentUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
            return Redirect(currentUrl);
        }
    }
}
