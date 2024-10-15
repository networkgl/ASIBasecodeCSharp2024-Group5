using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.WebApp.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using System.Drawing;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    public class BaseController : Controller
    {
        protected ISession _session;
        public TicketingSystemDBContext _db;
        public AssisthubDBContext _db1; //temporary rani
        public BaseRepository<User> _userRepo;
        public BaseRepository<VwUserRoleView> _vw_UserRoleView;
        public BaseRepository<UserRole> _userRoleRepo;
        public BaseRepository<VwUsersAndAgentsView> _vw_UsersAndAgentsView;
        public BaseRepository<Ticket> _ticketRepo;
        public BaseRepository<UserTicket> _userTicketRepo;
        public BaseRepository<AssignedTicket> _assignedTicketRepo;
        public BaseRepository<Notification> _notifRepo;
        public BaseRepository<Category> _catRepo;
        public BaseRepository<Article> _articleRepo;

        public BaseController(IHttpContextAccessor httpContextAccessor)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _db = new TicketingSystemDBContext();
            _db1 = new AssisthubDBContext();
            _userRepo = new BaseRepository<User>();
            _vw_UserRoleView = new BaseRepository<VwUserRoleView>();
            _userRoleRepo = new BaseRepository<UserRole>();
            _vw_UsersAndAgentsView = new BaseRepository<VwUsersAndAgentsView>();
            _ticketRepo = new BaseRepository<Ticket>();
            _userTicketRepo = new BaseRepository<UserTicket>();
            _assignedTicketRepo = new BaseRepository<AssignedTicket>();
            _notifRepo = new BaseRepository<Notification>();
            _catRepo = new BaseRepository<Category>();
            _articleRepo = new BaseRepository<Article>();

            // Only call RemindTicketNotif if the user is authenticated
            if (httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                string errorMsg, successMsg;
                RemindTicketNotif(out errorMsg, out successMsg);
            }
        }
        public BaseController()
        {
            _db = new TicketingSystemDBContext();
            _db1 = new AssisthubDBContext();
            _userRepo = new BaseRepository<User>();
            _vw_UserRoleView = new BaseRepository<VwUserRoleView>();
            _userRoleRepo = new BaseRepository<UserRole>();
            _vw_UsersAndAgentsView = new BaseRepository<VwUsersAndAgentsView>();
            _ticketRepo = new BaseRepository<Ticket>();
            _userTicketRepo = new BaseRepository<UserTicket>();
            _assignedTicketRepo = new BaseRepository<AssignedTicket>();
            _notifRepo = new BaseRepository<Notification>();
            _catRepo = new BaseRepository<Category>();

            // Only call RemindTicketNotif if the user is authenticated
            //if (HttpContext.User.Identity.IsAuthenticated)
            //{
            //    string errorMsg, successMsg;
            //    RemindTicketNotif(out errorMsg, out successMsg);
            //}
        }

        public ErrorCode RemindTicketNotif(out string errorMsg, out string successMsg)
        {
            errorMsg = successMsg = string.Empty;

            try
            {
                // Fetch the current UTC time, converted to the application's timezone
                // Fetch the current UTC time, converted to the application's timezone
                var currentTime = Utilities.TimeZoneConverter.ConvertTimeZone(DateTime.UtcNow);

                // Simulate advancing time by adding hours
                int hoursToAdvance = 1; // Adjust this value as needed for your tests
                currentTime = currentTime.AddHours(hoursToAdvance);

                var tickets = _db1.VwNotificationViews
                    .Where(m => m.AgentId.HasValue && m.DateAssigned.HasValue && m.ResolutionTime.HasValue && (m.StatusName.Equals("In Progress")))
                    .ToList(); // only get ticket that already has agent assigned
                
                if (tickets.Count == 0)
                {
                    return ErrorCode.Error;
                }

                // Sending reminders before the due date...
                foreach (var ticket in tickets)
                {
                    // Fetch the ticket's DateAssigned and keep it in UTC or local based on your needs
                    DateTime ticketAssignedTime = ticket.DateAssigned.Value; // Keep it in its original format

                    // Calculate the due date for the ticket (DateAssigned + ResolutionTime)
                    var dueDate = ticketAssignedTime.AddHours((double)ticket.ResolutionTime); // Use local time for due date calculations

                    int hoursBeforeTrigger = 7; // Fixed hours before due date (STATIC declared)
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
                            Content = $"Unresolved Ticket Reminder for Ticket ID: {ticket.TicketId} Date Assigned: {ticketAssignedTime} Hours To Be Resolve: {ticket.ResolutionTime}. Please resolve this ticket within {hoursBeforeTrigger} hours immediately!",
                        };

                        if (_notifRepo.Create(suppAgentNotif) == ErrorCode.Error)
                        {
                            return ErrorCode.Error;
                        }

                        // Notify also the user who owns the ticket...
                        var userNotif = new Notification()
                        {
                            ToUserId = ticket.UserId,  // Notify the user also 
                            UserTicketId = ticket.TicketId,
                            Content = $"We have noticed that your concerned with Ticket ID: {ticket.TicketId} has not been resolved. We already notified the assigned agent for this matter and promised to resolve this urgently. Thank you for your patience!",
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




        public enum CustomErrorCode {
            Success,
            InvalidRoleId,
        }


        public AlertMessageContent CreateNewUser(User user, int roleId, object? modelParam)
        {
            /* ROLE
             * 1 - user
             * 2 - support agent
             * 3 - administrator
             * 4 - superadmin
             */
            CustomUser customUser = new CustomUser();
            var roleList = _db.Roles.Where(m => m.RoleName == "user" || m.RoleName == "support agent").ToList();
            customUser.roleList = roleList;
            customUser.user = user;

            var alertMessageContent = new AlertMessageContent();

            if (roleId <= 0 && roleId > 4)
            {
                return new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Invalid role id."
                };
            }
            switch (_userRepo.Create(user))
            {
                case ErrorCode.Success:
                    var userRole = new UserRole()
                    {
                        UserId = user.UserId,
                        RoleId = roleId
                    };
                    if (_userRoleRepo.Create(userRole) == ErrorCode.Success)
                    {
                        customUser.user = user;
                        customUser.userRole = userRole;
                        return new AlertMessageContent()
                        {
                            Status = ErrorCode.Success,
                            Message = "New admin user is created successfully."
                        };
                    } else
                    {
                        _userRepo.Delete(user.UserId);
                        return new AlertMessageContent()
                        {
                            Status = ErrorCode.Error,
                            Message = "An error has occured. Please try again later."
                        };
                    }
                    
                case ErrorCode.Error:
                    return new AlertMessageContent()
                    {
                        Status = ErrorCode.Error,
                        Message = "An error has occured upon creating the account, please try again later."
                    };
                case ErrorCode.Duplicate:
                    return new AlertMessageContent()
                    {
                        Status = ErrorCode.Error,
                        Message = "Duplicate name or email has detected, please use another one."
                    };
                default:
                    return new AlertMessageContent()
                    {
                        Status = ErrorCode.Error,
                        Message = "An error has occured. Please try again later."
                    };
            }
        }
        public DateTime DateTimeToday()
        {
            TimeZoneInfo phTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            DateTime dateTimeToday = TimeZoneInfo.ConvertTime(DateTime.Now, phTimeZone);
            return dateTimeToday;
        }
    }
}
