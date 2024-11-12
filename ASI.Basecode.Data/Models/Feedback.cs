using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class Feedback
    {
        public int FeedbackId { get; set; }
        public int? UserId { get; set; }
<<<<<<< Updated upstream
        public int? AssignedTicketId { get; set; }
        public int? UserTicketId { get; set; }
=======
        public int? UserTicketId { get; set; }
        public string TicketCategory { get; set; }
>>>>>>> Stashed changes
        public string FeedbackText { get; set; }
        public decimal? FeedbackRating { get; set; }
        public int? AgentId { get; set; }
        public DateTime? CreatedAt { get; set; }
<<<<<<< Updated upstream
=======
        public int? AssignedTicketId { get; set; }
        public virtual User User { get; set; }
        public virtual AssignedTicket AssignedTicket { get; set; }
>>>>>>> Stashed changes
    }
}
