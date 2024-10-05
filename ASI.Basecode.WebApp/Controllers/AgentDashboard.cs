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
    [Authorize(Policy =("SupportAgentPolicy"))]
    public class AgentDashboard : BaseController
    {
        public AgentDashboard(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }
        public async Task<IActionResult> Index()
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
                UserCount = _db.VwUserCounts.Select(m => m.TotalUserCount).FirstOrDefault(),
                AgentCount = _db.VwAgentCounts.Select(m => m.TotalAgentCount).FirstOrDefault(),
                TicketsAssignedByMeCount = Convert.ToInt32(ticketAssignByMeCount.Value),
                TicketsResolvedCount = Convert.ToInt32(ticketsResolvedCount.Value),
            };

            return View(customAdminDashoardViewModel);
        }
    }
}
