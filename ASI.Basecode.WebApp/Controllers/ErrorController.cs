using Microsoft.AspNetCore.Mvc;

namespace ASI.Basecode.WebApp.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Error404()
        {
            return View();
        }
        public IActionResult Error403()
        {
            //ViewBag.ReturnUrl = returnUrl;
            return View();
        }
    }
}
