using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Controllers;
using ASI.Basecode.WebApp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Repository
{
    public class NotificationManager : BaseController
    {
        public NotificationManager() { }

        public List<Notification> GetUserAssociatedNotif(int? userId)
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
                    Content = string.Format(Constant.USER_SUCCESS_TICKET_MESSAGE, userTicketId),
                    CreatedAt = DateTimeToday()
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
                                Constant.SUPPORT_AGENT_TICKET_NOTIFICATION,
                                userName,
                                toUserId,
                                ticketId,
                                subjectOrCategoryName

                    );
                    suppAgentNotif.CreatedAt = DateTimeToday();

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

        public int? GetUserNotifCount(int? userId)
        {
            return _notifRepo.Table.Where(m => m.ToUserId == userId).Count();
        }



        public ErrorCode AssignOrReAssignTicketNotif(bool hasChangedPriorityId, bool isAssigning, int userTicketId, int assignerId, int assignedToId, string toUserName, string fromUserName, out string errorMsg, out string successsMsg)
        {
            errorMsg = successsMsg = string.Empty;
            try
            {
                //var assignedTicket = _db.AssignedTickets.Where(m => m.UserTicketId == userTicketId).FirstOrDefault();
                var userTicket = _db.VwNotificationViews.Where(m => m.UserTicketId == userTicketId).FirstOrDefault();

                if (userTicket is null)
                {
                    return ErrorCode.Error;
                }

                //the assigned agent of the current ticket before updating the assignedticket (which the agent will be possibly updated/changed)
                var previousAssignedAgent = userTicket.AgentId;
                if (isAssigning)
                {
                    var userAssignedNotif = new Notification()
                    {
                        FromUserId = assignerId,
                        ToUserId = assignedToId,
                        UserTicketId = userTicketId,
                        Content = string.Format(Constant.TICKET_ASSIGNED_MESSAGE_FOR_ASSIGNED_USER, toUserName),
                        CreatedAt = DateTimeToday()
                    };

                    if (_notifRepo.Create(userAssignedNotif) == ErrorCode.Error)
                    {
                        return ErrorCode.Error;
                    }

                    var assignerNotif = new Notification()
                    {
                        ToUserId = assignerId,
                        UserTicketId = userTicketId,
                        Content = string.Format(Constant.TICKET_ASSIGNED_MESSAGE_FOR_ASSIGNER, fromUserName),
                        CreatedAt = DateTimeToday()
                    };

                    if (_notifRepo.Create(assignerNotif) == ErrorCode.Error)
                    {
                        return ErrorCode.Error;
                    }


                    //trigger also notif for the user who owns the ticket...
                    var userNotif = new Notification()
                    {
                        ToUserId = userTicket.UserId,
                        UserTicketId = userTicketId,
                        Content = $"Your ticket has been assigned by our support agent. Ticket ID: {userTicket.TicketId}",
                        CreatedAt = DateTimeToday()
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
                        Content = string.Format(Constant.TICKET_RE_ASSIGNED_MESSAGE_FOR_ASSIGNED_USER, toUserName),
                        CreatedAt = DateTimeToday()
                    };

                    if (_notifRepo.Create(userAssignedNotif) == ErrorCode.Error)
                    {
                        return ErrorCode.Error;
                    }

                    var assignerNotif = new Notification()
                    {
                        ToUserId = assignerId,
                        UserTicketId = userTicketId,
                        Content = string.Format(Constant.TICKET_RE_ASSIGNED_MESSAGE_FOR_ASSIGNER, fromUserName),
                        CreatedAt = DateTimeToday()
                    };

                    if (_notifRepo.Create(assignerNotif) == ErrorCode.Error)
                    {
                        return ErrorCode.Error;
                    }



                    string notifContent = string.Empty;
                    if (hasChangedPriorityId)
                    {
                        //trigger notif for updating the prio level
                        var notifPrioLevelChange = new Notification()
                        {
                            ToUserId = userTicket.UserId,
                            UserTicketId = userTicketId,
                            Content = $"Good day! Your ticket priority level has been updated to our support agent to matched prior to the issue. Ticket ID: {userTicket.TicketId}. Please be guided.",
                            CreatedAt = DateTimeToday()
                        };

                        if (_notifRepo.Create(notifPrioLevelChange) == ErrorCode.Error)
                        {
                            return ErrorCode.Error;
                        }
                    }

                    //trigger also notif for the user who owns the ticket...
                    var userNotif = new Notification()
                    {
                        ToUserId = userTicket.UserId,
                        UserTicketId = userTicketId,
                        Content = $"Good day! Your ticket has been re-assigned to our another support agent. Ticket ID: {userTicket.TicketId}. Please be guided.",
                        CreatedAt = DateTimeToday()
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

                var userTickets = _db.VwNotificationViews.Where(m => m.UserTicketId == userTicketId).FirstOrDefault();

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
                    Content = $"Good News! Your ticket has been resolved by our support agent team Ticket ID: {userTickets.TicketId}. If you have any concern or more please don't hesitate to reach us. Thank you!",
                    CreatedAt = DateTimeToday()
                };

                if (_notifRepo.Create(userNotif) == ErrorCode.Error)
                {
                    return ErrorCode.Error;
                }

                //also send notif for the supp agent who was assign the resolve ticket...
                var suppAgentNotif = new Notification()
                {
                    ToUserId = userTickets.AgentId,
                    UserTicketId = userTicketId,
                    Content = $"Job well done! You have successfully resolved user concern that has Ticket ID: {userTicketId}. Thank you!",
                    CreatedAt = DateTimeToday()
                };

                if (_notifRepo.Create(suppAgentNotif) == ErrorCode.Error)
                {
                    return ErrorCode.Error;
                }

                successMsg = "Ticket successfully resolve!";
            }
            catch (Exception e)
            {
                errorMsg = e.InnerException == null || e.InnerException.InnerException == null ? e.Message : e.InnerException.InnerException.Message;
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }


        protected ErrorCode MarkUserNotifAllAsRead(int userId, byte? value, out string errorMsg, out string successMsg)
        {
            errorMsg = successMsg = string.Empty;

            try
            {
                var getUserNotifById = _notifRepo.Table.Where(m => m.ToUserId == userId).ToList();

                foreach (var notif in getUserNotifById)
                {
                    var updateNotif = _notifRepo.Table.Where(m => m.NotificationId == notif.NotificationId).FirstOrDefault();
                    updateNotif.IsRead = (byte)NotifStatus.HasRead;

                    if (_notifRepo.Update(notif.NotificationId, updateNotif) == ErrorCode.Error)
                    {
                        return ErrorCode.Error;
                    }
                }

                successMsg = "Successfully mark all as read.";
            }
            catch (Exception e)
            {
                errorMsg = e.InnerException == null || e.InnerException.InnerException == null ? e.Message : e.InnerException.InnerException.Message;
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }




        public ErrorCode RemindTicketNotif(out string errorMsg, out string successMsg)
        {
            errorMsg = successMsg = string.Empty;

            try
            {
                // Fetch the current UTC time, converted to the application's timezone
                var currentTime = Utilities.TimeZoneConverter.ConvertTimeZone(DateTime.UtcNow);

                // Loop through the tickets and send reminders before the due date
                var tickets = _db.VwNotificationViews
                    .Where(m => m.DateAssigned.HasValue && m.ResolutionTime.HasValue) // Ensure DateAssigned and ResolutionTime are present
                    .ToList();

                foreach (var ticket in tickets)
                {
                    // Calculate the due date for the ticket (DateAssigned + ResolutionTime)
                    var dueDate = ticket.DateAssigned.Value.AddHours((double)ticket.ResolutionTime);

                    int hoursBeforeTrigger = 5; // Fixed hours before due date (STATIC declared)
                    var reminderThreshold = TimeSpan.FromHours(hoursBeforeTrigger);

                    // Determine the reminder date (when the notification should be sent)
                    var reminderDate = dueDate - reminderThreshold;

                    // Check if the current time is within the reminder window (and the ticket is not yet overdue)
                    if (currentTime >= reminderDate && currentTime < dueDate)
                    {
                        // Send reminder to the assigned user
                        var suppAgentNotif = new Notification()
                        {
                            ToUserId = ticket.AgentId,  // Notify the assigned support agent
                            UserTicketId = ticket.TicketId,
                            Content = $"Unresolved Ticket Reminder for Ticket ID: {ticket.TicketId} Date Assigned: {ticket.DateAssigned} Hours To Be Resolve: {ticket.ResolutionTime}. Please resolve this ticket within {hoursBeforeTrigger} hours immediately!",
                            CreatedAt = DateTimeToday()
                        };

                        if (_notifRepo.Create(suppAgentNotif) == ErrorCode.Error)
                        {
                            return ErrorCode.Error;
                        }

                        //notify also the user who owns the ticket...
                        var userNotif = new Notification()
                        {
                            ToUserId = ticket.UserId,  // Notify the user also 
                            UserTicketId = ticket.TicketId,
                            Content = $"We have noticed that your concerned with Ticket ID: {ticket.TicketId} has not been resolved. We already notified the assigned agent for this matter and promised to resolve this urgently. Thank you for your patience!",
                            CreatedAt = DateTimeToday()
                        };

                        if (_notifRepo.Create(userNotif) == ErrorCode.Error)
                        {
                            return ErrorCode.Error;
                        }


                    }
                }

                successMsg = "Upcoming due date reminders sent successfully.";
            }
            catch (Exception e)
            {
                // Handle exceptions and set error message
                errorMsg = e.InnerException?.InnerException?.Message ?? e.Message;
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }


    }
}
