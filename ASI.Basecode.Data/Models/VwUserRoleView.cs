﻿using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class VwUserRoleView
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ProfilePicturePath { get; set; }
        public string EmailVerificationCode { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int UserRoleId { get; set; }
    }
}
