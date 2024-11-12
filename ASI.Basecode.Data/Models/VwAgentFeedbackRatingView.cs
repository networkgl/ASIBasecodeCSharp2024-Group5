using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class VwAgentFeedbackRatingView
    {
        public int AgentId { get; set; }
        public string AgentName { get; set; }
        public string ProfilePicture { get; set; }
        public decimal? AverageRating { get; set; }
        public int? FeedbackCount { get; set; }
    }
}
