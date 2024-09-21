using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Sample Crud Controller
    /// </summary>
    public class UserController : ControllerBase<UserController>
    {
        private readonly ISampleCrudService _userService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="configuration"></param>
        /// <param name="localizer"></param>
        /// <param name="mapper"></param>
        public UserController(ISampleCrudService sampleCrudService,
            IHttpContextAccessor httpContextAccessor,
                              ILoggerFactory loggerFactory,
                              IConfiguration configuration,
                              IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _userService = sampleCrudService;
        }

        /// <summary>
        /// Returns Sample Crud View.
        /// </summary>
        /// <returns> Sample Crud View </returns>
        public IActionResult Index()
        {
            try {
                _logger.LogInformation("=======Sample Crud : Retrieve All Start=======");
                var data = _userService.RetrieveAll();
                var role = UserRole;
                ViewData["Role"] = role;
                var model = new UserListViewModel
                {
                    dataList = data
                };
                _logger.LogInformation("=======Sample Crud : Retrieve All End=======");
                return View(model);
            } catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View(null);
            }
        }


        [HttpPost]
        public IActionResult SearchUsers(UserListViewModel model)
        {
            try
            {
                _logger.LogInformation("=======Sample Crud : Retrieve All Start=======");
                var data = _userService.RetrieveAll(string.IsNullOrEmpty(model.IdFilter) ? null : int.Parse(model.IdFilter), model.FirstNameFilter);
                var role = UserRole;
                ViewData["Role"] = role;
                var viewModel = new UserListViewModel
                {
                    dataList = data
                };
                _logger.LogInformation("=======Sample Crud : Retrieve All End=======");
                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View(null);
            }
        }

        #region GET Methods
        /// <summary>
        /// Return Create View
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Details
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Details(int Id)
        {
            var data = _userService.RetrieveAll().Where(x => x.Id.Equals(Id)).FirstOrDefault();
            return View(data);
        }

        [HttpGet]
        public IActionResult Edit(int Id)
        {
            var data = _userService.RetrieveUser(Id);
            return View(data);
        }

        [HttpGet]
        public IActionResult Delete(int Id)
        {
            var data = _userService.RetrieveAll().Where(x => x.Id.Equals(Id)).FirstOrDefault();
            return View(data);
        }
        #endregion

        #region POST Methods
        [HttpPost]
        public IActionResult PostCreate(UserViewModel model)
        {
            _logger.LogInformation("=======Sample Crud : PostCreate Start=======");
            try {
                bool isExist = _userService.RetrieveAll().Any(data => data.UserCode == model.UserCode);
                if (isExist)
                {
                    TempData["DuplicateErr"] = "Duplicate Data";
                    _logger.LogError($"Duplicate Name: {model.UserCode}");
                    return RedirectToAction("Create", model);
                }
                _userService.Add(model);
            }catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            
            TempData["CreateMessage"] = "Added Successfully";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult PostUpdate(UserViewModel model)
        {
            _userService.Update(model);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult PostDelete(int Id)
        {
            _userService.Delete(Id);
            return RedirectToAction("Index");
        }
        #endregion
    }
}
