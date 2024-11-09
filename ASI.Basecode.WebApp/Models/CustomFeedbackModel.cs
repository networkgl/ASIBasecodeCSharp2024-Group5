using ASI.Basecode.Data.Models;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public class CustomFeedbackModel
    {
        public Feedback Feedback { get; set; }
        public List<Ticket> Ticket { get; set; }
        public int TicketId { get; set; }
        public string TicketDescription { get; set; }
        public List<Feedback> FeedbackList { get; set; }
    }
}
