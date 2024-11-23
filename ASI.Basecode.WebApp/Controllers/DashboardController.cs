using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.Services.Controllers;
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
    [Authorize]
    public class DashboardController : BaseController
    {
        public DashboardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }
        [Authorize(Policy ="UserPolicy")]
        public async Task<IActionResult> UserDashboard()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return BadRequest();
            }

            var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

            var totalTicketsCreatedByMeParam = new SqlParameter("@result", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            await _db.Database.ExecuteSqlRawAsync("exec GetTotalTicketsCreatedByMe @UserId = {0}, @result = {1} output", userId, totalTicketsCreatedByMeParam);

            var unresolvedTicketsParam = new SqlParameter("@result", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            await _db.Database.ExecuteSqlRawAsync("exec GetTotalTicketsNotResolvedForReporter @UserId = {0}, @result = {1} output", userId, unresolvedTicketsParam);

            var resolvedTicketsParam = new SqlParameter("@result", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            await _db.Database.ExecuteSqlRawAsync("exec GetTotalTicketsResolvedForReporter @UserId = {0}, @result = {1} output", userId, resolvedTicketsParam);

            var customAdminDashboardViewModel = new CustomDashoardViewModel()
            {
                TotalTicketCreatedByMe = Convert.ToInt32(totalTicketsCreatedByMeParam.Value),
                ResolvedTicketForReporterCount = Convert.ToInt32(resolvedTicketsParam.Value),
                UnresolvedTicketForReporterCount = Convert.ToInt32(unresolvedTicketsParam.Value)
            };

            return View(customAdminDashboardViewModel);
        }

        [Authorize(Policy = "SupportAgentPolicy")]
        public async Task<IActionResult> AgentDashboard()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return BadRequest();
            }

            int agentId = Convert.ToInt32(User.FindFirst("UserId")?.Value);
            var ticketsResolvedCount = new SqlParameter("@result", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            await _db.Database.ExecuteSqlRawAsync("exec GetMyTotalTicketsResolved @agentId = {0}, @result = @result output", agentId, ticketsResolvedCount);

            var ticketAssignByMeCount = new SqlParameter("@result", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output,
            };

            await _db.Database.ExecuteSqlRawAsync("exec GetTotalTicketsYouAssigned @AssignerId = {0}, @result = {1} output", agentId, ticketAssignByMeCount);

            var customAdminDashoardViewModel = new CustomDashoardViewModel()
            {
                UserCount = _db.VwUserRoleViews.Where(m => m.RoleId == 1).ToList().Count,
                AgentCount = _db.VwAgentCounts.Select(m => m.TotalAgentCount).FirstOrDefault(),
                TicketsAssignedByMeCount = Convert.ToInt32(ticketAssignByMeCount.Value),
                TicketsResolvedCount = Convert.ToInt32(ticketsResolvedCount.Value),
            };

            return View(customAdminDashoardViewModel);
        }

        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> AdminDashboard()
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

        [Authorize(Policy = "SuperAdminPolicy")]
        public IActionResult SuperAdminDashboard()
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
