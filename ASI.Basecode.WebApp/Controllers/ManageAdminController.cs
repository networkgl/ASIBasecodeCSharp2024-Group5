using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Policy = ("SuperAdminPolicy"))]
    public class ManageAdminController : BaseController
    {
        public ManageAdminController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
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
                        Message = "An admin account has deleted successfully!"
                    };
                } else
                {
                    TempData["ResMsg"] = new AlertMessageContent()
                    {
                        Status = ErrorCode.Error,
                        Message = "An error has occured upon deleting the account."
                    };
                }
            }
            var adminUsers = _db.VwAdminUsersViews.ToList();
            return View(adminUsers);
        }
        public IActionResult Edit(int id)
        {
            ViewData["UserType"] = "Admin";
            var adminUser = _db.VwAdminUsersViews.Where(m => m.UserId == id).FirstOrDefault();

            if (adminUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var role = _db.Roles.Where(m => m.RoleId == adminUser.RoleId).FirstOrDefault();

            if (role == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = _db.Users.Where(m => m.UserId == adminUser.UserId).FirstOrDefault();

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var userRole = _db.UserRoles.Where(m => m.UserId == user.UserId).FirstOrDefault();

            if (userRole == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var roleList = _db.Roles.ToList();
            var adminUsersModel = new CustomUser()
            {
                user = user,
                userRole = userRole,
                role = role,
                roleList = roleList,
            };
            return View(adminUsersModel);
        }

        [HttpPost]
        public IActionResult Edit(CustomUser adminUser)
        {
			ViewData["UserType"] = "Admin";
            var roleList = _db.Roles.ToList();
            adminUser.roleList = roleList;
            if (adminUser.user == null || adminUser.userRole == null)
            {
                TempData["ResMsg"] = new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "An error has occured when updating user."
                };
                return View(adminUser);
            }

            if (string.IsNullOrEmpty(adminUser.user.Password))
            {
                TempData["ResMsg"] = new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Please enter the original or new password."
                };
                return View(adminUser);
            }

			var user = adminUser.user;
            var userRole = adminUser.userRole;

            var role = _db.Roles.Where(m => m.RoleId == userRole.RoleId).FirstOrDefault();

            if (role == null)
            {
                TempData["ResMsg"] = new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "An error has occured when updating user."
                };
                return View(adminUser);
            }

            if (_userRepo.Update(user.UserId, user) == ErrorCode.Success)
            {
                if (_userRoleRepo.Update(userRole.UserRoleId, userRole) == ErrorCode.Success)
                {
                    TempData["ResMsg"] = new AlertMessageContent()
                    {
                        Status = ErrorCode.Success,
                        Message = "Updated Successfully!"
                    };
                    return View(adminUser);
                }
            }
            TempData["ResMsg"] = new AlertMessageContent()
            {
                Status = ErrorCode.Error,
                Message = "An error has occured when updating user."
            };
            return View(adminUser);
        }


        [HttpGet]
        public IActionResult Create() 
        {
            ViewData["UserType"] = "Admin";
            return View();
        }

        [HttpPost]
        public IActionResult Create(User user, string ConfirmPassword)
        {
            string[] AllowedDomains = { "gmail.com", "yahoo.com", "outlook.com" };

            if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(ConfirmPassword))
            {
                TempData["ResMsg"] = new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Please fill out all required fields."
                };
                return View();
            }

            var domain = user.Email.Split('@').Last();
            if (!AllowedDomains.Contains(domain))
            {
                TempData["ResMsg"] = new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Invalid email address"
                };
                return View();
            }


            if (user == null)
            {
                TempData["ResMsg"] = new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "An error has occured upon creating the account."
                };
                return View();
            }

            if (ConfirmPassword != user.Password)
            {
                TempData["ResMsg"] = new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Password mismatch, please try again."
                };
                return View();
            }
            var result = CreateNewUser(user, 3, null);
            TempData["ResMsg"] = result;
            return View();
        }

		public IActionResult Details(int id)
        {
            var adminUsers = _db.VwAdminUsersViews.ToList();
            return View(adminUsers);
        }
        public IActionResult Delete(int id)
        {
            TempData["temp"] = "delete";
            if (_userRepo.Delete(id) == ErrorCode.Success)
            {
                TempData["status"] = 0;
                return Ok();
            }

            TempData["status"] = 1;
            return BadRequest();
        }
    }
}
