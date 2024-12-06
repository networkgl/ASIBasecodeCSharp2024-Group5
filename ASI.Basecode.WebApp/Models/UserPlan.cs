using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class UserPlan
    {
        public int UserPlanId { get; set; }
        public int? UserId { get; set; }
        public int? StoragePlanId { get; set; }
        public DateTime SubscriptionDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        public virtual StoragePlan StoragePlan { get; set; }
        public virtual User User { get; set; }
    }
}
