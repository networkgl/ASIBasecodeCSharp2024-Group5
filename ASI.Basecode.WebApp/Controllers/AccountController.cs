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
using System.Net;
using System.Net.Mail;
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
            if (TempData["Success"] is not null)
            {
                ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Success,
                    Message = "Your password has been successfully updated."
                });
            }
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

            var allUser = _userRepo.GetAll();

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
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            ViewData["UserType"] = "User";

            var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

            var roleList = _db.Roles.Where(m => m.RoleName == "user" || m.RoleName == "support agent").ToList();
            customUser.roleList = roleList;
            if (customUser.user == null || customUser.userRole == null)
            {
                ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "An error has occured when updating user."
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
            var userRole = customUser.userRole;

            var oldPassword = _db.Users.Where(m => m.UserId == userId).Select(m => m.Password).FirstOrDefault();

            if (!string.IsNullOrEmpty(user.Password))
            {
                if (user.Password.Length < 8 ||
                    !user.Password.Any(char.IsUpper) ||  
                    !user.Password.Any(char.IsLower) || 
                    !user.Password.Any(char.IsDigit) || 
                    !(user.Password.Any(char.IsSymbol) || user.Password.Any(char.IsPunctuation)))
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

            //if (customUser.formFile != null)
            //{
            //    var imageFile = customUser.formFile;
            //    var root = Path.Combine(_webHostEnvironment.WebRootPath, "profilepics");

            //    if (!Directory.Exists(root))
            //    {
            //        Directory.CreateDirectory(root);
            //    }
            //    if (imageFile != null)
            //    {
            //        var uniqueFileName = GetUniqueFileName(root, imageFile.FileName);
            //        var filePath = Path.Combine(root, uniqueFileName);
            //        using (var stream = new FileStream(filePath, FileMode.Create))
            //        {
            //            await imageFile.CopyToAsync(stream);
            //        }
            //        var relativePath = Path.Combine("/profilepics", uniqueFileName).Replace("\\", "/");
            //        customUser.user.ProfilePicturePath = relativePath;
            //    }
            //}
            if (_userRepo.Update(user.UserId, user) == ErrorCode.Success)
            {
                ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Success,
                    Message = "Updated Profile Successfully!"
                });
                return View(customUser);
            }
            ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
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

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            var user = _db.Users.Where(m => m.Email == email).FirstOrDefault();

            if(user is not null)
            {
                try
                {
                    var sendersEmail = _appConfiguration["EmailSettings:SendersEmail"];
                    var sendersPassword = _appConfiguration["EmailSettings:SendersPassword"];
                    var noreplyEmail = "no-reply@ecofridge.com";
                    var subject = "Forgot Password";

                    Guid guid = Guid.NewGuid();
                    string emailVerificationCode = guid.ToString("N").Substring(0, 8);

                    while (true) 
                    {
                        if (_db.Users.Where(m => m.EmailVerificationCode == emailVerificationCode).FirstOrDefault() is not null)
                        {
                            emailVerificationCode = guid.ToString("N").Substring(0, 8);
                            continue;
                        } else
                        {
                            break;
                        }
                    }

                    var body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f4f4f4;'>
                        <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 10px; box-shadow: 0 2px 5px rgba(0,0,0,0.1);'>
                            <h2 style='color: #333;'>Password Reset Verification</h2>
                            <p>Hello,</p>
                            <p>We received a request to reset your password. To proceed, please use the verification code provided below:</p>
                            <p style='font-size: 18px; font-weight: bold; color: #307a59;'>{emailVerificationCode}</p>
                            <p>Once you have entered the code, you will be able to create a new password.</p>
                            <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                            <p>If you didn't request this, please ignore this email.</p>
                            <p>Thank you,</p>
                            <p><strong>Team CodeBlooded</strong></p>
                        </div>
                    </div>";

                    user.EmailVerificationCode = emailVerificationCode;

                    if (_userRepo.Update(user.UserId, user) == ErrorCode.Success)
                    {
                        using (MailMessage message = new MailMessage())
                        {
                            message.From = new MailAddress(noreplyEmail);
                            message.To.Add(user.Email);
                            message.Subject = subject;
                            message.Body = body;
                            message.IsBodyHtml = true;
                            message.ReplyToList.Add(new MailAddress(noreplyEmail));

                            using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                            {
                                smtp.Credentials = new NetworkCredential(sendersEmail, sendersPassword);
                                smtp.EnableSsl = true;
                                smtp.Send(message);
                            }
                        }
                        return RedirectToAction("ValidateVerificationCode");
                    }
                }
                catch (Exception)
                {
                    ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                    {
                        Status = ErrorCode.Error,
                        Message = "Invalid email."
                    });
                    return View();
                }
            }
            ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
            {
                Status = ErrorCode.Error,
                Message = "Invalid email."
            });
            return View();
        }

        public IActionResult ValidateVerificationCode()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ValidateVerificationCode(string verificationCode)
        {
            var validCode = _db.Users.Where(m => m.EmailVerificationCode == verificationCode).FirstOrDefault();

            if (validCode is not null)
            {
                TempData["UserId"] = validCode.UserId;
                return RedirectToAction("CreateNewPassword");
            }
            ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
            {
                Status = ErrorCode.Error,
                Message = "Invalid code, please try again."
            });
            return View();
        }

        public IActionResult CreateNewPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateNewPassword(string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Password does not match."
                });
                TempData["UserId"] = TempData["UserId"];
                return View();
            }

            if (!IsPasswordValid(newPassword))
            {
                ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Password must be at least 8 characters long, contain an uppercase letter, a lowercase letter, a number, and a special character."
                });
                TempData["UserId"] = TempData["UserId"];
                return View();
            }

            if (TempData["UserId"] is null)
            {
                TempData["UserId"] = TempData["UserId"];
                return BadRequest();
            }

            int userId = Convert.ToInt32(TempData["UserId"].ToString());

            var user = _db.Users.Where(m => m.UserId == userId).FirstOrDefault();
            if (user.Password == newPassword)
            {
                ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
                {
                    Status = ErrorCode.Error,
                    Message = "Please use a new password."
                });
                TempData["UserId"] = TempData["UserId"];
                return View();
            }

            user.Password = newPassword;
            if (_userRepo.Update(userId, user) == ErrorCode.Success)
            {
                TempData["Success"] = true;
                return RedirectToAction("LogIn");
            }

            ViewData["ResMsg"] = JsonConvert.SerializeObject(new AlertMessageContent()
            {
                Status = ErrorCode.Error,
                Message = "An error has occured upon updating your password, please try again later."
            });
            TempData["UserId"] = TempData["UserId"];
            return View();
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
    }
}
