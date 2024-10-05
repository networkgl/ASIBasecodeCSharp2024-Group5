using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class User
    {
        public User()
        {
            Articles = new HashSet<Article>();
            AssignedTicketAgents = new HashSet<AssignedTicket>();
            AssignedTicketAssigners = new HashSet<AssignedTicket>();
            Feedbacks = new HashSet<Feedback>();
            Notifications = new HashSet<Notification>();
            UserRoles = new HashSet<UserRole>();
            UserTickets = new HashSet<UserTicket>();
        }

        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public virtual ICollection<Article> Articles { get; set; }
        public virtual ICollection<AssignedTicket> AssignedTicketAgents { get; set; }
        public virtual ICollection<AssignedTicket> AssignedTicketAssigners { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<UserTicket> UserTickets { get; set; }
    }
}
