using ASI.Basecode.Data.Models;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models.CustomModels
{
    public class CustomEditTicketAssignment
    {
        public AssignedTicket AssignedTicket { get; set; }
        public Ticket Ticket { get; set; }
        public List<VwUserRoleView> Agents { get; set; }
        public User? Agent { get; set; }
        public List<Category> Category = new List<Category>();
        public List<Priority> Priority = new List<Priority>();
        public List<Status> Status = new List<Status>();
        public UserTicket UserTicket { get; set; }

        public User? AssignedBy { get; set; }

        public List<VwTicketCountForAgent> ticketCountForAgent = new List<VwTicketCountForAgent>();
        public List<int> ticketCountAgentId = new List<int>();
    }
}
