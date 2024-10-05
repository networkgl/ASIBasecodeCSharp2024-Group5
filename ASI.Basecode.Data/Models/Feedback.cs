using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class Feedback
    {
        public int FeedbackId { get; set; }
        public int? UserId { get; set; }
        public string FeedbackText { get; set; }

        public virtual User User { get; set; }
    }
}
