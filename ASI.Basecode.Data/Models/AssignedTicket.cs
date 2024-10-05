using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class AssignedTicket
    {
        public int AssignedTicketId { get; set; }
        public int? UserTicketId { get; set; }
        public int? AssignerId { get; set; }
        public int? AgentId { get; set; }
        public DateTime? DateAssigned { get; set; }
        public DateTime? LastModified { get; set; }

        public virtual User Agent { get; set; }
        public virtual User Assigner { get; set; }
        public virtual UserTicket UserTicket { get; set; }
    }
}
