using ASI.Basecode.Data.Models.CustomModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Policy = "SuperAdminPolicy")]
    public class SuperAdminDashboard : BaseController
    {
        public SuperAdminDashboard(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }
        public IActionResult Index()
        {
            var role = User.FindFirst("UserRole")?.Value;
            var customAdminDashoardViewModel = new CustomDashoardViewModel()
            {
                UserCount = _db.VwAdminCounts.Select(m => m.TotalAdminCount).FirstOrDefault(),
                TicketsResolvedCount = _db.VwTotalTicketsResolveds.Select(m => m.TotalTicketsResolved).FirstOrDefault()
            };
            return View(customAdminDashoardViewModel);
        }
    }
}
