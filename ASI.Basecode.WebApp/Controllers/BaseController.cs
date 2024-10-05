using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.Data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    public class BaseController : Controller
    {
        protected ISession _session;
        public TicketingSystemDBContext _db;
        public BaseRepository<User> _userRepo;
        public BaseRepository<VwUserRoleView> _vw_UserRoleView;
        public BaseRepository<UserRole> _userRoleRepo;
        public BaseRepository<VwUsersAndAgentsView> _vw_UsersAndAgentsView;
        public BaseRepository<Ticket> _ticketRepo;
        public BaseRepository<UserTicket> _userTicketRepo;
        public BaseRepository<AssignedTicket> _assignedTicketRepo;
        public BaseRepository<Notification> _notifRepo;
        public BaseRepository<Category> _catRepo;

        public BaseController(IHttpContextAccessor httpContextAccessor)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _db = new TicketingSystemDBContext();
            _userRepo = new BaseRepository<User>();
            _vw_UserRoleView = new BaseRepository<VwUserRoleView>();
            _userRoleRepo = new BaseRepository<UserRole>();
            _vw_UsersAndAgentsView = new BaseRepository<VwUsersAndAgentsView>();
            _ticketRepo = new BaseRepository<Ticket>();
            _userTicketRepo = new BaseRepository<UserTicket>();
            _assignedTicketRepo = new BaseRepository<AssignedTicket>();
            _notifRepo = new BaseRepository<Notification>();
            _catRepo = new BaseRepository<Category>();
    }
        public BaseController()
        {
            _db = new TicketingSystemDBContext();
            _userRepo = new BaseRepository<User>();
            _vw_UserRoleView = new BaseRepository<VwUserRoleView>();
            _userRoleRepo = new BaseRepository<UserRole>();
            _vw_UsersAndAgentsView = new BaseRepository<VwUsersAndAgentsView>();
            _ticketRepo = new BaseRepository<Ticket>();
            _userTicketRepo = new BaseRepository<UserTicket>();
            _assignedTicketRepo = new BaseRepository<AssignedTicket>();
            _notifRepo = new BaseRepository<Notification>();
            _catRepo = new BaseRepository<Category>();
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
