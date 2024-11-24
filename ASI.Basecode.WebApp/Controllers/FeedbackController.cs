using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ASI.Basecode.WebApp.Repository;
using System.Linq;
using System;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.Data.Interfaces;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using ASI.Basecode.Services.Controllers;
//using Microsoft.AspNetCore.MV

namespace ASI.Basecode.WebApp.Controllers
{
    public class FeedbackController : BaseController
    {
        private readonly UserManager _userManager;

        public FeedbackController()
        {
            _userManager = new UserManager();
        }
        public IActionResult Index(int? agentId = null)
        {
            ViewData["TableId"] = "myFeedbackTable";

            int? loggedInUserId = _userManager.GetLoggedInUserId(HttpContext);
            if (loggedInUserId == null)
                return Unauthorized("User not logged in");

            var userRole = _db.VwUserRoleViews.FirstOrDefault(u => u.UserId == loggedInUserId);
            if (userRole == null)
                return Unauthorized("User role not found");

            List<VwFeedbackView> feedbacks = new List<VwFeedbackView>();

            if (userRole.RoleId == 1) // Regular user
            {
                feedbacks = _db.VwFeedbackViews
                    .Where(f => f.UserId == loggedInUserId)
                    .ToList();
                ViewData["UserRoleId"] = userRole.RoleId;
                return View(feedbacks);
            }
            else if (userRole.RoleId == 2) // Agent
            {
                feedbacks = _db.VwFeedbackViews
                    .Where(f => f.AgentId == loggedInUserId)
                    .ToList();

                var agentFeedbacks = feedbacks.Where(f => f.FeedbackRating.HasValue);
                var averageRating = agentFeedbacks.Any() ? agentFeedbacks.Average(f => f.FeedbackRating.Value) : 0;
                var feedbackCount = agentFeedbacks.Count();

                ViewData["AverageRating"] = averageRating;
                ViewData["FeedbackCount"] = feedbackCount;
                ViewData["UserRoleId"] = userRole.RoleId;
                return View(feedbacks);
            }
            else if (userRole.RoleId == 3) // Admin
            {
                if (agentId.HasValue) // Admin viewing specific agent's feedback
                {
                    var agentFeedbacks = _db.VwFeedbackViews
                        .Where(f => f.AgentId == agentId)
                        .ToList();

                    var agentName = _db.VwUserRoleViews
                        .FirstOrDefault(u => u.UserId == agentId)?.Name;

                    ViewData["AgentName"] = agentName;
                    ViewData["UserRoleId"] = userRole.RoleId;
                    return View("~/Views/Feedback/Index.cshtml", agentFeedbacks);
                }
                else
                {
                    var agentFeedbackRatings = _db.VwAgentFeedbackRatingViews.ToList();
                    ViewData["UserRoleId"] = userRole.RoleId;
                    return View("~/Views/AdminFeedback/Index.cshtml", agentFeedbackRatings);
                }
            }

            return Forbid("Access denied for this user role.");
        }

        [HttpGet]
        public IActionResult Create()
        {
            int? loggedInUserId = _userManager.GetLoggedInUserId(HttpContext);
            if (loggedInUserId == null)
                return Unauthorized("User not logged in");

            var ticketsWithNoFeedback = _db.VwAssignedTicketViews
                .Where(v => v.UserId == loggedInUserId && (v.StatusId == 3 || v.StatusId == 4)) // Only resolved/closed tickets
                .Where(v => !_db.VwFeedbackViews.Any(f => f.UserTicketId == v.TicketId)) // Exclude tickets with feedback
                .ToList();

            ViewBag.Tickets = ticketsWithNoFeedback;

            return View(new Feedback());
        }

        [HttpPost]
        public IActionResult Create(Feedback feedback, string CategoryName)
        {
            if (!ModelState.IsValid)
            {
                int? loggedInUserId = _userManager.GetLoggedInUserId(HttpContext);
                ViewBag.Tickets = _db.VwAssignedTicketViews
                    .Where(v => v.UserId == loggedInUserId)
                    .ToList();

                return View(feedback);
            }

            feedback.UserId = _userManager.GetLoggedInUserId(HttpContext).Value;
            feedback.TicketCategory = CategoryName;
            feedback.AgentId = feedback.AgentId;
            feedback.CreatedAt = DateTime.Now;
            feedback.AssignedTicketId = feedback.AssignedTicketId;
            _db.Feedbacks.Add(feedback);
            _db.SaveChanges();

            return RedirectToAction("Index", "Feedback");
        }
        //[HttpGet]
        //public IActionResult Edit(int id)
        //{
        //    var feedbackView = _db.VwFeedbackViews.FirstOrDefault(f => f.FeedbackId == id);
        //    if (feedbackView == null)
        //    {
        //        return NotFound("Feedback not found.");
        //    }

        //    return View(feedbackView);
        //}

        //[HttpPost]
        //public IActionResult Edit(int feedbackId, string feedbackText, decimal feedbackRating)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        var feedbackView = _db.VwFeedbackViews.FirstOrDefault(f => f.FeedbackId == feedbackId);
        //        return View(feedbackView);
        //    }

        //    var existingFeedback = _db.Feedbacks.FirstOrDefault(f => f.FeedbackId == feedbackId);
        //    if (existingFeedback == null)
        //    {
        //        return NotFound("Feedback not found.");
        //    }

        //    existingFeedback.FeedbackText = feedbackText;
        //    existingFeedback.FeedbackRating = feedbackRating;
        //    existingFeedback.CreatedAt = DateTime.Now;

        //    _db.SaveChanges();

        //    return RedirectToAction("Index", "Feedback");
        //}

        [HttpPost]
        [Route("Feedback/Delete/{id:int}")]
        public IActionResult Delete(int id)
        {
            var feedback = _db.Feedbacks.FirstOrDefault(f => f.FeedbackId == id);
            if (feedback == null)
            {
                return NotFound(new { message = "Feedback not found." });
            }

            _db.Feedbacks.Remove(feedback);
            _db.SaveChanges();

            return Ok(new { message = "Feedback deleted successfully." });
        }
    }
}