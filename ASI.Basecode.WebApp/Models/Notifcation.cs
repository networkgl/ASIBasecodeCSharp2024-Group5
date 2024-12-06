using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class Notifcation
    {
        public int NotificationId { get; set; }
        public int? UserId { get; set; }
        public int? FoodId { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }
        public bool? HasBeenViewed { get; set; }

        public virtual User CreatedByNavigation { get; set; }
        public virtual Food Food { get; set; }
        public virtual User UpdatedByNavigation { get; set; }
        public virtual User User { get; set; }
    }
}
