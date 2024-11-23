using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class VwTotalTicketSummaryWithStatus
    {
        public int? TotalCount { get; set; }
        public string StatusName { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
