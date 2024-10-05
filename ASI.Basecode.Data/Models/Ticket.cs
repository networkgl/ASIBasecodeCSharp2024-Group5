using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class Ticket
    {
        public Ticket()
        {
            UserTickets = new HashSet<UserTicket>();
        }

        public int TicketId { get; set; }
        public int? CategoryId { get; set; }
        public int? PriorityId { get; set; }
        public int? StatusId { get; set; }
        public string IssueDescription { get; set; }
        public string AttachmentPath { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? LastModified { get; set; }
        public int? ResolveAt { get; set; }

        public virtual Category Category { get; set; }
        public virtual Priority Priority { get; set; }
        public virtual Status Status { get; set; }
        public virtual ICollection<UserTicket> UserTickets { get; set; }
    }
}
