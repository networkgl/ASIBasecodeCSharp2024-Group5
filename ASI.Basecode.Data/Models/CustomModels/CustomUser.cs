using ASI.Basecode.Data.Models;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models.CustomModels
{
    public class CustomUser
    {
        public User user { get; set; }
        public UserRole userRole { get; set; }
        public Role role { get; set; }
        public List<Role> roleList = new List<Role>();
    }
}
