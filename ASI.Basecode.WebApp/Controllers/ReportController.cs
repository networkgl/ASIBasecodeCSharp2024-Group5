using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.Services.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    public class ReportController : BaseController
    {
        public ReportController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public IActionResult Index(DateTime? startDate, DateTime? endDate)
        {

            var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

            if (User.FindFirst("UserRole")?.Value == "administrator" || User.FindFirst("UserRole")?.Value == "superadmin")
            {
                List<VwTotalTicketSummaryWithCategory> TicketsByCategory;
                List<VwTotalTicketSummaryWithStatus> TicketsByStatus;
                List<VwTotalTicketSummaryWithPriority> TicketsByPriority;
                List<VwResolvedTicketByAgent> agentResolvedTicket;
                List<VwCustomerSatisfactionRating> agentCustSatRating;

                if (startDate == null && endDate == null)
                {
                    TicketsByCategory = _db.VwTotalTicketSummaryWithCategories
                    .GroupBy(x => x.CategoryName)
                    .Select(g => new VwTotalTicketSummaryWithCategory
                    {
                        CategoryName = g.Key,
                        TotalCount = g.Sum(x => x.TotalCount) 
                    })
                    .ToList();

                    TicketsByStatus = _db.VwTotalTicketSummaryWithStatuses
                        .GroupBy(x => x.StatusName)
                        .Select(g => new VwTotalTicketSummaryWithStatus
                        {
                            StatusName = g.Key,
                            TotalCount = g.Sum(x => x.TotalCount) // Assuming TotalCount is the field you want to sum
                        })
                        .ToList();

                    TicketsByPriority = _db.VwTotalTicketSummaryWithPriorities
                        .GroupBy(x => x.PriorityName)
                        .Select(g => new VwTotalTicketSummaryWithPriority
                        {
                            PriorityName = g.Key,
                            TotalCount = g.Sum(x => x.TotalCount) // Assuming TotalCount is the field you want to sum
                        })
                        .ToList();
                    agentResolvedTicket = _db.VwResolvedTicketByAgents
                        .GroupBy(x => x.AgentName)
                        .Select(g => new VwResolvedTicketByAgent
                        {
                            AgentName = g.Key,
                            TotalResolvedCount = g.Sum(x => x.TotalResolvedCount) // Assuming TotalCount is the field you want to sum
                        })
                        .ToList();
                    //var agentResolutionTime = _db.VwAssignedTicketViews.ToList();
                    agentCustSatRating = _db.VwCustomerSatisfactionRatings
                        .GroupBy(x => x.AgentName)
                        .Select(g => new VwCustomerSatisfactionRating
                        {
                            AgentName = g.Key,
                            AvgFeedbackRating = g.Average(x => x.AvgFeedbackRating) // Assuming TotalCount is the field you want to sum
                        })
                        .ToList();
                }
                else
                {
                    TicketsByCategory = _db.VwTotalTicketSummaryWithCategories
                        .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate)
                        .GroupBy(x => x.CategoryName)
                        .Select(g => new VwTotalTicketSummaryWithCategory
                        {
                            CategoryName = g.Key,
                            TotalCount = g.Sum(x => x.TotalCount) // Assuming TotalCount is the field you want to sum
                        })
                        .ToList();

                     TicketsByStatus = _db.VwTotalTicketSummaryWithStatuses
                        .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate)
                        .GroupBy(x => x.StatusName)
                        .Select(g => new VwTotalTicketSummaryWithStatus
                        {
                            StatusName = g.Key,
                            TotalCount = g.Sum(x => x.TotalCount) // Assuming TotalCount is the field you want to sum
                        })
                        .ToList();
                    TicketsByPriority = _db.VwTotalTicketSummaryWithPriorities
                        .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate)
                        .GroupBy(x => x.PriorityName)
                        .Select(g => new VwTotalTicketSummaryWithPriority
                        {
                            PriorityName = g.Key,
                            TotalCount = g.Sum(x => x.TotalCount) // Assuming TotalCount is the field you want to sum
                        })
                        .ToList();
                    agentResolvedTicket = _db.VwResolvedTicketByAgents
                        .Where(x => x.ResolvedAt >= startDate && x.ResolvedAt <= endDate)
                        .GroupBy(x => x.AgentName)
                        .Select(g => new VwResolvedTicketByAgent
                        {
                            AgentName = g.Key,
                            TotalResolvedCount = g.Sum(x => x.TotalResolvedCount) // Assuming TotalCount is the field you want to sum
                        })
                        .ToList();
                    //var agentResolutionTime = _db.VwAssignedTicketViews.ToList();
                    agentCustSatRating = _db.VwCustomerSatisfactionRatings
                        .Where(x => x.FeedbackedAt >= startDate && x.FeedbackedAt <= endDate)
                        .GroupBy(x => x.AgentName)
                        .Select(g => new VwCustomerSatisfactionRating
                        {
                            AgentName = g.Key,
                            AvgFeedbackRating = g.Average(x => x.AvgFeedbackRating) // Assuming TotalCount is the field you want to sum
                        })
                        .ToList();
                }

                if (!TicketsByCategory.Any() || !TicketsByStatus.Any() || !TicketsByPriority.Any() || !agentResolvedTicket.Any() || !agentCustSatRating.Any())
                {
                    ViewData["NoDataMessage"] = "No data available for the selected time period.";
                }
                else
                {
                    ViewData["NoDataMessage"] = null;
                }


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

            ViewData["startDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["endDate"] = endDate?.ToString("yyyy-MM-dd");

            return View();

        }


    }
}
