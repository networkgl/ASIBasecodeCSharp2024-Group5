using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.Services.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    public class ReportController : BaseController
    {
        public ReportController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public IActionResult Index()
        {
            var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

            if (User.FindFirst("UserRole")?.Value == "administrator" || User.FindFirst("UserRole")?.Value == "superadmin")
            {
                var TicketsByCategory = _db.VwTotalTicketSummaryWithCategories.ToList();
                var TicketsByStatus = _db.VwTotalTicketSummaryWithStatuses.ToList();
                var TicketsByPriority = _db.VwTotalTicketSummaryWithPriorities.ToList();
                var agentResolvedTicket = _db.VwResolvedTicketByAgents.ToList();
                //var agentResolutionTime = _db.VwAssignedTicketViews.ToList();
                var agentCustSatRating = _db.VwCustomerSatisfactionRatings.ToList();

                var summaryModel = new ReportingAndAnalyticsModel
                {
                    TicketSummaryWithCategory = TicketsByCategory,
                    TicketSummaryWithStatus = TicketsByStatus,
                    TicketSummaryWithPriority = TicketsByPriority,
                    TicketsResolved = agentResolvedTicket,
                    //AverageResolutionTime = agentResolutionTime,
                    CustomerSatisfactionRatings = agentCustSatRating,   

                };
                return View(summaryModel);
            }
            else if (User.FindFirst("UserRole")?.Value == "support agent")
            {
                var TicketsAssignedByMeAgent = _db.VwTicketAssignedToMeAgents.Where(m => m.UserId == userId).ToList();
                var TicketsByCategory = _db.VwTicketsByCategories.Where(m => m.UserId == userId).ToList();
                var TicketsByStatus = _db.VwTicketsByStatuses.Where(m => m.UserId == userId).ToList();
                var TicketsByPriority = _db.VwTicketsByPriorities.Where(m => m.UserId == userId).ToList();

                var summaryModel = new ReportingAndAnalyticsModel
                {
                    TicketsByCategory = TicketsByCategory,
                    TicketsByStatus = TicketsByStatus,
                    TicketsByPriority = TicketsByPriority
                };

                return View(summaryModel);
            }

            return View();

        }


    }
}
