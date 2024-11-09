using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Models.CustomModels
{
    public class TicketSummaryModel
    {
        public List<VwTicketsByCategory> TicketsByCategory { get; set; }
        public List<VwTicketsByPriority> TicketsByPriority { get; set; }
        public List<VwTicketsByStatus> TicketsByStatus { get; set; }
        public List<VwTotalTicketSummaryWithCategory> TicketSummaryWithCategory { get; set; }
        public List<VwTotalTicketSummaryWithStatus> TicketSummaryWithStatus { get; set; }
        public List<VwTotalTicketSummaryWithPriority> TicketSummaryWithPriority { get; set; }
        public List<VwTicketAssignedToMeAgent> TicketsAssignedToMeAgent { get; set; }
    }
}
