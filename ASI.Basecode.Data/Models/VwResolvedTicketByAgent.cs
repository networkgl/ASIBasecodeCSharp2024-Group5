using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class VwResolvedTicketByAgent
    {
        public int? AgentId { get; set; }
        public int? TotalResolvedCount { get; set; }
        public string AgentName { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
