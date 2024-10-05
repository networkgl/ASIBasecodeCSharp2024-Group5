using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.WebApp.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Controllers
{
    public class AgentManageTicketController : BaseController
    {
        public AgentManageTicketController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
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
                var userId = User.FindFirst("UserId")?.Value;
                var myTickets = _db.VwTicketAssignments.ToList();

                return View(myTickets);
            }
            return NotFound();
        }

        public IActionResult Edit(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var customTicket = new CustomEditTicketAssignment();
                var ticketAssignment = _db.VwTicketAssignments.Where(m => m.TicketId == id).FirstOrDefault();

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

        [HttpPost]
        public IActionResult Edit(CustomEditTicketAssignment customTicket)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return BadRequest();
            }

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

            //check if there is a chosen priority, if none then set priority to null or zero, otherwise set the selected value
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

            //basis for choosing between create new or update Resolve, if null then we create, otherwise we update
            var assignedTicket = _db.AssignedTickets.Where(m => m.UserTicketId == userTicket.UserTicketId).FirstOrDefault();

            //if no agent is selected then just update the ticket immediately
            if (customTicket.AssignedTicket.AgentId is null || customTicket.AssignedTicket.AgentId == 0)
            {
                if (_ticketRepo.Update(ticket.TicketId, ticket) == ErrorCode.Success)
                {
                    TempData["ResMsg"] = new AlertMessageContent()
                    {
                        Status = ErrorCode.Success,
                        Message = "Ticket is updated successfully!"
                    };
                    customTicket.Ticket = ticket;
                    customTicket.Agent = null;
                    return View(customTicket);
                }
                else
                {
                    TempData["ResMsg"] = new AlertMessageContent()
                    {
                        Status = ErrorCode.Error,
                        Message = "An error has occured upon updating the ticket, pls try again."
                    };
                    return View(customTicket);
                }
            }

            var AssignerId = User.FindFirst("UserId")?.Value;
            //if resolve is null then we create new resolve, otherwise update existing resolve
            if (assignedTicket is null)
            {
                assignedTicket = new AssignedTicket()
                {
                    UserTicketId = userTicket.UserTicketId,
                    AssignerId = Convert.ToInt32(AssignerId),
                    AgentId = customTicket.AssignedTicket.AgentId,
                    DateAssigned = DateTimeToday(),
                    LastModified = DateTimeToday(),
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
                        if (_notificationManger.AssignOrReAssignTicketNotif(true, (int)assignedTicket.UserTicketId, int.Parse(AssignerId), (int)customTicket.AssignedTicket.AgentId, fromUserName, toUserName, out errorMsg, out successMsg) == ErrorCode.Success)
                        {

                            TempData["ResMsg"] = new AlertMessageContent()
                            {
                                Status = ErrorCode.Success,
                                Message = "Ticket is updated successfully",
                            };

                        }
       

                        customTicket.Ticket = ticket;
                        customTicket.AssignedTicket = assignedTicket;
                        customTicket.Agent = userAgent;
                        return View(customTicket);
                    }
                }
            }
            else
            {
                assignedTicket.LastModified = DateTimeToday();
                assignedTicket.AssignerId = Convert.ToInt32(AssignerId);
                assignedTicket.AgentId = customTicket.AssignedTicket.AgentId;
                var userAgent = _db.Users.Where(m => m.UserId == assignedTicket.AgentId).FirstOrDefault();

                if (userAgent == null)
                {
                    return BadRequest();
                }

                if (_ticketRepo.Update(ticket.TicketId, ticket) == ErrorCode.Success)
                {
                    if (_assignedTicketRepo.Update(assignedTicket.AssignedTicketId, assignedTicket) == ErrorCode.Success)
                    {
        
                        customTicket.Ticket = ticket;
                        customTicket.AssignedTicket = assignedTicket;
                        customTicket.Agent = userAgent;


                        //trigger notif...
                        string successMsg, errorMsg;
                        NotificationManager _notificationManger = new NotificationManager();
                        string? fromUserName = User.FindFirst(ClaimsIdentity.DefaultNameClaimType)?.Value;
                        string? toUserName = userAgent.Name;
                        if (_notificationManger.AssignOrReAssignTicketNotif(true, (int)assignedTicket.UserTicketId, int.Parse(AssignerId), (int)customTicket.AssignedTicket.AgentId, toUserName, fromUserName, out errorMsg, out successMsg) == ErrorCode.Success)
                        {

                            TempData["ResMsg"] = new AlertMessageContent()
                            {
                                Status = ErrorCode.Success,
                                Message = successMsg,
                            };

                        }


                        return View(customTicket);
                    }
                }
            }
            return NotFound();
        }
    }
}
