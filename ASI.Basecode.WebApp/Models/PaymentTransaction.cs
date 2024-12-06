using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class PaymentTransaction
    {
        public int PaymentTransactionId { get; set; }
        public int? UserId { get; set; }
        public int? StoragePlanId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal? Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionStatus { get; set; }
        public string TransactionReferenceId { get; set; }

        public virtual StoragePlan StoragePlan { get; set; }
        public virtual User User { get; set; }
    }
}
