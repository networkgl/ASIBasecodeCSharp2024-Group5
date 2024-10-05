using ASI.Basecode.WebApp.Controllers;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ASI.Basecode.WebApp.Repository
{
    public class UserManager : BaseController
    {
        public UserManager()
        {
        }
        public string? GetUserNameById(int userId)
        {
            var retVal = _userRepo.Table.Where(m => m.UserId == userId).FirstOrDefault().Name == null ? null : _userRepo.Table.Where(m => m.UserId == userId).FirstOrDefault().Name;
            return retVal;
        }
    }
}
