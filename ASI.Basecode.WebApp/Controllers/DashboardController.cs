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
            @ViewData["Title"] = "Dashboard";
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
            @ViewData["Title"] = "Dashboard";
            int? agentId = Convert.ToInt32(User.FindFirst("UserId")?.Value);
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

            var avgResolutionTime = _db.VwAverageResolutionTimes.Where(m => m.AgentId == agentId).FirstOrDefault()?.AvgResolutionTime ?? 0;

            int hours = (int)avgResolutionTime;
            int minutes = (int)((avgResolutionTime - hours) * 60);

            var avgFeedbackRating = _db.VwCustomerSatisfactionRatings.Where(m => m.AgentId == agentId).FirstOrDefault()?.AvgFeedbackRating ?? 0;

            var customAdminDashoardViewModel = new CustomDashoardViewModel()
            {
                UserCount = _db.VwUserRoleViews.Count(m => m.RoleId == 1),
                AgentCount = _db.VwAgentCounts.Select(m => m.TotalAgentCount).FirstOrDefault(),
                TicketsAssignedByMeCount = Convert.ToInt32(ticketAssignByMeCount?.Value ?? 0),
                TicketsResolvedCount = Convert.ToInt32(ticketsResolvedCount?.Value ?? 0),
                YourAverageResolutionTimeHours = hours,
                YourAverageResolutionTimeMins = minutes,
                YourCustomerSatisfactoryRating = avgFeedbackRating.ToString("F2"),
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
            @ViewData["Title"] = "Dashboard";
            var adminId = Convert.ToInt32(User.FindFirst("UserId")?.Value);
            var ticketAssignByMeCount = new SqlParameter("@result", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output,
            };

            await _db.Database.ExecuteSqlRawAsync("exec GetTotalTicketsYouAssigned @AssignerId = {0}, @result = {1} output", adminId, ticketAssignByMeCount);

            var customAdminDashoardViewModel = new CustomDashoardViewModel()
            {
                UserCount = _db.VwUserRoleViews.Where(m => m.RoleId == 1).ToList().Count(),
                AgentCount = _db.VwAgentCounts.Select(m => m.TotalAgentCount).FirstOrDefault(),
                AdminCount = _db.VwAdminCounts.Select(m => m.TotalAdminCount).FirstOrDefault(),
                TicketsResolvedCount = _db.VwTotalTicketsResolveds.Select(m => m.TotalTicketsResolved).FirstOrDefault(),
                TicketsAssignedByMeCount = Convert.ToInt32(ticketAssignByMeCount?.Value),
            };
            return View(customAdminDashoardViewModel);
        }

        [Authorize(Policy = "SuperAdminPolicy")]
        public IActionResult SuperAdminDashboard()
        {
            @ViewData["Title"] = "Dashboard";
            var role = User.FindFirst("UserRole")?.Value;
            var customAdminDashoardViewModel = new CustomDashoardViewModel()
            {
                UserCount = _db.VwUserRoleViews.Where(m => m.RoleId == 1).ToList().Count,
                AgentCount = _db.VwAgentCounts.Select(m => m.TotalAgentCount).FirstOrDefault(),
                AdminCount = _db.VwUserRoleViews.Where(m => m.RoleId == 3).ToList().Count,
                SuperAdminCount = _db.VwUserRoleViews.Where(m => m.RoleId == 4).ToList().Count,
                TicketsResolvedCount = _db.VwTotalTicketsResolveds.Select(m => m.TotalTicketsResolved).FirstOrDefault()
            };
            return View(customAdminDashoardViewModel);
        }
    }
}
