using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class VwTicketsByPriority
    {
        public int UserId { get; set; }
        public string PriorityName { get; set; }
        public int? TotalCount { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
