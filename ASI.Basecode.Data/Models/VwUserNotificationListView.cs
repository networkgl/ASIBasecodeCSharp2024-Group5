using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class VwUserNotificationListView
    {
        public int NotificationId { get; set; }
        public int? FromUserId { get; set; }
        public int? ToUserId { get; set; }
        public string Content { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }
        public byte? IsRead { get; set; }
        public int? ArticleId { get; set; }
        public string Approved { get; set; }
        public int? UserId { get; set; }
        public int? TicketId { get; set; }
        public int? UserTicketId { get; set; }
        public int? AssignedTicketId { get; set; }
        public int? AssignerId { get; set; }
        public int? AgentId { get; set; }
        public DateTime? DateAssigned { get; set; }
        public byte? HasReminded { get; set; }
        public DateTime? DateResolved { get; set; }
    }
}
