using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.WebApp.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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
        public IActionResult Index()
        {
            if (TempData["temp"] != null)
            {
                if (TempData["status"] as int? == 0)
                {
                    TempData["ResMsg"] = new AlertMessageContent()
                    {
                        Status = ErrorCode.Success,
                        Message = "A ticket has deleted successfully!"
                    };
                }
                else
                {
                    TempData["ResMsg"] = new AlertMessageContent()
                    {
                        Status = ErrorCode.Error,
                        Message = "An error has occured upon deleting the ticket."
                    };
                }
            }
            if (User.Identity.IsAuthenticated)
            {
                var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);
                var myTickets = _db.VwUserTicketViews.Where(m => m.UserId == userId).ToList();

                return View(myTickets);
            }
            return NotFound();
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

            var categories = _db.Categories.ToList();
            customTicket.category = categories;

            var priorities = _db.Priorities.ToList();
            customTicket.priority = priorities;

            var status = _db.Statuses.ToList();
            customTicket.status = status;

            if (customTicket.ticket.CategoryId is null || string.IsNullOrEmpty(customTicket.ticket.IssueDescription)) 
            {
                TempData["ResMsg"] = new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Please fill out all required fields."
                };
                return View(customTicket);
            }

            var imageFile = customTicket.formFile;
            var root = Path.Combine(_webHostEnvironment.WebRootPath, "images");

            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            ViewData["root"] = root;
            /*ID    StatusName
             * 1	Open
             * 2	In Progress
             * 3	Resolved
             * 4	Closed
             */

            customTicket.ticket.StatusId = 1;

            //for errorMsg or successMsg return...
            string errorMsg = string.Empty, successMsg;

            if (imageFile != null) 
            {
                var uniqueFileName = GetUniqueFileName(root, imageFile.FileName);
                var filePath = Path.Combine(root, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                var relativePath = Path.Combine("/images", uniqueFileName).Replace("\\", "/");
                customTicket.ticket.AttachmentPath = relativePath;
                if (_ticketRepo.Create(customTicket.ticket) == ErrorCode.Success)
                {
                    var userTicket = new UserTicket();
                    userTicket.UserId = Convert.ToInt32(userId);
                    userTicket.TicketId = customTicket.ticket.TicketId;
                    if (_userTicketRepo.Create(userTicket) == ErrorCode.Success)
                    {
                        NotificationManager _notifManager = new NotificationManager();
                        string? userName = User.FindFirst(ClaimsIdentity.DefaultNameClaimType)?.Value;

                        if (_notifManager.CreateTicketNotif(Convert.ToInt32(userId), userName, (int)customTicket.ticket.CategoryId, userTicket.UserTicketId, out errorMsg, out successMsg) == ErrorCode.Success)
                        {
                            TempData["ResMsg"] = new AlertMessageContent()
                            {
                                Status = ErrorCode.Success,
                                Message = successMsg
                            };
                        }
                        return View(customTicket);
                    }
                }
            } 
            else
            {
                if (_ticketRepo.Create(customTicket.ticket) == ErrorCode.Success)
                {
                    var userTicket = new UserTicket();
                    userTicket.UserId = Convert.ToInt32(userId);
                    userTicket.TicketId = customTicket.ticket.TicketId;
                    if (_userTicketRepo.Create(userTicket) == ErrorCode.Success)
                    {
                        NotificationManager _notifManager = new NotificationManager();
                        string? userName = User.FindFirst(ClaimsIdentity.DefaultNameClaimType)?.Value;

                        if (_notifManager.CreateTicketNotif(Convert.ToInt32(userId), userName, (int)customTicket.ticket.CategoryId, userTicket.UserTicketId, out errorMsg, out successMsg) == ErrorCode.Success)
                        {
                            TempData["ResMsg"] = new AlertMessageContent()
                            {
                                Status = ErrorCode.Success,
                                Message = successMsg
                            };
                        }
                        return View(customTicket);
                    }
                }
            }

            TempData["ResMsg"] = new AlertMessageContent()
            {
                Status = ErrorCode.Error,
                Message = string.IsNullOrEmpty(errorMsg) == true ? "An error has occured upon submitting the ticket, pls try again." : errorMsg
            };
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

        public IActionResult Edit(int id) 
        {
            if (!User.Identity.IsAuthenticated)
            {
                return BadRequest();
            }

            var userId = User.FindFirst("UserId")?.Value;
            var myTicket = _db.Tickets.Where(m => m.TicketId == id).FirstOrDefault();
            var customTicket = new CustomTicket();

            if (myTicket is null)
            {
                return NotFound();
            }
            var categories = _db.Categories.ToList();
            customTicket.category = categories;

            var priorities = _db.Priorities.ToList();
            customTicket.priority = priorities;

            var status = _db.Statuses.ToList();
            customTicket.status = status;

            customTicket.ticket = myTicket;

            return View(customTicket);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CustomTicket customTicket)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return BadRequest();
            }

            if (customTicket == null) {
                return BadRequest();
            }

            var myTicket = _db.Tickets.Where(m => m.TicketId == customTicket.ticket.TicketId).FirstOrDefault();

            if (myTicket is null)
            {
                return BadRequest();
            }
            var categories = _db.Categories.ToList();
            customTicket.category = categories;

            var priorities = _db.Priorities.ToList();
            customTicket.priority = priorities;

            var status = _db.Statuses.ToList();
            customTicket.status = status;


            myTicket.IssueDescription = customTicket.ticket.IssueDescription;
            myTicket.CategoryId = customTicket.ticket.CategoryId;
            if (!string.IsNullOrEmpty(customTicket.RemovedOriginalImg))
            {
                myTicket.AttachmentPath = null;
            } else
            {
                var imageFile = customTicket.formFile;
                var root = Path.Combine(_webHostEnvironment.WebRootPath, "images");

                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                }
                if (imageFile != null)
                {
                    var uniqueFileName = GetUniqueFileName(root, imageFile.FileName);
                    var filePath = Path.Combine(root, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    var relativePath = Path.Combine("/images", uniqueFileName);
                    myTicket.AttachmentPath = relativePath;
                }
            }
            myTicket.LastModified = DateTimeToday();

            if (_ticketRepo.Update(myTicket.TicketId, myTicket) == ErrorCode.Success)
            {
                TempData["ResMsg"] = new AlertMessageContent()
                {
                    Status = ErrorCode.Success,
                    Message = "Ticket updated successfully!"
                };
                customTicket.ticket = myTicket;
                return View(customTicket);
            }
            TempData["ResMsg"] = new AlertMessageContent()
            {
                Status = ErrorCode.Error,
                Message = "An error has occured when updating your ticket, pls try again."
            };
            customTicket.ticket = myTicket;
            return View(customTicket);
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
