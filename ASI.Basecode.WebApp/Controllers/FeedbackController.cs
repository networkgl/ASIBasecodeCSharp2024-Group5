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

namespace ASI.Basecode.WebApp.Controllers
{
    public class FeedbackController : BaseController
    {
        private readonly UserManager _userManager;

        public FeedbackController()
        {
            _userManager = new UserManager();
        }

        [HttpGet]
        public IActionResult Create()
        {
            int? loggedInUserId = _userManager.GetLoggedInUserId(HttpContext);

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
            var feedbacks = _db.Feedbacks.Where(m => m.UserId == userId).ToList();

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
    }
}