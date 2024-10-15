using ASI.Basecode.WebApp.Controllers;
using ASI.Basecode.WebApp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Interfaces;
using Microsoft.AspNetCore.Http;
using ASI.Basecode.Resources.Messages;

namespace ASI.Basecode.WebApp.Repository
{
    public class NotificationManager : BaseController
    {
        public NotificationManager()
        {
        }
        protected List<Notification> GetUserAssociatedNotif(int? userId)
        {
            return _notifRepo.Table.Where(m => m.ToUserId == userId).OrderByDescending(m => m.CreatedAt).ToList();
        }

        public ErrorCode CreateTicketNotif(int toUserId, string userName, int categoryId, int userTicketId, out string errorMsg, out string successMsg)
        {
            errorMsg = successMsg = string.Empty;

            try
            {
                var userNotif = new Notification()
                {
                    ToUserId = toUserId,
                    UserTicketId = userTicketId,
                    Content = string.Format(Constant.USER_SUCCESS_TICKET_MESSAGE, userTicketId)
                };

                if (_notifRepo.Create(userNotif) == ErrorCode.Error)//for user who create the notif
                {
                    return ErrorCode.Error;
                }


                //Get all UserId associated with the role type of Supp Agent...
                var suppAgentId = _db.VwUserRoleViews.Where(m => m.RoleId == (int)RoleType.SupportAgent).Select(m => m.UserId).ToArray();
                int? ticketId = _userTicketRepo.Table.Where(m => m.UserTicketId == userTicketId).Select(m => m.TicketId).FirstOrDefault();

                string subjectOrCategoryName = _catRepo.Table
                    .Where(m => m.CategoryId == categoryId)
                    .Select(m => m.CategoryName)
                    .FirstOrDefault() ?? "Unknown Category";

                for (int i = 0; i < suppAgentId.Count(); i++)
                {
                    var suppAgentNotif = new Notification();
                    suppAgentNotif.FromUserId = toUserId; //pasabot ani kinsa ang actor...
                    suppAgentNotif.ToUserId = suppAgentId[i];
                    suppAgentNotif.UserTicketId = userTicketId;
                    suppAgentNotif.Content = string.Format(
                                ASI.Basecode.WebApp.Utils.Constant.SUPPORT_AGENT_TICKET_NOTIFICATION,
                                userName,
                                toUserId,
                                ticketId,
                                subjectOrCategoryName
                    );

                    if (_notifRepo.Create(suppAgentNotif) == ErrorCode.Error) //for all support agents..
                    {
                        return ErrorCode.Error;
                    }

                }

                successMsg = "Ticket submitted successfully!";
            }
            catch (Exception e)
            {
                errorMsg = e.InnerException == null || e.InnerException.InnerException == null ? e.Message : e.InnerException.InnerException.Message;
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }

        public ErrorCode AssignOrReAssignTicketNotif(bool isAssigning, int userTicketId, int assignerId, int assignedToId, string toUserName, string fromUserName, out string errorMsg, out string successsMsg)
        {
            errorMsg = successsMsg = string.Empty;
            try
            {
                //var assignedTicket = _db.AssignedTickets.Where(m => m.UserTicketId == userTicketId).FirstOrDefault();
                var assignedTicket = _db.VwAssignedTicketViews.Where(m => m.UserTicketId == userTicketId).FirstOrDefault();
                
                if (assignedTicket is null)
                {
                    return ErrorCode.Error;
                }

                //the assigned agent of the current ticket before updating the assignedticket (which the agent will be possibly updated/changed)
                var previousAssignedAgent = assignedTicket.AgentId;
                if (isAssigning)
                {
                    var userAssignedNotif = new Notification()
                    {
                        FromUserId = assignerId,
                        ToUserId = assignedToId,
                        UserTicketId = userTicketId,
                        Content = string.Format(Constant.TICKET_ASSIGNED_MESSAGE_FOR_ASSIGNED_USER, toUserName)
                    };

                    if (_notifRepo.Create(userAssignedNotif) == ErrorCode.Error)
                    {
                        return ErrorCode.Error;
                    }

                    var assignerNotif = new Notification()
                    {
                        ToUserId = assignerId,
                        UserTicketId = userTicketId,
                        Content = string.Format(Constant.TICKET_ASSIGNED_MESSAGE_FOR_ASSIGNER, fromUserName)
                    };

                    if (_notifRepo.Create(assignerNotif) == ErrorCode.Error)
                    {
                        return ErrorCode.Error;
                    }


                    //trigger also notif for the user who owns the ticket...
                    var userNotif = new Notification()
                    {
                        ToUserId = assignedTicket.UserId,
                        UserTicketId = userTicketId,
                        Content = $"Your ticket has been assigned by our support agent. Ticket ID: {assignedTicket.TicketId}"
                    };

                    if (_notifRepo.Create(userNotif) == ErrorCode.Error)
                    {
                        return ErrorCode.Error;
                    }

                    successsMsg = previousAssignedAgent == assignedToId ? "Ticket is updated successfully!" : $"Ticket is successfully assigned to agent: {toUserName}";
                }
                else //meaning re-assigning or update ra ang ticket base sa new supp agent nga ma assign nan ani...
                {
                    var userAssignedNotif = new Notification()
                    {
                        FromUserId = assignerId,
                        ToUserId = assignedToId,
                        UserTicketId = userTicketId,
                        Content = string.Format(Constant.TICKET_RE_ASSIGNED_MESSAGE_FOR_ASSIGNED_USER, toUserName)
                    };

                    if (_notifRepo.Create(userAssignedNotif) == ErrorCode.Error)
                    {
                        return ErrorCode.Error;
                    }

                    var assignerNotif = new Notification()
                    {
                        ToUserId = assignerId,
                        UserTicketId = userTicketId,
                        Content = string.Format(Constant.TICKET_RE_ASSIGNED_MESSAGE_FOR_ASSIGNER, fromUserName)
                    };

                    if (_notifRepo.Create(assignerNotif) == ErrorCode.Error)
                    {
                        return ErrorCode.Error;
                    }


                    //trigger also notif for the user who owns the ticket...
                    var userNotif = new Notification()
                    {
                        ToUserId = assignedTicket.UserId,
                        UserTicketId = userTicketId,
                        Content = $"Good day! Your ticket has been re-assigned to our another support agent. Ticket ID: {assignedTicket.TicketId}. Please be guided."
                    };

                    if (_notifRepo.Create(userNotif) == ErrorCode.Error)
                    {
                        return ErrorCode.Error;
                    }

                    successsMsg = previousAssignedAgent == assignedToId ? "Ticket is updated successfully!" : $"Ticket is successfully re-assigned to another agent: {toUserName}";
                }


            }
            catch (Exception e)
            {
                errorMsg = e.InnerException == null || e.InnerException.InnerException == null ? e.Message : e.InnerException.InnerException.Message;
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
    
        public ErrorCode ResolveTicketNotif(int userTicketId, int userIdActor, out string errorMsg, out string successMsg)
        {
            errorMsg = successMsg = string.Empty;

            try
            {

                var userTickets = _db.VwAssignedTicketViews.Where(m => m.UserTicketId == userTicketId).FirstOrDefault();

                if (userTickets is null)
                {
                    return ErrorCode.Error;
                }

                //trigger notif about the ticket resolved...
                var userNotif = new Notification()
                {
                    FromUserId = userIdActor,
                    ToUserId = userTickets.UserId,
                    UserTicketId = userTicketId,
                    Content = $"Good News! Your ticket has been resolved by our support agent team Ticket ID: {userTickets.TicketId}. If you have any concern or more please don't hesitate to reach us. Thank you!"
                };

                if (_notifRepo.Create(userNotif) == ErrorCode.Error)
                {
                    return ErrorCode.Error;
                }


            }
            catch (Exception e)
            {
                errorMsg = e.InnerException == null || e.InnerException.InnerException == null ? e.Message : e.InnerException.InnerException.Message;
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
    }
}
