using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models.CustomModels
{
    public class CustomUser
    {
        public User user { get; set; }
        public UserRole userRole { get; set; }
        public Role role { get; set; }
        public List<Role> roleList = new List<Role>();
        public IFormFile formFile { get; set; }
        public List<Expertise> expertiseList = new List<Expertise>();
        public string Expertise { get; set; }
        public string OtherExpertise { get; set; }
    }
}
