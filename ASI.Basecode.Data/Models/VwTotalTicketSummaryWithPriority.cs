using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class VwTotalTicketSummaryWithPriority
    {
        public int? TotalCount { get; set; }
        public string PriorityName { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
