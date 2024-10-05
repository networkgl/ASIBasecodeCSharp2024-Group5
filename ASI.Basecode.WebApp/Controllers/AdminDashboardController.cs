using ASI.Basecode.Data.Models.CustomModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    public class AdminDashboardController : BaseController
    {

        public AdminDashboardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return BadRequest();
            }

            var adminId = Convert.ToInt32(User.FindFirst("UserId")?.Value);
            var ticketAssignByMeCount = new SqlParameter("@result", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output,
            };

            await _db.Database.ExecuteSqlRawAsync("exec GetTotalTicketsYouAssigned @AssignerId = {0}, @result = {1} output", adminId, ticketAssignByMeCount);

            var customAdminDashoardViewModel = new CustomDashoardViewModel()
            {
                UserCount = _db.VwUserCounts.Select(m => m.TotalUserCount).FirstOrDefault(),
                AgentCount = _db.VwAgentCounts.Select(m => m.TotalAgentCount).FirstOrDefault(),
                TicketsResolvedCount = _db.VwTotalTicketsResolveds.Select(m => m.TotalTicketsResolved).FirstOrDefault(),
                TicketsAssignedByMeCount = Convert.ToInt32(ticketAssignByMeCount?.Value),
            };
            return View(customAdminDashoardViewModel);
        }
    }
}
