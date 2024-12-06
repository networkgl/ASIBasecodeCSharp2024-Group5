using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class DonationTransactionMaster
    {
        public DonationTransactionMaster()
        {
            DonationTransactionDetails = new HashSet<DonationTransactionDetail>();
        }

        public int DonationTransactionMasterId { get; set; }
        public int? DonorId { get; set; }
        public int? DoneeId { get; set; }
        public string Status { get; set; }
        public DateTime? TransactionDate { get; set; }

        public virtual User Donee { get; set; }
        public virtual User Donor { get; set; }
        public virtual ICollection<DonationTransactionDetail> DonationTransactionDetails { get; set; }
    }
}
