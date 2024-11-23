using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.Resources.Messages;
using ASI.Basecode.Services.Controllers;
using ASI.Basecode.Services.Repository;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ASI.Basecode.Services.Services
{
    public class TicketServices : BaseController
    {
        public TicketServices() 
        { 
        }
        //UserTicketController:
        public List<VwUserTicketView> UserMyTicketIndex()
        {
            var userId = GetLoggedInUserId();
            var myTickets = _db.VwUserTicketViews
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.TicketId).ToList()
                .ToList();

            UpdateTicketStatusMessages(myTickets);
            PopulateCustomTicketViewData();

            return myTickets;
        }
        public List<VwTicketDetailsView> SupportAgentTicketIndex()
        {
            var userId = GetLoggedInUserId();
            var tickets = _db.VwTicketDetailsViews.OrderByDescending(m => m.TicketId).ToList();

            PopulateCustomTicketViewData();

            return tickets;
        }
        public List<VwTicketDetailsView> AdminTicketIndex()
        {
            var userId = GetLoggedInUserId();
            var tickets = _db.VwTicketDetailsViews.OrderByDescending(m => m.TicketId).ToList();

            PopulateCustomTicketViewData();

            return tickets;
        }
        public ErrorCode UserTicketCreate(CustomTicket customTicket, int userId, out string errorMsg, out string successMsg)
        {
            errorMsg = string.Empty;
            successMsg = string.Empty;

            if (_ticketRepo.Create(customTicket.ticket) == ErrorCode.Success)
            {
                var userTicket = new UserTicket
                {
                    UserId = userId,
                    TicketId = customTicket.ticket.TicketId
                };

                if (_userTicketRepo.Create(userTicket) == ErrorCode.Success)
                {
                    NotificationManager _notifManager = new NotificationManager();
                    string? userName = User.FindFirst(ClaimsIdentity.DefaultNameClaimType)?.Value;

                    if (_notifManager.CreateTicketNotif(userId, userName, (int)customTicket.ticket.CategoryId, userTicket.UserTicketId, out errorMsg, out successMsg) == ErrorCode.Success)
                    {
                        ViewData["temp"] = "create";
                        ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                        {
                            Status = ErrorCode.Success,
                            Message = successMsg
                        });
                        return ErrorCode.Success;
                    }
                }
            }
            return ErrorCode.Error;
        }
        public void CreateTempDataForAlertContent(ErrorCode errorCode, string message)
        {
            TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
            {
                Status = errorCode,
                Message = message
            });
        }
        public void PopulateCustomTicket(CustomTicket customTicket)
        {
            customTicket.category = _db.Categories.ToList();
            customTicket.priority = _db.Priorities.ToList();
            customTicket.status = _db.Statuses.ToList();
        }
        public void HandleTempDataMessages()
        {
            if (TempData["ResMsg"] is not null)
            {
                var resMsg = JsonConvert.DeserializeObject<AlertMessageContent>(TempData["ResMsg"].ToString());
                if (resMsg is not null && User.Identity.IsAuthenticated)
                {
                    ViewData["ResMsg"] = JsonConvert.SerializeObject(resMsg);
                }
            }
        }
        public ErrorCode ValidateInput(CustomTicket customTicket)
        {
            if (customTicket.ticket.CategoryId == null || string.IsNullOrEmpty(customTicket.ticket.IssueDescription))
            {
                CreateTempDataForAlertContent(ErrorCode.Error, "Please provide a description of the issue. This information is required to assist you effectively.");
                return ErrorCode.Error;
            }
            return ErrorCode.Success;
        }
        public void SetDateToday(CustomTicket customTicket)
        {
            customTicket.ticket.CreateAt = DateTimeToday();
            customTicket.ticket.LastModified = DateTimeToday();
        }
        public void UpdateTicketStatusMessages(List<VwUserTicketView> tickets)
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
        public void UpdateTicketStatusMessage(VwUserTicketView ticket)
        {
            ticket.StatusName = ticket.StatusId switch
            {
                1 => "Your ticket is pending assignment to an agent.",
                2 => "Your ticket is currently in progress. An agent is working on it.",
                3 => "Your ticket has been resolved.",
                4 => "Your ticket is closed.",
                _ => "Status is unknown. Please contact support for more information."
            };
        }
        public void PopulateCustomTicketViewData()
        {
            switch (UserRoleName().ToLower())
            {
                case "user":
                    ViewData["TableId"] = "myTicketsTable";
                    break;
                case "support agent":
                    ViewData["TableId"] = "agentManageTicketsTable";
                    break;
                case "administrator":
                    ViewData["TableId"] = "adminManageTicketsTable";
                    break;
                default:
                    break;
            }
            ViewData["Priorities"] = _db.Priorities.ToList();
            ViewData["Categories"] = _db.Categories.ToList();
            ViewData["Statuses"] = _db.Statuses.ToList();
        }
        public string UserRoleName()
        {
            return User.FindFirst("UserRole")?.Value.ToString();
        }

    }
}
