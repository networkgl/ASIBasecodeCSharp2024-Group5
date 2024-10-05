using ASI.Basecode.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp
{
    public class RolesInDBAuthorizationHandler : AuthorizationHandler<RolesAuthorizationRequirement>
    {
        private readonly TicketingSystemDBContext _dbContext;

        public RolesInDBAuthorizationHandler(TicketingSystemDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                             RolesAuthorizationRequirement requirement)
        {
            if (context.User == null || !context.User.Identity.IsAuthenticated)
            {
                context.Fail();
                return;
            }

            var userName = context.User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                context.Fail();
                return;
            }

            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Name == userName);
            if (user == null)
            {
                context.Fail();
                return;
            }

            var allowedRole = requirement.AllowedRoles.FirstOrDefault();
            var roleId = await _dbContext.Roles
                                          .Where(m => m.RoleName == allowedRole)
                                          .Select(m => m.RoleId).FirstOrDefaultAsync();

            var userHasRole = _dbContext.UserRoles
                                              .Where(m => m.UserId == user.UserId && m.RoleId == roleId).FirstOrDefault();

            if (userHasRole != null)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }

}
