using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class VwCustomerSatisfactionRating
    {
        public int? AgentId { get; set; }
        public string AgentName { get; set; }
        public decimal? AvgFeedbackRating { get; set; }
        public DateTime? FeedbackAt { get; set; }
    }
}
