using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class VwTotalTicketSummaryWithCategory
    {
        public string CategoryName { get; set; }
        public int? TotalCount { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
