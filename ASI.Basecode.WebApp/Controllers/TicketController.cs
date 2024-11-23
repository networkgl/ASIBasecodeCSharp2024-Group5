using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Resources.Messages;
using ASI.Basecode.Services.Controllers;
using ASI.Basecode.Services.Repository;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp.Repository;
using ASI.Basecode.WebApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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
    [Authorize]
    public class TicketController : TicketServices
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TicketController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet("MyTickets")]
        [Authorize(Policy ="UserPolicy")]
        public IActionResult UserTicketIndex()
        {
            HandleTempDataMessages();

            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    return View(UserMyTicketIndex());
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }
            }
            return NotFound();
        }

        [HttpGet("MyTickets/{id}")]
        [Authorize(Policy = "UserPolicy")]
        public IActionResult UserTicketIndex(int id)
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

        [HttpGet("AgentManageTickets")]
        [Authorize(Policy = "SupportAgentPolicy")]
        public IActionResult AgentTicketIndex()
        {
            HandleTempDataMessages();

            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    return View(SupportAgentTicketIndex());
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }
            }
            return NotFound();
        }

        [HttpGet("AgentManageTickets/{id}")]
        [Authorize(Policy = "SupportAgentPolicy")]
        public IActionResult AgentTicketIndex(int id)
        {
            HandleTempDataMessages();

            if (User.Identity.IsAuthenticated)
            {
                var userId = GetLoggedInUserId();

                var myTickets = _db.VwTicketDetailsViews.OrderByDescending(m => m.TicketId).ToList();
                ViewData["TableId"] = "agentManageTicketsTable";
                ViewData["Priorities"] = _db.Priorities.ToList();
                ViewData["Categories"] = _db.Categories.ToList();
                ViewData["Statuses"] = _db.Statuses.ToList();
                return View(myTickets);
            }

            return NotFound();
        }

        [HttpGet("AdminManageTickets")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult AdministratorTicketIndex()
        {
            HandleTempDataMessages();

            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    return View(AdminTicketIndex());
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }
            }
            return NotFound();
        }

        [HttpGet("AdminManageTickets/{id}")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult AdministratorTicketIndex(int id)
        {
            HandleTempDataMessages();

            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    return View(AdminTicketIndex());
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }
            }
            return NotFound();
        }

        public IActionResult CreateTicket()
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
        public async Task<IActionResult> CreateTicket(CustomTicket customTicket)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return NotFound();
            }

            if (ValidateInput(customTicket) == ErrorCode.Error)
            {
                PopulateCustomTicket(customTicket);
                return View(customTicket);
            }

            var userId = GetLoggedInUserId();
            string errorMsg = string.Empty, successMsg = string.Empty;

            SetDateToday(customTicket);
            PopulateCustomTicket(customTicket);
            await SetTicketAttachmentPath(customTicket);

            if (UserTicketCreate(customTicket, userId, out errorMsg, out successMsg) == ErrorCode.Success)
            {
                CreateTempDataForAlertContent(ErrorCode.Success, successMsg);
                Console.WriteLine(ViewData["ResMsg"]);
                return RedirectToAction("UserTicketIndex");
            }

            // Handle errors
            CreateTempDataForAlertContent(ErrorCode.Error, string.IsNullOrEmpty(errorMsg) ? "An error has occurred upon submitting the ticket, please try again." : errorMsg);
            return View(customTicket);
        }

        [HttpGet]
        public IActionResult AgentUpdateTicket(int id, int? NotificationId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var customTicket = new CustomEditTicketAssignment();
                var ticketAssignment = _db.VwTicketDetailsViews.Where(m => m.TicketId == id).FirstOrDefault();

                if (ticketAssignment == null)
                {
                    return BadRequest();
                }

                var assigner = _db.Users.Where(m => m.UserId == ticketAssignment.AssignerId).FirstOrDefault();
                var agents = _db.VwUserRoleViews.Where(m => m.RoleId == 2).ToList();
                var userTicket = _db.UserTickets.Where(m => m.UserTicketId == ticketAssignment.UserTicketId).FirstOrDefault();
                var assignedTicket = _db.AssignedTickets.Where(m => m.AssignedTicketId == ticketAssignment.AssignedTicketId).FirstOrDefault();
                var ticketCount = _db.VwTicketCountForAgents.ToList();

                var agent = new User();

                if (assignedTicket != null)
                {
                    agent = _db.Users.Where(m => m.UserId == assignedTicket.AgentId).FirstOrDefault();
                }
                else
                {
                    agent = null;
                }

                if (userTicket is null)
                {
                    return BadRequest();
                }

                var categories = _db.Categories.ToList();
                customTicket.Category = categories;

                var priorities = _db.Priorities.ToList();
                customTicket.Priority = priorities;

                var status = _db.Statuses.ToList().OrderBy(s =>
                                                            s.StatusName == "Open" ? 1 :
                                                            s.StatusName == "In Progress" ? 2 :
                                                            s.StatusName == "Resolved" ? 3 :
                                                            s.StatusName == "Closed" ? 4 : 5).ToList();
                customTicket.Status = status;
                var ticket = _db.Tickets.Where(m => m.TicketId == id).FirstOrDefault();

                if (ticket == null)
                {
                    return BadRequest();
                }

                customTicket.Ticket = ticket;
                customTicket.AssignedTicket = assignedTicket;
                customTicket.Agents = agents;
                customTicket.Agent = agent;
                customTicket.ticketCountForAgent = ticketCount;


                //update notif mark as read if this route is visited from notification view
                if (NotificationId != null)
                {
                    var getNotifById = _notifRepo.Table.Where(m => m.NotificationId == NotificationId).FirstOrDefault();
                    getNotifById.IsRead = (byte)Enums.NotifStatus.HasRead;

                    if (_notifRepo.Update(getNotifById.NotificationId, getNotifById) == ErrorCode.Error)
                    {
                        //Possible error internal upon updating if there is
                        return BadRequest();//temporary return...
                    };
                }



                return View(customTicket);
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult AgentUpdateTicket(CustomEditTicketAssignment customTicket)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return BadRequest();
            }

            TempData["temp"] = "update";

            //current ticket
            var ticket = _db.Tickets.Where(m => m.TicketId == customTicket.Ticket.TicketId).FirstOrDefault();
            int? prevPrioId = ticket.PriorityId;


            if (ticket is null)
            {
                return BadRequest();
            }

            var userTicket = _db.UserTickets.Where(m => m.TicketId == ticket.TicketId).FirstOrDefault();

            if (userTicket is null)
            {
                return BadRequest();
            }

            var categories = _db.Categories.ToList();
            var priorities = _db.Priorities.ToList();
            var status = _db.Statuses.ToList();
            var agents = _db.VwUserRoleViews.Where(m => m.RoleId == 2).ToList();

            customTicket.Category = categories;
            customTicket.Priority = priorities;
            customTicket.Status = status;
            customTicket.Agents = agents;

            //check if there is a chosen priority. if none then set priority to null, otherwise set to selected value   
            if (customTicket.Ticket.PriorityId is null || customTicket.Ticket.PriorityId == 0)
            {
                ticket.PriorityId = null;
            }
            else
            {
                ticket.PriorityId = customTicket.Ticket.PriorityId;
            }

            ticket.CategoryId = customTicket.Ticket.CategoryId;
            ticket.StatusId = customTicket.Ticket.StatusId;

            //basis for choosing between create new or update AssignedTicket, if null then we create, otherwise we update
            var assignedTicket = _db.AssignedTickets.Where(m => m.UserTicketId == userTicket.UserTicketId).FirstOrDefault();


            ticket.LastModified = DateTimeToday();

            if (ticket.StatusId == 3)
            {
                var currentAssignedTicket = _db.AssignedTickets.Where(m => m.UserTicketId == ticket.TicketId).FirstOrDefault();
                ticket.ResolveAt = DateTimeToday().Hour - currentAssignedTicket.DateAssigned.Value.Hour;
            }

            //if no agent is selected then just update the ticket immediately
            if (customTicket.AssignedTicket.AgentId is null || customTicket.AssignedTicket.AgentId == 0)
            {
                if (_ticketRepo.Update(ticket.TicketId, ticket) == ErrorCode.Success)
                {
                    customTicket.Ticket = ticket;
                    customTicket.Agent = null;
                    return RedirectToAction("Details", ticket.TicketId);
                }
                else
                {
                    TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                    {
                        Status = ErrorCode.Error,
                        Message = "An error has occured upon updating the ticket, pls try again."
                    });
                    return View(customTicket);
                }
            }

            var AssignerId = User.FindFirst("UserId")?.Value;

            //if assignedTicket is null then we create new AssignedTicket, otherwise update existing assignedTicket

            if (assignedTicket is null)
            {
                ticket.StatusId = 2;

                assignedTicket = new AssignedTicket()
                {
                    UserTicketId = userTicket.UserTicketId,
                    AssignerId = Convert.ToInt32(AssignerId),
                    AgentId = customTicket.AssignedTicket.AgentId,
                    //DateAssigned = DateTimeToday(),
                    DateAssigned = Utilities.TimeZoneConverter.ConvertTimeZone(DateTime.UtcNow),
                    LastModified = Utilities.TimeZoneConverter.ConvertTimeZone(DateTime.UtcNow),
                };

                if (_ticketRepo.Update(ticket.TicketId, ticket) == ErrorCode.Success)
                {
                    if (_assignedTicketRepo.Create(assignedTicket) == ErrorCode.Success)
                    {
                        var userAgent = _db.Users.Where(m => m.UserId == assignedTicket.AgentId).FirstOrDefault();

                        if (userAgent == null)
                        {
                            return BadRequest();
                        }

                        //trigger notif...
                        string successMsg, errorMsg;
                        NotificationManager _notificationManger = new NotificationManager();
                        string? fromUserName = User.FindFirst(ClaimsIdentity.DefaultNameClaimType)?.Value;
                        string? toUserName = userAgent.Name;

                        /*
                         -----STATUS ID REFERENCE-----
                         Open = 1
                         In progress = 2
                         Resolved = 3
                         Closed = 4  
                         */
                        var hasChangedPriorityId = false;
                        if (prevPrioId != customTicket.Ticket.PriorityId)
                        {
                            hasChangedPriorityId = true;
                        }

                        if (customTicket.Ticket.StatusId == 3)
                        {
                            if (_notificationManger.ResolveTicketNotif((int)assignedTicket.UserTicketId, int.Parse(AssignerId), out errorMsg, out successMsg) == ErrorCode.Success)
                            {
                                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                                {
                                    Status = ErrorCode.Success,
                                    Message = $"Ticket was successfully resolved",
                                });
                            }
                        }
                        else if (_notificationManger.AssignOrReAssignTicketNotif(hasChangedPriorityId, true, (int)assignedTicket.UserTicketId, int.Parse(AssignerId), (int)customTicket.AssignedTicket.AgentId, fromUserName, toUserName, out errorMsg, out successMsg) == ErrorCode.Success)
                        {

                            TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                            {
                                Status = ErrorCode.Success,
                                Message = $"Ticket is successfully assigned to Agent: {userAgent.Name}",
                            });

                        }


                        customTicket.Ticket = ticket;
                        customTicket.AssignedTicket = assignedTicket;
                        customTicket.Agent = userAgent;
                        customTicket.Category = categories;
                        TempData["status"] = ErrorCode.Success;
                        return RedirectToAction("AgentTicketIndex");
                    }
                }
            }
            else
            {
                //RE ASSIGN..
                //the assigned agent of the current ticket before updating the assignedticket (which the agent will be possibly updated/changed)
                var previousAssignedAgent = assignedTicket.AgentId;

                assignedTicket.LastModified = Utilities.TimeZoneConverter.ConvertTimeZone(DateTime.UtcNow);
                assignedTicket.AssignerId = Convert.ToInt32(AssignerId);
                assignedTicket.AgentId = customTicket.AssignedTicket.AgentId;
                var userAgent = _db.Users.Where(m => m.UserId == assignedTicket.AgentId).FirstOrDefault();

                if (userAgent == null)
                {
                    return BadRequest();
                }
                //if (customTicket.AssignedTicket.AgentId is not null)
                //{
                //    ticket.StatusId = 2;
                //}
                ticket.LastModified = DateTimeToday();
                if (_ticketRepo.Update(ticket.TicketId, ticket) == ErrorCode.Success)
                {
                    assignedTicket.LastModified = DateTimeToday();
                    if (_assignedTicketRepo.Update(assignedTicket.AssignedTicketId, assignedTicket) == ErrorCode.Success)
                    {
                        customTicket.Ticket = ticket;
                        customTicket.AssignedTicket = assignedTicket;
                        customTicket.Agent = userAgent;


                        //trigger notif...
                        string successMsg, errorMsg;
                        NotificationManager _notificationManger = new NotificationManager();
                        string fromUserName = User.FindFirst(ClaimsIdentity.DefaultNameClaimType)?.Value;
                        string toUserName = userAgent.Name;

                        /*
                        -----STATUS ID REFERENCE-----
                        Open = 1
                        In progress = 2
                        Resolved = 3
                        Closed = 4  
                        */

                        var hasChangedPriorityId = false;
                        if (prevPrioId != customTicket.Ticket.PriorityId)
                        {
                            hasChangedPriorityId = true;
                        }

                        if (customTicket.Ticket.StatusId == 3)
                        {
                            if (_notificationManger.ResolveTicketNotif((int)assignedTicket.UserTicketId, int.Parse(AssignerId), out errorMsg, out successMsg) == ErrorCode.Success)
                            {
                                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                                {
                                    Status = ErrorCode.Success,
                                    Message = $"Ticket was successfully resolved",
                                });
                            }
                        }
                        else if (_notificationManger.AssignOrReAssignTicketNotif(hasChangedPriorityId, false, (int)assignedTicket.UserTicketId, int.Parse(AssignerId), (int)customTicket.AssignedTicket.AgentId, toUserName, fromUserName, out errorMsg, out successMsg) == ErrorCode.Success)
                        {

                            TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                            {
                                Status = ErrorCode.Success,
                                Message = assignedTicket.AgentId == previousAssignedAgent ? successMsg : $"Ticket is successfully reassigned to Agent: {userAgent.Name}",
                            });

                        }
                        TempData["status"] = ErrorCode.Success;
                        TempData["TicketId"] = ticket.TicketId;
                        return RedirectToAction("Index");
                    }
                }
            }
            return NotFound();
        }

        public IActionResult AgentTicketDetails(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return BadRequest();
            }

            var userId = User.FindFirst("UserId")?.Value;
            var myTicket = _db.VwTicketDetailsViews.Where(m => m.TicketId == id).FirstOrDefault();

            return View(myTicket);
        }

        public IActionResult UserTicketDetails(int id, int? NotificationId) 
        {
            if (!User.Identity.IsAuthenticated)
            {
                return BadRequest();
            }

            var userId = User.FindFirst("UserId")?.Value;
            var myTicket = _db.VwUserTicketViews.Where(m => m.TicketId == id).FirstOrDefault();
            UpdateTicketStatusMessage(myTicket);


            //update notif mark as read if this route is visited from notification view
            if (NotificationId != null)
            {
                var getNotifById = _notifRepo.Table.Where(m => m.NotificationId == NotificationId).FirstOrDefault();
                getNotifById.IsRead = (byte)Enums.NotifStatus.HasRead;

                if (_notifRepo.Update(getNotifById.NotificationId, getNotifById) == ErrorCode.Error)
                {
                    //Possible error internal upon updating if there is
                    return BadRequest();//temporary return...
                };
            }

            return View(myTicket);
        }

        public IActionResult Delete(int id)
        {
            ViewData["temp"] = "delete";
            if (_ticketRepo.Delete(id) == ErrorCode.Success)
            {
                CreateTempDataForAlertContent(ErrorCode.Success, "Ticket deleted successfully.");
                return Ok();
            }
            CreateTempDataForAlertContent(ErrorCode.Error, "An error has occured when deleting the ticket, please try again later.");
            return BadRequest();
        }

        public async Task SetTicketAttachmentPath(CustomTicket customTicket)
        {
            var root = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            var imageFile = customTicket.formFile;

            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            ViewData["root"] = root;
            customTicket.ticket.StatusId = 1;


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
    }
}
