using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ASI.Basecode.WebApp.Repository;
using System.Linq;
using System;
<<<<<<< Updated upstream
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.Data.Interfaces;
using System.Collections.Generic;
=======
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
>>>>>>> Stashed changes

namespace ASI.Basecode.WebApp.Controllers
{
    public class FeedbackController : BaseController
    {
        private readonly UserManager _userManager;

        public FeedbackController()
        {
            _userManager = new UserManager();
        }
<<<<<<< Updated upstream
=======
        public IActionResult Index(int? agentId = null)
        {
            int? loggedInUserId = _userManager.GetLoggedInUserId(HttpContext);
            if (loggedInUserId == null)
                return Unauthorized("User not logged in");

            var userRole = _db.VwUserRoleViews.FirstOrDefault(u => u.UserId == loggedInUserId);
            if (userRole == null)
                return Unauthorized("User role not found");

            List<VwFeedbackView> feedbacks;
            if (userRole.RoleId == 1)
            {
                feedbacks = _db.VwFeedbackViews.Where(f => f.UserId == loggedInUserId).ToList();
                ViewData["UserRoleId"] = userRole.RoleId;
                return View(feedbacks);
            }
            else if (userRole.RoleId == 2)
            {
                feedbacks = _db.VwFeedbackViews.Where(f => f.AgentId == loggedInUserId).ToList();
                var agentFeedbacks = feedbacks.Where(f => f.FeedbackRating.HasValue);
                var averageRating = agentFeedbacks.Any() ? agentFeedbacks.Average(f => f.FeedbackRating.Value) : 0;
                var feedbackCount = agentFeedbacks.Count();

                ViewData["AverageRating"] = averageRating;
                ViewData["FeedbackCount"] = feedbackCount;
                ViewData["UserRoleId"] = userRole.RoleId;

                return View(feedbacks);
            }
            else if (userRole.RoleId == 3)
            {
                if (agentId != null)
                {
                    var agentFeedbacks = _db.VwFeedbackViews.Where(f => f.AgentId == agentId).ToList();
                    var agentName = _db.VwUserRoleViews.FirstOrDefault(u => u.UserId == agentId)?.Name;
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
            else
            {
                return Forbid("Access denied for this user role.");
            }
        }

>>>>>>> Stashed changes

        [HttpGet]
        public IActionResult Create()
        {
            int? loggedInUserId = _userManager.GetLoggedInUserId(HttpContext);
<<<<<<< Updated upstream

            if (loggedInUserId == null)
            {
                return Unauthorized("User not logged in");
            }

            var tickets = _db.Tickets.Include(t => t.Category)
                                     .Where(t => t.UserTickets.Any(ut => ut.UserId == loggedInUserId) &&
                                                 (t.StatusId == 3 || t.StatusId == 4))
                                     .ToList();

            if (!tickets.Any())
            {
                return NotFound("No resolved or closed tickets found");
            }

            //ViewBag.Tickets = tickets;

            var customFeedbackModel = new CustomFeedbackModel()
            {
                Ticket = tickets,
                Feedback = new Feedback(),
            };

            return View(customFeedbackModel);
        }

        [HttpPost]
        public IActionResult Create(CustomFeedbackModel customFeedbackModel)
        {
            if (ModelState.IsValid)
            {
                var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);
                var assignedTicket = _db.AssignedTickets.Where(m => m.UserTicketId == customFeedbackModel.TicketId).FirstOrDefault();

                customFeedbackModel.Feedback.UserTicketId = customFeedbackModel.TicketId;
                customFeedbackModel.Feedback.AgentId = assignedTicket.AgentId;
                customFeedbackModel.Feedback.CreatedAt = DateTimeToday();
                customFeedbackModel.Feedback.UserId = userId;
                customFeedbackModel.Feedback.AssignedTicketId = assignedTicket.AssignedTicketId;

                if (_feedbackRepo.Create(customFeedbackModel.Feedback) == ErrorCode.Success)
                {
                    return RedirectToAction("Index", "Feedback");

                }
                return View(customFeedbackModel);
            }
            return View(customFeedbackModel);
        }

        public IActionResult Index()
        {
            var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);
            var feedbacks = _db.Feedbacks.Where(m => m.UserId == userId).OrderByDescending(m => m.FeedbackId).ToList();

            var ticket = new List<Ticket>();
            foreach (var item in feedbacks)
            {
                ticket.Add(_db.Tickets.Where(m => m.TicketId == item.UserTicketId).FirstOrDefault()); 
            }

            var customFeedbackModel = new CustomFeedbackModel()
            {
                FeedbackList = feedbacks,
                Ticket = ticket
            };
            return View(customFeedbackModel);
        }
=======
            if (loggedInUserId == null)
                return Unauthorized("User not logged in");

            var tickets = _db.VwAssignedTicketViews
                .Where(v => v.UserId == loggedInUserId)
                .Where(tickets => tickets.StatusId == 3 || tickets.StatusId == 4)
                .ToList();

            ViewBag.Tickets = tickets;

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
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var feedbackView = _db.VwFeedbackViews.FirstOrDefault(f => f.FeedbackId == id);
            if (feedbackView == null)
            {
                return NotFound("Feedback not found.");
            }

            return View(feedbackView);
        }

        [HttpPost]
        public IActionResult Edit(int feedbackId, string feedbackText, decimal feedbackRating)
        {
            if (!ModelState.IsValid)
            {
                var feedbackView = _db.VwFeedbackViews.FirstOrDefault(f => f.FeedbackId == feedbackId);
                return View(feedbackView);
            }

            var existingFeedback = _db.Feedbacks.FirstOrDefault(f => f.FeedbackId == feedbackId);
            if (existingFeedback == null)
            {
                return NotFound("Feedback not found.");
            }

            existingFeedback.FeedbackText = feedbackText;
            existingFeedback.FeedbackRating = feedbackRating;
            existingFeedback.CreatedAt = DateTime.Now;

            _db.SaveChanges();

            return RedirectToAction("Index", "Feedback");
        }
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
        public IActionResult AdminFeedback()
        {
            // Retrieve all users with the role of 'Agent' (assuming RoleId = 2)
            var agents = _db.VwUserRoleViews
                .Where(u => u.RoleId == 2) // Filter for agents
                .Select(agent => new
                {
                    AgentId = agent.UserId,
                    AgentName = agent.Name,
                    AverageRating = _db.VwFeedbackViews
                        .Where(f => f.AgentId == agent.UserId && f.FeedbackRating.HasValue)
                        .Average(f => (decimal?)f.FeedbackRating) ?? 0, // Calculate average rating
                    FeedbackCount = _db.VwFeedbackViews
                        .Count(f => f.AgentId == agent.UserId) // Count feedbacks
                })
                .ToList();

            return View(agents);
        }

>>>>>>> Stashed changes
    }
}