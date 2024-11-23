using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.CustomModels;
using ASI.Basecode.Services.Controllers;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.WebApp.Authentication;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    public class AccountController : BaseController
    {
        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly TokenValidationParametersFactory _tokenValidationParametersFactory;
        private readonly TokenProviderOptionsFactory _tokenProviderOptionsFactory;
        private readonly IConfiguration _appConfiguration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        //private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="localizer">The localizer.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        public AccountController(
                            IWebHostEnvironment webHostEnvironment,
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            //IUserService userService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base (httpContextAccessor)
        {
            this._webHostEnvironment = webHostEnvironment;
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            //this._userService = userService;
        }

        /// <summary>
        /// Login Method
        /// </summary>
        /// <returns>Created response view</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            TempData["returnUrl"] = System.Net.WebUtility.UrlDecode(HttpContext.Request.Query["ReturnUrl"]);
            this._sessionManager.Clear();
            this._session.SetString("SessionId", System.Guid.NewGuid().ToString());

            if (User.Identity.IsAuthenticated)
            {
                switch (User.FindFirst("UserRole")?.Value)
                {
                    case "superadmin":
                        return RedirectToAction("SuperAdminDashboard", "Dashboard");
                    case "administrator":
                        return RedirectToAction("AdminDashboard", "Dashboard");
                    case "user":
                        return RedirectToAction("UserDashboard", "Dashboard");
                    case "support agent":
                        return RedirectToAction("AgentDashboard", "Dashboard");
                    default:
                        return View();
                }
            }
            return View();
        }

        /// <summary>
        /// Authenticate user and signs the user in when successful.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns> Created response view </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(User u, string rememberMe)
        {
            this._session.SetString("HasSession", "Exist");

            var user = _db.VwUserRoleViews.Where(m => m.Email == u.Email && m.Password == u.Password).FirstOrDefault();

            if (user == null)
            {
                TempData["ErrorMsg"] = "Invalid credentials, please try again";
                return View();
            }

            // 認証OK
            var isPersistent = !string.IsNullOrEmpty(rememberMe) && rememberMe.ToLower() == "on" ? true : false;
            await _signInManager.SignInAsync(user, isPersistent);
            this._session.SetString("UserName", user.Name);

            switch (user.RoleName)
            {
                case "superadmin":
                    return RedirectToAction("SuperAdminDashboard", "Dashboard");
                case "administrator":
                    return RedirectToAction("AdminDashboard", "Dashboard");
                case "user":
                    return RedirectToAction("UserDashboard", "Dashboard");
                case "support agent":
                    return RedirectToAction("AgentDashboard", "Dashboard");
                default:
                    // 認証NG
                    TempData["ErrorMsg"] = "An error has occured, please try again";
                    return View();
            }
        }

        //[HttpGet]
        //[AllowAnonymous]
        //public IActionResult Register()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //public IActionResult Register(User model)
        //{
        //    try
        //    {
        //        //_userService.AddUser(model);
        //        return RedirectToAction("Login", "Account");
        //    }
        //    catch(InvalidDataException ex)
        //    {
        //        TempData["ErrorMessage"] = ex.Message;
        //    }
        //    catch(Exception)
        //    {
        //        TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
        //    }
        //    return View();
        //}

        /// <summary>
        /// Sign Out current account and return login view.
        /// </summary>
        /// <returns>Created response view</returns>
        [AllowAnonymous]
        public async Task<IActionResult> SignOutUser()
        {
            await this._signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }


        public IActionResult EditProfile()
        {
            if(!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            ViewData["UserType"] = "User";
            var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);
            var user = _db.VwUserRoleViews.Where(m => m.UserId == userId).FirstOrDefault();

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
        public async Task<IActionResult> EditProfile(CustomUser customUser, string confirmPassword)
        {
            ViewData["UserType"] = "User";
            var roleList = _db.Roles.Where(m => m.RoleName == "user" || m.RoleName == "support agent").ToList();
            customUser.roleList = roleList;
            if (customUser.user == null || customUser.userRole == null)
            {
                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "An error has occured when updating user."
                });
                return View(customUser);
            }

            var user = customUser.user;
            var userRole = customUser.userRole;

            

            var oldPassword = _db.Users.Where(m => m.UserId == user.UserId).Select(m => m.Password).FirstOrDefault();

            if (oldPassword != confirmPassword)
            {
                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Old password is incorrect."
                });
                return View(customUser);
            }

            if (string.IsNullOrEmpty(customUser.user.Password))
            {
                customUser.user.Password = oldPassword;
            }

            if (customUser.formFile != null)
            {
                var imageFile = customUser.formFile;
                var root = Path.Combine(_webHostEnvironment.WebRootPath, "profilepics");

                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                }
                if (imageFile != null)
                {
                    var uniqueFileName = GetUniqueFileName(root, imageFile.FileName);
                    var filePath = Path.Combine(root, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    var relativePath = Path.Combine("/profilepics", uniqueFileName).Replace("\\", "/");
                    customUser.user.ProfilePicturePath = relativePath;
                }
            }
            if (_userRepo.Update(user.UserId, user) == ErrorCode.Success)
            {
                TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Success,
                    Message = "Updated Profile Successfully!"
                });
                return View(customUser);
            }
            TempData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
            {
                Status = ErrorCode.Error,
                Message = "An error has occured when updating user."
            });
            return View(customUser);
        }
        public string GetUniqueFileName(string parentPath, string fileName)
        {
            string uniqueName = fileName;
            int count = 1;

            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, parentPath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, uniqueName);

            while (System.IO.File.Exists(filePath))
            {
                string extension = Path.GetExtension(fileName);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                uniqueName = $"{fileNameWithoutExtension}_{count}{extension}";
                filePath = Path.Combine(folderPath, uniqueName);
                count++;
            }

            return uniqueName;
        }
    }
}
