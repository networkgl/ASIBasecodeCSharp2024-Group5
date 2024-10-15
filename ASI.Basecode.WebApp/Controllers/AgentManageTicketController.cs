using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.WebApp.Repository;
using ASI.Basecode.WebApp.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
            if (TempData["temp"] is not null)
            {
                if ((string)TempData["temp"] == "create")
                {
                    var resMsg = JsonConvert.DeserializeObject<AlertMessageContent>(TempData["ResMsg"].ToString());

                    if (resMsg is not null)
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                            {
                                Status = resMsg.Status,
                                Message = resMsg.Message
                            });
                            var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);
                            var myTickets = _db.VwUserTicketViews.Where(m => m.UserId == userId).ToList();

                            return View(myTickets);
                        }
                    }
                }
                if ((string)TempData["temp"] == "delete")
                {
                    if (TempData["status"] as int? == 0)
                    {
                        TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                        {
                            Status = ErrorCode.Success,
                            Message = "A ticket has deleted successfully!"
                        });
                    }
                    else
                    {
                        TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                        {
                            Status = ErrorCode.Error,
                            Message = "An error has occured upon deleting the ticket."
                        });
                    }
                }

                if ((string)TempData["temp"] == "update")
                {
                    var resMsg = JsonConvert.DeserializeObject<AlertMessageContent>(TempData["ResMsg"].ToString());
                    
                    if(resMsg is not null)
                    {
                        TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                        {
                            Status = resMsg.Status,
                            Message = resMsg.Message
                        });
                    }
                }
            }
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst("UserId")?.Value;
                var myTickets = _db.VwTicketDetailsViews.ToList();

                return View(myTickets);
            }
            return NotFound();
        }

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

        [HttpPost]
        public IActionResult Edit(CustomEditTicketAssignment customTicket)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return BadRequest();
            }

            //if (customTicket.AssignedTicket.AgentId is null)
            //{
            //    TempData["ResMsg"] = new AlertMessageContent()
            //    {
            //        Status = ErrorCode.Error,
            //        Message = "Cant update the priority if this ticket is unassigned!"
            //    };
            //    return View();
            //}

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

            //basis for choosing between create new or update AssignedTicket, if null then we create, otherwise we update
            var assignedTicket = _db.AssignedTickets.Where(m => m.UserTicketId == userTicket.UserTicketId).FirstOrDefault();

            TempData["temp"] = "update";

            ticket.LastModified = DateTimeToday();
            //if no agent is selected then just update the ticket immediately
            if (customTicket.AssignedTicket.AgentId is null || customTicket.AssignedTicket.AgentId == 0)
            {
                if (_ticketRepo.Update(ticket.TicketId, ticket) == ErrorCode.Success)
                {
                    customTicket.Ticket = ticket;
                    customTicket.Agent = null;
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                    {
                        Status = ErrorCode.Error,
                        Message = "An error has occured upon updating the ticket, pls try again."
                    });
                    return RedirectToAction("Index");
                }
            }

            var AssignerId = User.FindFirst("UserId")?.Value;

            //if assignedTicket is null then we create new resolve, otherwise update existing assignedTicket
            if (assignedTicket is null)
            {
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
                        return RedirectToAction("Index");
                    }
                }
            }
            return NotFound();
        }
    }
}
