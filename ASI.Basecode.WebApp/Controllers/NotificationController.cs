using ASI.Basecode.Data.Models;
using ASI.Basecode.WebApp.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Controllers
{
    public class NotificationController : NotificationManager
    {
        public NotificationController()
        {
        }
        public IActionResult List()
        {
            List<Notification> retValNotif = null;

            if (User.Identity.IsAuthenticated)
            {
                if (int.TryParse(User.FindFirstValue("UserId"), out int userId)) 
                {
                    retValNotif = GetUserAssociatedNotif(userId);
                }
            }

            return View(retValNotif);
        }

    }
}
