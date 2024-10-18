using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.WebApp.Repository;
using ASI.Basecode.WebApp.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    public class ReportController : BaseController
    {
        public ReportController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {

        }

        public IActionResult Index()
        {
            // Fetch categories with their ticket counts
            var categoryCounts = _db.Categories
                .Select(category => new CategoryDataModel
                {
                    CategoryName = category.CategoryName,
                    TicketCount = _db.Tickets.Count(ticket => ticket.CategoryId == category.CategoryId)
                })
                .ToList(); // Execute the query and convert to a list

            // Fetch statuses with their ticket counts
            var statusCounts = _db.Statuses
                .Select(status => new StatusDataModel
                {
                    StatusName = status.StatusName,
                    TicketCount = _db.Tickets.Count(ticket => ticket.StatusId == status.StatusId)
                })
                .ToList();

            // Fetch priorities with their ticket counts
            var priorityCounts = _db.Priorities
                .Select(priority => new PriorityDataModel
                {
                    PriorityName = priority.PriorityName,
                    TicketCount = _db.Tickets.Count(ticket => ticket.PriorityId == priority.PriorityId)
                })
                .ToList();

            // Fetch user activities with their ticket counts
            var userActivityCounts = _db.Users
                .Select(user => new UserActivityDataModel
                {
                    UserName = user.Name,
                    //TicketCount = _db.Tickets.Count(ticket => ticket.AssignedToUserId == user.UserId)
                })
                .ToList();

            // Create a summary model to pass to the view
            var summaryModel = new CustomTicketSummaryViewModel
            {
                Categories = categoryCounts,
                Statuses = statusCounts,
                Priorities = priorityCounts,
                UserActivities = userActivityCounts
            };

            return View(summaryModel); // Pass the model to the view
        }





    }
}
