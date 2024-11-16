using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Policy = "AdminAndAgentPolicy")]
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
            var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

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
                            var thisUsersList = _db.VwUsersAndAgentsViews.OrderByDescending(m => m.UserId).ToList();
                            return View(thisUsersList);
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
                            Message = "A user has deleted successfully!"
                        });
                    }
                    else
                    {
                        TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                        {
                            Status = ErrorCode.Error,
                            Message = "An error has occured upon deleting the user."
                        });
                    }
                }

                if ((string)TempData["temp"] == "update")
                {
                    if (TempData["status"] as int? == 0)
                    {
                        TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                        {
                            Status = ErrorCode.Success,
                            Message = "User created successfully!"
                        });
                    }
                    else
                    {
                        TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                        {
                            Status = ErrorCode.Error,
                            Message = "An error has occured when updating this user, pls try again."
                        });
                    }
                }
            }

            var users = new List<VwUsersAndAgentsView>();
            if (User.FindFirst("UserRole")?.Value == "support agent")
            {
                users = _db.VwUsersAndAgentsViews.Where(m => m.RoleId == 1 || m.RoleId == 2 ).OrderByDescending(m => m.UserId).ToList();
            } else
            {
                users = _db.VwUsersAndAgentsViews.OrderByDescending(m => m.UserId).ToList();
            }
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
                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Please fill out all required fields."
                });
                return View(customUser);
            }

            var domain = customUser.user.Email.Split('@').Last();
            if (!AllowedDomains.Contains(domain))
            {
                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Invalid email address"
                });
                return View(customUser);
            }


            if (customUser == null)
            {
                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "An error has occured upon creating the account."
                });
                return View(customUser);
            }

            if (ConfirmPassword != customUser.user.Password)
            {
                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Password mismatch, please try again."
                });
                return View(customUser);
            }

            var result = CreateNewUser(customUser.user, Convert.ToInt32(customUser.userRole.RoleId), null);
            TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
            {
                Status = ErrorCode.Success,
                Message = "User Created Successfully!"
            });
            return RedirectToAction("Index");
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
