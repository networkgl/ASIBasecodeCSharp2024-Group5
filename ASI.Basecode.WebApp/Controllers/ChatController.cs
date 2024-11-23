using ASI.Basecode.Services.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Policy = "AllRoleTypePolicy")]
    public class ChatController:BaseController
    {
        public ChatController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public IActionResult Index()
        {
            var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);


            return View();
        }
    }
}
