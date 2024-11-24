using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class VwTicketsByPriority
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string PriorityName { get; set; }
        public int? TicketByPriority { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
