using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class Notification
    {
        public int NotificationId { get; set; }
        public int? FromUserId { get; set; }
        public int? UserTicketId { get; set; }
        public string Content { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }
        public int? ToUserId { get; set; }

        public virtual User FromUser { get; set; }
        public virtual UserTicket UserTicket { get; set; }
    }
}
