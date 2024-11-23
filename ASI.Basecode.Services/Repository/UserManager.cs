using ASI.Basecode.Services.Controllers;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Repository
{
    public class UserManager : BaseController
    {
        public UserManager()
        {
        }
        public int? GetLoggedInUserId(HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
        public string? GetUserNameById(int userId)
        {
            var retVal = _userRepo.Table.Where(m => m.UserId == userId).FirstOrDefault().Name == null ? null : _userRepo.Table.Where(m => m.UserId == userId).FirstOrDefault().Name;
            return retVal;
        }
    }
}
