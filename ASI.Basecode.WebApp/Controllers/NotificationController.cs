using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Repository;
using ASI.Basecode.WebApp.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Policy = "AdminAgentUserPolicy")]
    public class NotificationController : NotificationManager
    {
        public NotificationController()
        {
        }
        public IActionResult List(byte? option)
        {
            List<Notification> retValNotif = null;

            if (User.Identity.IsAuthenticated)
            {
                if (int.TryParse(User.FindFirstValue("UserId"), out int userId)) 
                {
                    if (option != null && option != (byte)NotifStatus.All) //then do filtering...
                    {
                        if (option == (byte)NotifStatus.HasRead)
                        {
                            TempData["hasRead"] = (byte)NotifStatus.HasRead;
                            retValNotif = GetUserAssociatedNotif(userId).Where(m => m.IsRead == (byte)NotifStatus.HasRead).ToList();
                        }
                        else if (option == (byte)NotifStatus.NotRead)
                        {
                            TempData["hasRead"] = (byte)NotifStatus.NotRead;
                            retValNotif = GetUserAssociatedNotif(userId).Where(m => m.IsRead == (byte)NotifStatus.NotRead || m.IsRead == null).ToList();
                            TempData["totalCountNotRead"] = retValNotif.Count;
                        }
                    }
                    else
                    {
                        retValNotif = GetUserAssociatedNotif(userId);
                    }

                    //Always get count for reference in the view.
                    var notReadNotifCount = GetUserAssociatedNotif(userId).Where(m => m.IsRead == (byte)NotifStatus.NotRead || m.IsRead == null).ToList().Count;
                    TempData["totalCountNotRead"] = notReadNotifCount;
                }
            }

            return View(retValNotif);
        }

        [HttpPost]
        public IActionResult NotifStatusFilter(byte? option)
        {
            return RedirectToAction("List","Notification", new { option } );
        }

        [HttpPost]
        public IActionResult MarkAllAsRead(byte? value)
        {
            string errorMsg, successMsg = string.Empty;

            if (int.TryParse(User.FindFirstValue("UserId"), out int userId))
            {
                if (MarkUserNotifAllAsRead(userId, value, out errorMsg, out successMsg) == ErrorCode.Error)
                {
                    TempData["ErrorOrExcetion"] = errorMsg;
                }
            }

            TempData["SuccessMsg"] = successMsg;
            return RedirectToAction("List", "Notification");
        }
    }
}
