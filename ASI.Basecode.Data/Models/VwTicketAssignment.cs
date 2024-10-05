using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class VwTicketAssignment
    {
        public int? AssignedTicketId { get; set; }
        public int TicketId { get; set; }
        public string IssueDescription { get; set; }
        public int? ReporterId { get; set; }
        public string Reporter { get; set; }
        public string ReporterEmail { get; set; }
        public string ReporterPassword { get; set; }
        public int? AssignerId { get; set; }
        public string AssignerEmail { get; set; }
        public string AssignerName { get; set; }
        public string AssignerPassword { get; set; }
        public int? AgentId { get; set; }
        public string AgentEmail { get; set; }
        public string AgentName { get; set; }
        public string AgentPassword { get; set; }
        public DateTime? DateAssigned { get; set; }
        public DateTime? ResolveLastModified { get; set; }
        public int? PriorityId { get; set; }
        public string PriorityName { get; set; }
        public int? StatusId { get; set; }
        public string StatusName { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public DateTime? TicketLastModified { get; set; }
        public DateTime? CreateAt { get; set; }
        public string AttachmentPath { get; set; }
        public int UserTicketId { get; set; }
    }
}
