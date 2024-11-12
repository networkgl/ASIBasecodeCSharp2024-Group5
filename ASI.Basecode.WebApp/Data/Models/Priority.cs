using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Data.Models
{
    public partial class Priority
    {
        public Priority()
        {
            Tickets = new HashSet<Ticket>();
        }

        public int PriorityId { get; set; }
        public string PriorityName { get; set; }
        public int? ResolutionTime { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
