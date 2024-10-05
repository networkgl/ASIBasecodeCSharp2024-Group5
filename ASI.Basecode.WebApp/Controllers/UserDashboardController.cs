using ASI.Basecode.Data.Models.CustomModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Policy = "UserPolicy")]
    public class UserDashboardController : BaseController
    {
        public UserDashboardController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }
        public async Task<IActionResult> Index()
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
    }
}
