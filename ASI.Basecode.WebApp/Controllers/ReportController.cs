using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.Services.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
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
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd"); 
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewData["Title"] = "Reports";

            var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

            if (User.FindFirst("UserRole")?.Value == "administrator")
            {
                List<VwTotalTicketSummaryWithCategory> TicketsByCategory;
                List<VwTotalTicketSummaryWithStatus> TicketsByStatus;
                List<VwTotalTicketSummaryWithPriority> TicketsByPriority;
                List<VwResolvedTicketByAgent> agentResolvedTicket;
                List<VwAverageResolutionTime> agentAvgResTime;
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
                            TotalCount = g.Sum(x => x.TotalCount) 
                        })
                        .ToList();

                    TicketsByPriority = _db.VwTotalTicketSummaryWithPriorities
                        .GroupBy(x => x.PriorityName)
                        .Select(g => new VwTotalTicketSummaryWithPriority
                        {
                            PriorityName = g.Key,
                            TotalCount = g.Sum(x => x.TotalCount) 
                        })
                        .ToList();

                    agentResolvedTicket = _db.VwResolvedTicketByAgents
                        .GroupBy(x => x.AgentName)
                        .Select(g => new VwResolvedTicketByAgent
                        {
                            AgentName = g.Key,
                            TotalResolvedCount = g.Sum(x => x.TotalResolvedCount) 
                        })
                        .ToList();

                    agentAvgResTime = _db.VwAverageResolutionTimes
                        .GroupBy(x => x.AgentName)
                        .Select(g => new VwAverageResolutionTime
                        {
                            AgentName = g.Key,
                            AvgResolutionTime = Math.Round((decimal)g.Average(x => x.AvgResolutionTime), 2)
                        })
                        .ToList();

                    agentCustSatRating = _db.VwCustomerSatisfactionRatings
                        .GroupBy(x => x.AgentName)
                        .Select(g => new VwCustomerSatisfactionRating
                        {
                            AgentName = g.Key,
                            AvgFeedbackRating = g.Average(x => x.AvgFeedbackRating) 
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
                            TotalCount = g.Sum(x => x.TotalCount) 
                        })
                        .ToList();

                     TicketsByStatus = _db.VwTotalTicketSummaryWithStatuses
                        .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate)
                        .GroupBy(x => x.StatusName)
                        .Select(g => new VwTotalTicketSummaryWithStatus
                        {
                            StatusName = g.Key,
                            TotalCount = g.Sum(x => x.TotalCount)
                        })
                        .ToList();

                    TicketsByPriority = _db.VwTotalTicketSummaryWithPriorities
                        .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate)
                        .GroupBy(x => x.PriorityName)
                        .Select(g => new VwTotalTicketSummaryWithPriority
                        {
                            PriorityName = g.Key,
                            TotalCount = g.Sum(x => x.TotalCount)
                        })
                        .ToList();

                    agentResolvedTicket = _db.VwResolvedTicketByAgents
                        .Where(x => x.ResolvedAt >= startDate && x.ResolvedAt <= endDate)
                        .GroupBy(x => x.AgentName)
                        .Select(g => new VwResolvedTicketByAgent
                        {
                            AgentName = g.Key,
                            TotalResolvedCount = g.Sum(x => x.TotalResolvedCount) 
                        })
                        .ToList();

                    agentAvgResTime = _db.VwAverageResolutionTimes
                        .Where(x => x.ResolvedAt >= startDate && x.ResolvedAt <= endDate)
                        .GroupBy(x => x.AgentName)
                        .Select(g => new VwAverageResolutionTime
                        {
                            AgentName = g.Key,
                            AvgResolutionTime = Math.Round((decimal)g.Average(x => x.AvgResolutionTime), 2)
                        })
                        .ToList();

                    agentCustSatRating = _db.VwCustomerSatisfactionRatings
                        .Where(x => x.FeedbackAt >= startDate && x.FeedbackAt <= endDate)
                        .GroupBy(x => x.AgentName)
                        .Select(g => new VwCustomerSatisfactionRating
                        {
                            AgentName = g.Key,
                            AvgFeedbackRating = g.Average(x => x.AvgFeedbackRating) 
                        })
                        .ToList();
                }

                var dataCollections = new Dictionary<string, IEnumerable>
                {
                    { "NoDataCategory", TicketsByCategory },
                    { "NoDataStatus", TicketsByStatus },
                    { "NoDataPriority", TicketsByPriority },
                    { "NoDataResolvedTickets", agentResolvedTicket },
                    { "NoDataResolutionTime", agentAvgResTime },
                    { "NoDataSatisfaction", agentCustSatRating }
                };

                foreach (var collection in dataCollections)
                {
                    if (collection.Value == null)
                    {
                        ViewData[collection.Key] = $"No data for the selected time period in {collection.Key.Split('D')[1].ToLower()} data.";
                    }
                    else
                    {
                        ViewData[collection.Key] = null;  
                    }
                }


                var summaryModel = new ReportingAndAnalyticsModel
                {
                    TicketSummaryWithCategory = TicketsByCategory,
                    TicketSummaryWithStatus = TicketsByStatus,
                    TicketSummaryWithPriority = TicketsByPriority,
                    TicketsResolved = agentResolvedTicket,
                    AverageResolutionTime = agentAvgResTime,
                    CustomerSatisfactionRatings = agentCustSatRating,   

                };
                return View(summaryModel);
            }
            else if (User.FindFirst("UserRole")?.Value == "support agent")
            {
                List<VwTicketsByCategory> TicketsByCategoryAgent;
                List<VwTicketsByStatus> TicketsByStatusAgent;
                List<VwTicketsByPriority> TicketsByPriorityAgent;

                if (startDate == null && endDate == null)
                {
                    TicketsByCategoryAgent = _db.VwTicketsByCategories
                        .Where(m => m.UserId == userId)
                        .GroupBy(x => x.CategoryName)
                        .Select(g => new VwTicketsByCategory
                        {
                            CategoryName = g.Key,
                            TicketsByCategory = g.Sum(x => x.TicketsByCategory)
                        })
                        .ToList();

                    TicketsByStatusAgent = _db.VwTicketsByStatuses
                            .Where(m => m.UserId == userId)
                            .GroupBy(x => x.StatusName)
                            .Select(g => new VwTicketsByStatus
                            {
                                StatusName = g.Key,
                                TicketByStatus = g.Sum(x => x.TicketByStatus)
                            })
                            .ToList();

                    TicketsByPriorityAgent = _db.VwTicketsByPriorities
                            .Where(m => m.UserId == userId)
                            .GroupBy(x => x.PriorityName)
                            .Select(g => new VwTicketsByPriority
                            {
                                PriorityName = g.Key,
                                TicketByPriority = g.Sum(x => x.TicketByPriority)
                            })
                            .ToList(); 
                }
                else
                {
                    TicketsByCategoryAgent = _db.VwTicketsByCategories
                        .Where(m => m.UserId == userId)
                        .Where(x => x.AssignedDate >= startDate && x.AssignedDate <= endDate)
                        .GroupBy(x => x.CategoryName)
                        .Select(g => new VwTicketsByCategory
                        {
                            CategoryName = g.Key,
                            TicketsByCategory = g.Sum(x => x.TicketsByCategory)
                        })
                        .ToList();

                    TicketsByStatusAgent = _db.VwTicketsByStatuses
                            .Where(m => m.UserId == userId)
                            .Where(x => x.AssignedDate >= startDate && x.AssignedDate <= endDate)
                            .GroupBy(x => x.StatusName)
                            .Select(g => new VwTicketsByStatus
                            {
                                StatusName = g.Key,
                                TicketByStatus = g.Sum(x => x.TicketByStatus)
                            })
                            .ToList();

                    TicketsByPriorityAgent = _db.VwTicketsByPriorities       
                            .Where(m => m.UserId == userId)
                            .Where(x => x.AssignedDate >= startDate && x.AssignedDate <= endDate)
                            .GroupBy(x => x.PriorityName)
                            .Select(g => new VwTicketsByPriority
                            {
                                PriorityName = g.Key,
                                TicketByPriority = g.Sum(x => x.TicketByPriority)
                            })
                            .ToList();
                }
                //var TicketsAssignedByMeAgent = _db.VwTicketAssignedToMeAgents.Where(m => m.UserId == userId).ToList();
                var dataCollections = new Dictionary<string, IEnumerable>
                {
                    { "NoDataCategory", TicketsByCategoryAgent },
                    { "NoDataStatus", TicketsByStatusAgent },
                    { "NoDataPriority", TicketsByPriorityAgent }
                };
                foreach (var collection in dataCollections)
                {
                    if (collection.Value == null)
                    {
                        ViewData[collection.Key] = $"No {collection.Key.Split('D')[1].ToLower()} data for the selected time period.";
                    }
                    else
                    {
                        ViewData[collection.Key] = null;  // Clear if there's data
                    }
                }

                var summaryModel = new ReportingAndAnalyticsModel
                {
                    TicketsByCategory = TicketsByCategoryAgent,
                    TicketsByStatus = TicketsByStatusAgent,
                    TicketsByPriority = TicketsByPriorityAgent
                };

                return View(summaryModel);
            }

            return View();

        }

    }
}
