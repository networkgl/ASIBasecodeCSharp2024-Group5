using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class UserTicket
    {
        public UserTicket()
        {
            AssignedTickets = new HashSet<AssignedTicket>();
            Notifications = new HashSet<Notification>();
        }

        public int UserTicketId { get; set; }
        public int? UserId { get; set; }
        public int? TicketId { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<AssignedTicket> AssignedTickets { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
