using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class VwDonorChatConversation
    {
        public int ChatConversationId { get; set; }
        public int? DoneeId { get; set; }
        public int? DonorId { get; set; }
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
        public string LastChat { get; set; }
        public string LastSentAt { get; set; }
        public string ThisDoneeOrganizationName { get; set; }
        public string ThisProfilePicturePath { get; set; }
    }
}
