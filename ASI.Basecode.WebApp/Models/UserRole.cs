using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class UserRole
    {
        public UserRole()
        {
            Donees = new HashSet<Donee>();
            Donors = new HashSet<Donor>();
        }

        public int UserRoleId { get; set; }
        public int? UserId { get; set; }
        public int? RoleId { get; set; }

        public virtual Role Role { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<Donee> Donees { get; set; }
        public virtual ICollection<Donor> Donors { get; set; }
    }
}
