using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class Donor
    {
        public int DonorId { get; set; }
        public int? UserRoleId { get; set; }

        public virtual UserRole UserRole { get; set; }
    }
}
