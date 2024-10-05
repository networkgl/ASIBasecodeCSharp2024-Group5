using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace ASI.Basecode.Data.Models.CustomModels
{
    public class CustomTicket
    {
        public Ticket ticket { get; set; }
        public UserTicket userTicket { get; set; }
        public List<Category> category = new List<Category>();
        public List<Priority> priority = new List<Priority>();
        public List<Status> status = new List<Status>();
        public User user { get; set; }
        public IFormFile formFile { get; set; }
        public string? RemovedOriginalImg { get; set; }
    }
}
