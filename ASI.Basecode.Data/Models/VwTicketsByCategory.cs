using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class VwTicketsByCategory
    {
        public int UserId { get; set; }
        public string CategoryName { get; set; }
        public int? TicketsByCategory { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
