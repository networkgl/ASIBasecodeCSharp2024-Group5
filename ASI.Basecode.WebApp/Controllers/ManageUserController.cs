using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models.CustomModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Policy ="AdminPolicy")]
    public class ManageUserController:BaseController
    {
        public ManageUserController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
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
                }
                else
                {
                    TempData["ResMsg"] = new AlertMessageContent()
                    {
                        Status = ErrorCode.Error,
                        Message = "An error has occured upon deleting the account."
                    };
                }
            }
            var users = _db.VwUsersAndAgentsViews.ToList();
            return View(users);
        }
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["UserType"] = "User";
            CustomUser customUser = new CustomUser();
            var roleList = _db.Roles.Where(m => m.RoleName == "user" || m.RoleName == "support agent").ToList();
            customUser.roleList = roleList;
            return View(customUser);
        }

        [HttpPost]
        public IActionResult Create(CustomUser customUser, string ConfirmPassword)
        {
            string[] AllowedDomains = { "gmail.com", "yahoo.com", "outlook.com" };
            var roleList = _db.Roles.Where(m => m.RoleName == "user" || m.RoleName == "support agent").ToList();
            customUser.roleList = roleList;
            customUser.user = customUser.user;

            if (string.IsNullOrEmpty(customUser.user.Name) || string.IsNullOrEmpty(customUser.user.Email) || string.IsNullOrEmpty(customUser.user.Password) || string.IsNullOrEmpty(ConfirmPassword))
            {
                TempData["ResMsg"] = new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Please fill out all required fields."
                };
                return View(customUser);
            }

            var domain = customUser.user.Email.Split('@').Last();
            if (!AllowedDomains.Contains(domain))
            {
                TempData["ResMsg"] = new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Invalid email address"
                };
                return View(customUser);
            }


            if (customUser == null)
            {
                TempData["ResMsg"] = new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "An error has occured upon creating the account."
                };
                return View(customUser);
            }

            if (ConfirmPassword != customUser.user.Password)
            {
                TempData["ResMsg"] = new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Password mismatch, please try again."
                };
                return View();
            }

            var result = CreateNewUser(customUser.user, Convert.ToInt32(customUser.userRole.RoleId), null);
            TempData["ResMsg"] = result;
            return View();
        }

        public IActionResult Edit(int id)
        {
            ViewData["UserType"] = "User";
            var user = _db.VwUsersAndAgentsViews.Where(m => m.UserId == id).FirstOrDefault();

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var role = _db.Roles.Where(m => m.RoleId == user.RoleId).FirstOrDefault();

            if (role == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var userModel = _db.Users.Where(m => m.UserId == user.UserId).FirstOrDefault();

            if (userModel == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var userRole = _db.UserRoles.Where(m => m.UserId == userModel.UserId).FirstOrDefault();

            if (userRole == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var roleList = _db.Roles.Where(m => m.RoleName == "user" || m.RoleName == "support agent").ToList();
            var customUserModel = new CustomUser()
            {
                user = userModel,
                userRole = userRole,
                role = role,
                roleList = roleList,
            };
            return View(customUserModel);
        }

        [HttpPost]
        public IActionResult Edit(CustomUser customUser)
        {
            ViewData["UserType"] = "User";
            var roleList = _db.Roles.Where(m => m.RoleName == "user" || m.RoleName == "support agent").ToList();
            customUser.roleList = roleList;
            if (customUser.user == null || customUser.userRole == null)
            {
                TempData["ResMsg"] = new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "An error has occured when updating user."
                };
                return View(customUser);
            }

            var user = customUser.user;
            var userRole = customUser.userRole;

            var role = _db.Roles.Where(m => m.RoleId == userRole.RoleId).FirstOrDefault();

            if (role == null)
            {
                TempData["ResMsg"] = new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "An error has occured when updating user."
                };
                return View(customUser);
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
                    return View(customUser);
                }
            }
            TempData["ResMsg"] = new AlertMessageContent()
            {
                Status = ErrorCode.Error,
                Message = "An error has occured when updating user."
            };
            return View(customUser);
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
