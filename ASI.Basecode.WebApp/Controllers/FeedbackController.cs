using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ASI.Basecode.WebApp.Repository;
using System.Linq;
using System;

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

            ViewBag.Tickets = tickets;

            return View(new Feedback());
        }

        [HttpPost]
        public IActionResult Create(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                feedback.UserId = (int)_userManager.GetLoggedInUserId(HttpContext);

                _db.Feedbacks.Add(feedback);
                _db.SaveChanges();

                return RedirectToAction("Index", "Feedback");
            }
            ViewBag.Tickets = _db.Tickets
                                 .Include(t => t.Category)
                                 .Where(t => t.UserTickets.Any(ut => ut.UserId == feedback.UserId) &&
                                             (t.StatusId == 3 || t.StatusId == 4))
                                 .ToList();

            return View(feedback);
        }

        public IActionResult Index()
        {
            var feedbacks = _db.Feedbacks.Include(f => f.User).Include(f => f.Ticket).ToList();
            return View(feedbacks);
        }
    }
}