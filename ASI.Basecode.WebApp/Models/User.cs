using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class User
    {
        public User()
        {
            ChatConversationDonees = new HashSet<ChatConversation>();
            ChatConversationDonors = new HashSet<ChatConversation>();
            Chats = new HashSet<Chat>();
            DonationTransactionMasterDonees = new HashSet<DonationTransactionMaster>();
            DonationTransactionMasterDonors = new HashSet<DonationTransactionMaster>();
            NotifcationCreatedByNavigations = new HashSet<Notifcation>();
            NotifcationUpdatedByNavigations = new HashSet<Notifcation>();
            NotifcationUsers = new HashSet<Notifcation>();
            PaymentTransactions = new HashSet<PaymentTransaction>();
            UserFoods = new HashSet<UserFood>();
            UserPlans = new HashSet<UserPlan>();
            UserRoles = new HashSet<UserRole>();
        }

        public int UserId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FoodBusinessName { get; set; }
        public string DoneeOrganizationName { get; set; }
        public DateTime? Birthdate { get; set; }
        public string Gender { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string Barangay { get; set; }
        public string ProfilePicturePath { get; set; }
        public string ProofPicturePath { get; set; }
        public bool? AccountApproved { get; set; }
        public bool? EmailConfirmed { get; set; }
        public int? StorageSize { get; set; }
        public int? FoodStoredCount { get; set; }
        public string Availability { get; set; }

        public virtual ICollection<ChatConversation> ChatConversationDonees { get; set; }
        public virtual ICollection<ChatConversation> ChatConversationDonors { get; set; }
        public virtual ICollection<Chat> Chats { get; set; }
        public virtual ICollection<DonationTransactionMaster> DonationTransactionMasterDonees { get; set; }
        public virtual ICollection<DonationTransactionMaster> DonationTransactionMasterDonors { get; set; }
        public virtual ICollection<Notifcation> NotifcationCreatedByNavigations { get; set; }
        public virtual ICollection<Notifcation> NotifcationUpdatedByNavigations { get; set; }
        public virtual ICollection<Notifcation> NotifcationUsers { get; set; }
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; }
        public virtual ICollection<UserFood> UserFoods { get; set; }
        public virtual ICollection<UserPlan> UserPlans { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
