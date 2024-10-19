using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class VwNotificationView
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int? AssignedTicketId { get; set; }
        public int TicketId { get; set; }
        public int UserTicketId { get; set; }
        public string CategoryName { get; set; }
        public string IssueDescription { get; set; }
        public string PriorityName { get; set; }
        public int? ResolutionTime { get; set; }
        public string StatusName { get; set; }
        public int? AgentId { get; set; }
        public string AgentName { get; set; }
        public DateTime? DateAssigned { get; set; }
        public byte? HasReminded { get; set; }
    }
}
