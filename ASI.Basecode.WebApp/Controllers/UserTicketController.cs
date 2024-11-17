using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.WebApp.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Policy = "UserPolicy")]
    public class UserTicketController : BaseController
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserTicketController(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet("MyTickets")]
        public IActionResult Index()
        {
            if (TempData["temp"] is not null)
            {
                HandleTempDataMessages();
            }

            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);
                    var myTickets = _db.VwUserTicketViews
                        .Where(m => m.UserId == userId)
                        .OrderByDescending(m => m.TicketId).ToList()
                        .ToList();

                    UpdateTicketStatusMessages(myTickets);

                    ViewData["TableId"] = "myTicketsTable";
                    ViewBag.Priorities = _db.Priorities.ToList();
                    ViewBag.Categories = _db.Categories.ToList();
                    ViewBag.Statuses = _db.Statuses.ToList();

                    return View(myTickets);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }
            }

            return NotFound();
        }

        [HttpGet("MyTickets/{id}")]
        public IActionResult Index(int id)
        {
            HandleTempDataMessages();

            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);
                    var myTickets = _db.VwUserTicketViews
                        .Where(m => m.UserId == userId && (id == 1 ? m.StatusId == 3 : m.StatusId != 3))
                        .OrderByDescending(m => m.TicketId).ToList();

                    UpdateTicketStatusMessages(myTickets);

                    TempData["TableId"] = "myTicketsTable";
                    ViewBag.Priorities = _db.Priorities.ToList();
                    ViewBag.Categories = _db.Categories.ToList();
                    ViewBag.Statuses = _db.Statuses.ToList();

                    return View(myTickets);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }
            }

            return NotFound();
        }

        private void HandleTempDataMessages()
        {
            var temp = TempData["temp"]?.ToString();
            if (temp == "create")
            {
                HandleCreateTempData();
            }
            else if (temp == "delete")
            {
                HandleDeleteTempData();
            }
            else if (temp == "update")
            {
                HandleUpdateTempData();
            }
        }

        private void HandleCreateTempData()
        {
            if (TempData["ResMsg"] is not null)
            {
                var resMsg = JsonConvert.DeserializeObject<AlertMessageContent>(TempData["ResMsg"].ToString());
                if (resMsg is not null && User.Identity.IsAuthenticated)
                {
                    TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent
                    {
                        Status = resMsg.Status,
                        Message = resMsg.Message
                    });
                }
            }
        }

        private void HandleDeleteTempData()
        {
            var status = TempData["status"] as int?;
            TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent
            {
                Status = status == 0 ? ErrorCode.Success : ErrorCode.Error,
                Message = status == 0
                    ? "A ticket has been deleted successfully!"
                    : "An error has occurred upon deleting the ticket."
            });
        }

        private void HandleUpdateTempData()
        {
            var status = TempData["status"] as int?;
            TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent
            {
                Status = status == 0 ? ErrorCode.Success : ErrorCode.Error,
                Message = status == 0
                    ? "Ticket updated successfully!"
                    : "An error has occurred when updating your ticket, please try again."
            });
        }

        private void UpdateTicketStatusMessages(List<VwUserTicketView> tickets)
        {
            tickets.ForEach(ticket =>
            {
                ticket.StatusName = ticket.StatusId switch
                {
                    1 => "Your ticket is pending assignment to an agent.",
                    2 => "Your ticket is currently in progress. An agent is working on it.",
                    3 => "Your ticket has been resolved.",
                    4 => "Your ticket is closed.",
                    _ => "Status is unknown. Please contact support for more information."
                };
            });
        }

        public IActionResult Create()
        {
            CustomTicket ticket = new CustomTicket();

            var categories = _db.Categories.ToList();
            ticket.category = categories;

            var priorities = _db.Priorities.ToList();
            ticket.priority = priorities;

            var status = _db.Statuses.ToList();
            ticket.status = status;

            return View(ticket);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomTicket customTicket)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return NotFound();
            }

            var userId = User.FindFirst("UserId")?.Value;

            customTicket.category = _db.Categories.ToList();
            customTicket.priority = _db.Priorities.ToList();
            customTicket.status = _db.Statuses.ToList();

            if (customTicket.ticket.CategoryId == null || string.IsNullOrEmpty(customTicket.ticket.IssueDescription))
            {
                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Please provide a description of the issue. This information is required to assist you effectively."
                });
                return View(customTicket);
            }

            customTicket.ticket.CreateAt = DateTime.Today;
            customTicket.ticket.LastModified = DateTime.Today;

            var imageFile = customTicket.formFile;
            var root = Path.Combine(_webHostEnvironment.WebRootPath, "images");

            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            ViewData["root"] = root;
            customTicket.ticket.StatusId = 1;

            string errorMsg = string.Empty, successMsg;

            if (imageFile != null)
            {
                string uniqueFileName = GetUniqueFileName(root, imageFile.FileName);
                string filePath = Path.Combine(root, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                string relativePath = Path.Combine("/images", uniqueFileName).Replace("\\", "/");
                customTicket.ticket.AttachmentPath = relativePath;
            }

            if (_ticketRepo.Create(customTicket.ticket) == ErrorCode.Success)
            {
                var userTicket = new UserTicket
                {
                    UserId = Convert.ToInt32(userId),
                    TicketId = customTicket.ticket.TicketId
                };

                if (_userTicketRepo.Create(userTicket) == ErrorCode.Success)
                {
                    NotificationManager _notifManager = new NotificationManager();
                    string? userName = User.FindFirst(ClaimsIdentity.DefaultNameClaimType)?.Value;

                    if (_notifManager.CreateTicketNotif(Convert.ToInt32(userId), userName, (int)customTicket.ticket.CategoryId, userTicket.UserTicketId, out errorMsg, out successMsg) == ErrorCode.Success)
                    {
                        TempData["temp"] = "create";
                        TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                        {
                            Status = ErrorCode.Success,
                            Message = successMsg
                        });
                        return RedirectToAction("Index");
                    }
                }
            }

            // Handle errors
            TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
            {
                Status = ErrorCode.Error,
                Message = string.IsNullOrEmpty(errorMsg) ? "An error has occurred upon submitting the ticket, please try again." : errorMsg
            });

            return View(customTicket);
        }

        public string GetUniqueFileName(string parentPath, string fileName)
        {
            string uniqueName = fileName;
            int count = 1;

            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, parentPath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, uniqueName);

            while (System.IO.File.Exists(filePath))
            {
                string extension = Path.GetExtension(fileName);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                uniqueName = $"{fileNameWithoutExtension}_{count}{extension}";
                filePath = Path.Combine(folderPath, uniqueName);
                count++;
            }

            return uniqueName;
        }

        public IActionResult Details(int id) 
        {
            if (!User.Identity.IsAuthenticated)
            {
                return BadRequest();
            }

            var userId = User.FindFirst("UserId")?.Value;
            var myTicket = _db.VwUserTicketViews.Where(m => m.TicketId == id).FirstOrDefault();

            return View(myTicket);
        }

        public IActionResult Delete(int id)
        {
            TempData["temp"] = "delete";
            if (_ticketRepo.Delete(id) == ErrorCode.Success)
            {
                TempData["status"] = 0;
                return Ok();
            }

            TempData["status"] = 1;
            return BadRequest();
        }
    }
}
