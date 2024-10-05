using ASI.Basecode.WebApp.Controllers;
using ASI.Basecode.WebApp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Interfaces;
using Microsoft.AspNetCore.Http;

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

        public ErrorCode CreateTicketNotif(int toUserId, string? userName, int categoryId, int userTicketId, out string errorMsg, out string successMsg)
        {
            errorMsg = successMsg = string.Empty;

            try
            {
                var userNotif = new Notification()
                {
                    ToUserId = toUserId,
                    UserTicketId = userTicketId,
                    Content = ASI.Basecode.WebApp.Utils.Constant.USER_SUCCESS_TICKET_MESSAGE
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
                if (isAssigning)
                {
                    var userAssignedNotif = new Notification()
                    {
                        FromUserId = assignedToId,
                        ToUserId = assignedToId,
                        UserTicketId = userTicketId,
                        Content = string.Format(ASI.Basecode.WebApp.Utils.Constant.TICKET_ASSIGNED_MESSAGE_FOR_ASSIGNED_USER, toUserName)
                    };

                    if (_notifRepo.Create(userAssignedNotif) == ErrorCode.Error)
                    {
                        return ErrorCode.Error;
                    }

                    var assignerNotif = new Notification()
                    {
                        ToUserId = assignerId,
                        UserTicketId = userTicketId,
                        Content = string.Format(ASI.Basecode.WebApp.Utils.Constant.TICKET_ASSIGNED_MESSAGE_FOR_ASSIGNER, fromUserName)
                    };

                    if (_notifRepo.Create(assignerNotif) == ErrorCode.Error)
                    {
                        return ErrorCode.Error;
                    }

                    successsMsg = $"Ticket is successfully assigned to agent: {toUserName}";
                }
                else //meaning update ra ang ticket base sa new supp agent nga ma assign nan ani...
                {
                    var userAssignedNotif = new Notification()
                    {
                        FromUserId = assignedToId,
                        ToUserId = assignedToId,
                        UserTicketId = userTicketId,
                        Content = string.Format(ASI.Basecode.WebApp.Utils.Constant.TICKET_RE_ASSIGNED_MESSAGE_FOR_ASSIGNED_USER, toUserName)
                    };

                    if (_notifRepo.Create(userAssignedNotif) == ErrorCode.Error)
                    {
                        return ErrorCode.Error;
                    }

                    var assignerNotif = new Notification()
                    {
                        ToUserId = assignerId,
                        UserTicketId = userTicketId,
                        Content = string.Format(ASI.Basecode.WebApp.Utils.Constant.TICKET_RE_ASSIGNED_MESSAGE_FOR_ASSIGNER, fromUserName)
                    };

                    if (_notifRepo.Create(assignerNotif) == ErrorCode.Error)
                    {
                        return ErrorCode.Error;
                    }

                    successsMsg = $"Ticket is successfully re-assigned to another agent: {toUserName}";
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
