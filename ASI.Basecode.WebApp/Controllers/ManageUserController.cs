using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.Services.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Policy = "SuperAdmin_Admin_AgentPolicy")]
    public class ManageUserController:BaseController
    {
        public ManageUserController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }
        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }
            HandleTempDataMessages();

            var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);
            var users = new List<VwUserRoleView>();

            if (User.FindFirst("UserRole")?.Value == "support agent")
            {
                users = _db.VwUserRoleViews.Where(m => m.RoleId == 1 || m.RoleId == 2).OrderByDescending(m => m.UserId).ToList();
            }
            else if (User.FindFirst("UserRole")?.Value == "superadmin") 
            {
                users = _db.VwUserRoleViews.OrderByDescending(m => m.UserId).ToList();
            }
            else
            {
                users = _db.VwUserRoleViews.OrderByDescending(m => m.UserId).ToList();
            }
            ViewData["TableId"] = "manageUsersTable";
            return View(users.OrderByDescending(m => m.UserId));
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["UserType"] = "User";
            CustomUser customUser = new CustomUser();

            var expertiseList = _db.Expertises.ToList();
            customUser.roleList = PopulateRoles();
            customUser.expertiseList = expertiseList;
            return View(customUser);
        }

        [HttpPost]
        public IActionResult Create(CustomUser customUser)
        {
            string[] AllowedDomains = { "gmail.com", "yahoo.com", "outlook.com" };

            customUser.roleList = PopulateRoles();

            if (string.IsNullOrWhiteSpace(customUser.user.Name) ||
                string.IsNullOrWhiteSpace(customUser.user.Email) ||
                string.IsNullOrWhiteSpace(customUser.user.Password))
            {
                SetTempDataMessage(ErrorCode.Error, "Please fill out all required fields.");
                ViewData["ResMsg"] = TempData["ResMsg"];
                return View(customUser);
            }

            var domain = customUser.user.Email.Split('@').LastOrDefault();
            if (domain == null || !AllowedDomains.Contains(domain))
            {
                SetTempDataMessage(ErrorCode.Error, "Invalid email address.");
                ViewData["ResMsg"] = TempData["ResMsg"];
                return View(customUser);
            }

            if (!IsPasswordValid(customUser.user.Password))
            {
                SetTempDataMessage(ErrorCode.Error, "Password must be at least 8 characters long, contain an uppercase letter, a lowercase letter, a number, and a special character.");
                ViewData["ResMsg"] = TempData["ResMsg"];
                return View(customUser);
            }

            //if (customUser.user.Password != ConfirmPassword)
            //{
            //    SetTempDataMessage(ErrorCode.Error, "Password mismatch, please try again.");
            //    return View(customUser);
            //}

            var result = CreateNewUser(customUser.user, Convert.ToInt32(customUser.userRole.RoleId), customUser.Expertise, customUser.OtherExpertise);

            if (result.Status == ErrorCode.Error)
            {
                SetTempDataMessage(result.Status, result.Message);
                ViewData["ResMsg"] = TempData["ResMsg"];
                return View(customUser);
            }

            SetTempDataMessage(ErrorCode.Success, "User Created Successfully!");
            return RedirectToAction("Index");
        }

        private void SetTempDataMessage(ErrorCode status, string message)
        {
            TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent
            {
                Status = status,
                Message = message
            });
        }

        public IActionResult Edit(int id)
        {
            ViewData["UserType"] = "User";
            var user = _db.VwUsersAndAgentsViews.Where(m => m.UserId == id).FirstOrDefault();

            var roleList = PopulateRoles();
            var expertiseList = _db.Expertises.ToList();
            var userAgent = _db.UserAgents.Where(m => m.AgentId == id).FirstOrDefault();

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

            var customUserModel = new CustomUser();
            if (userAgent != null)
            {
                customUserModel = new CustomUser()
                {
                    user = userModel,
                    userRole = userRole,
                    role = role,
                    roleList = roleList,
                    expertiseList = expertiseList,
                    Expertise = userAgent.Expertise,
                    OtherExpertise = expertiseList.Select(m => m.ExpertiseName).Contains(userAgent.Expertise) ? null : userAgent.Expertise
                };
            }
            else
            {
                customUserModel = new CustomUser()
                {
                    user = userModel,
                    userRole = userRole,
                    role = role,
                    roleList = roleList,
                    expertiseList = expertiseList,
                    Expertise = "",
                    OtherExpertise = null
                };
            }
            return View(customUserModel);
        }


        [HttpPost]
        public IActionResult Edit(CustomUser customUser, string confirmPassword)
        {
            TempData["temp"] = "update";
            ViewData["UserType"] = "User";

            customUser.roleList = PopulateRoles();
            customUser.expertiseList = _db.Expertises.ToList();

            if (customUser?.user == null || customUser.userRole == null)
            {
                ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent
                {
                    Status = ErrorCode.Error,
                    Message = "An error occurred while updating the user. Please try again."
                });
                return View(customUser);
            }

            if ((!string.IsNullOrEmpty(customUser.user.Password)) && customUser.user.Password != confirmPassword)
            {
                ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent
                {
                    Status = ErrorCode.Error,
                    Message = "Password mismatched."
                });
                return View(customUser);
            }

            var user = customUser.user;
            
            var oldPassword = _db.Users
                .Where(m => m.UserId == user.UserId)
                .Select(m => m.Password)
                .FirstOrDefault();


            if (!string.IsNullOrEmpty(user.Password))
            {
                if (user.Password.Length < 8 ||
                    !user.Password.Any(char.IsUpper) ||
                    !user.Password.Any(char.IsLower) ||
                    !user.Password.Any(char.IsDigit) ||
                    !(customUser.user.Password.Any(char.IsSymbol) || user.Password.Any(char.IsPunctuation)))
                {
                    ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent
                    {
                        Status = ErrorCode.Error,
                        Message = "Password must be at least 8 characters long and include uppercase and lowercase letters, numbers, and symbols."
                    });
                    return View(customUser);
                }
            }
            else
            {
                user.Password = oldPassword; // use old password if no new password provided
            }

            if (_userRepo.Update(user.UserId, user) == ErrorCode.Success)
            {
                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent
                {
                    Status = ErrorCode.Success,
                    Message = "User updated successfully!"
                });
                return RedirectToAction("Index");
            }

            ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent
            {
                Status = ErrorCode.Error,
                Message = "An error occurred while updating the user. Please try again later."
            });

            return View(customUser);
        }

        public IActionResult Details(int id)
        {
            var adminUsers = _db.VwAdminUsersViews.ToList();
            return View(adminUsers);
        }

        public IActionResult Delete(int id)
        {
            var result = _userRepo.Delete(id);
            if (result == ErrorCode.Success)
            {
                SetTempDataMessage(ErrorCode.Success, "User is deleted successfully!");
                return Ok();
            } else if (result == ErrorCode.ForeignKeyConstraintError)
            {
                SetTempDataMessage(ErrorCode.Error, "Cannot delete an agent who has currently assigned tickets, this agent must resolve or reassign their tickets before they can be deleted to ensure that ongoing issues are properly managed.");
            } else
            {
                SetTempDataMessage(ErrorCode.Error, "An error has occured upon deleting the user, try again later.");
            }

            return BadRequest();
        }

        private bool IsPasswordValid(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        private void HandleTempDataMessages()
        {
            if (TempData["ResMsg"] is not null)
            {
                var resMsg = JsonConvert.DeserializeObject<AlertMessageContent>(TempData["ResMsg"].ToString());
                if (resMsg is not null && User.Identity.IsAuthenticated)
                {
                    ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent
                    {
                        Status = resMsg.Status,
                        Message = resMsg.Message
                    });
                }
            }
        }

        private List<Role> PopulateRoles()
        {
            var roleList = new List<Role>();

            switch (User.FindFirst("UserRole").Value.ToLower())
            {
                case "adminstrator":
                    roleList = _db.Roles.Where(m => m.RoleName == "user" || m.RoleName == "support agent").ToList();
                    break;
                case "superadmin":
                default:
                    roleList = _db.Roles.ToList();
                    break;
            }

            roleList.ForEach(role => role.RoleName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.RoleName.ToLower()));

            return roleList;
        }
    }
}
