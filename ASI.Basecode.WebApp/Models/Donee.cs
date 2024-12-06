using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class Donee
    {
        public int DoneeId { get; set; }
        public int? UserRoleId { get; set; }

        public virtual UserRole UserRole { get; set; }
    }
}
