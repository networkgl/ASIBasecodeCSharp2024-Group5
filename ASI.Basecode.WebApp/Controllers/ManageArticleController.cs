using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models.CustomModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;

namespace ASI.Basecode.WebApp.Controllers
{
    public class ManageArticleController : BaseController
    {
        public IActionResult Index()
        {
            if (TempData["temp"] != null)
            {
                if (TempData["status"] as int? == 0)
                {
                    TempData["ResMsg"] = new AlertMessageContent()
                    {
                        Status = ErrorCode.Success,
                        Message = "An article has deleted successfully!"
                    };
                }
                else
                {
                    TempData["ResMsg"] = new AlertMessageContent()
                    {
                        Status = ErrorCode.Error,
                        Message = "An error has occured upon deleting the article."
                    };
                }
            }
            if (User.Identity.IsAuthenticated)
            {
                var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);
                var myTickets = _db.VwUserTicketViews.Where(m => m.UserId == userId).ToList();

                return View(myTickets);
            }
            return NotFound();
        }


    }
}
