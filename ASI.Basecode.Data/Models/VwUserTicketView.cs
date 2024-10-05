using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class VwUserTicketView
    {
        public string CategoryName { get; set; }
        public string StatusName { get; set; }
        public int TicketId { get; set; }
        public int? CategoryId { get; set; }
        public int? PriorityId { get; set; }
        public int? StatusId { get; set; }
        public string IssueDescription { get; set; }
        public string AttachmentPath { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? LastModified { get; set; }
        public int? ResolveAt { get; set; }
        public int UserTicketId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
