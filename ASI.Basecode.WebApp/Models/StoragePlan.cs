using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class StoragePlan
    {
        public StoragePlan()
        {
            PaymentTransactions = new HashSet<PaymentTransaction>();
            UserPlans = new HashSet<UserPlan>();
        }

        public int StoragePlanId { get; set; }
        public string StoragePlanName { get; set; }
        public int StorageSize { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; }
        public virtual ICollection<UserPlan> UserPlans { get; set; }
    }
}
