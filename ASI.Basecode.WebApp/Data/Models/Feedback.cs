using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Data.Models
{
    public partial class Feedback
    {
        public int? FeedbackId { get; set; }
        public int? UserId { get; set; }
        public string FeedbackText { get; set; }
        public decimal? Rating { get; set; }
        public int? TicketId { get; set; }
    }
}
