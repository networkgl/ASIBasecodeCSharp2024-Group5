using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.WebApp.Repository;
using ASI.Basecode.WebApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Policy = "SupportAgentPolicy")]
    public class AgentManageTicketController : BaseController
    {
        public AgentManageTicketController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }
        [HttpGet("ManageTickets")]
        public IActionResult Index()
        {
            HandleTempDataMessages();

            if (User.Identity.IsAuthenticated)
            {
                var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

                var myTickets = _db.VwTicketDetailsViews.ToList().OrderByDescending(m => m.TicketId);
                ViewData["TableId"] = "agentManageTicketsTable";
                ViewData["Priorities"] = _db.Priorities.ToList();
                ViewData["Categories"] = _db.Categories.ToList();
                ViewData["Statuses"] = _db.Statuses.ToList();
                return View(myTickets);
            }

            return NotFound();
        }

        [HttpGet("ManageTickets/{id}")]
        public IActionResult Index(int id)
        {
            HandleTempDataMessages();

            if (User.Identity.IsAuthenticated)
            {
                var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

                var myTickets = _db.VwTicketDetailsViews.ToList().OrderByDescending(m => m.TicketId);
                ViewData["TableId"] = "agentManageTicketsTable";
                ViewBag.Priorities = _db.Priorities.ToList();
                ViewBag.Categories = _db.Categories.ToList();
                ViewBag.Statuses = _db.Statuses.ToList();
                return View(myTickets);
            }

            return NotFound();
        }

        //[HttpGet("UpdateTicket/{id}")]
        [HttpGet]
        public IActionResult Edit(int id)
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

                return View(customTicket);
            }
            return NotFound();
        }

        //[HttpPost("UpdateTicket/{id}")]
        [HttpPost]
        public IActionResult Edit(CustomEditTicketAssignment customTicket)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return BadRequest();
            }
            TempData["temp"] = "update";

            //current ticket
            var ticket = _db.Tickets.Where(m => m.TicketId == customTicket.Ticket.TicketId).FirstOrDefault();

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
                        else if (_notificationManger.AssignOrReAssignTicketNotif(true, (int)assignedTicket.UserTicketId, int.Parse(AssignerId), (int)customTicket.AssignedTicket.AgentId, fromUserName, toUserName, out errorMsg, out successMsg) == ErrorCode.Success)
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
                        return RedirectToAction("Index");
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
                        else if (_notificationManger.AssignOrReAssignTicketNotif(false, (int)assignedTicket.UserTicketId, int.Parse(AssignerId), (int)customTicket.AssignedTicket.AgentId, toUserName, fromUserName, out errorMsg, out successMsg) == ErrorCode.Success)
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

        public IActionResult Details(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return BadRequest();
            }
            //if (TempData["TicketId"] != null)
            //{
            //    id = Convert.ToInt32(ViewData["TicketId"]);
            //}

            var userId = User.FindFirst("UserId")?.Value;
            var myTicket = _db.VwTicketDetailsViews.Where(m => m.TicketId == id).FirstOrDefault();

            return View(myTicket);
        }

        private void HandleTempDataMessages()
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

        //private void HandleCreateTempData()
        //{
        //    if (TempData["ResMsg"] is not null)
        //    {
        //        var resMsg = JsonConvert.DeserializeObject<AlertMessageContent>(TempData["ResMsg"].ToString());
        //        if (resMsg is not null && User.Identity.IsAuthenticated)
        //        {
        //            TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent
        //            {
        //                Status = resMsg.Status,
        //                Message = resMsg.Message
        //            });
        //        }
        //    }
        //}

        //private void HandleDeleteTempData()
        //{
        //    var resMsg = JsonConvert.DeserializeObject<AlertMessageContent>(TempData["ResMsg"].ToString());
        //    TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent
        //    {
        //        Status = status == 0 ? ErrorCode.Success : ErrorCode.Error,
        //        Message = status == 0
        //            ? "A ticket has been deleted successfully!"
        //            : "An error has occurred upon deleting the ticket."
        //    });
        //}

        //private void HandleUpdateTempData()
        //{
        //    var resMsg = JsonConvert.DeserializeObject<AlertMessageContent>(TempData["ResMsg"].ToString());
        //    TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent
        //    {
        //        Status = status == 0 ? ErrorCode.Success : ErrorCode.Error,
        //        Message = status == 0
        //            ? "Ticket updated successfully!"
        //            : "An error has occurred when updating your ticket, please try again."
        //    });
        //}
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
    } 
}
