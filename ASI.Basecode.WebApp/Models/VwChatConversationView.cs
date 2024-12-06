using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class VwChatConversationView
    {
        public int ChatConversationId { get; set; }
        public int? DonorId { get; set; }
        public int? DoneeId { get; set; }
        public string DoneesEmail { get; set; }
        public string DoneeOrganizationName { get; set; }
        public string DoneesProvince { get; set; }
        public string DoneesCity { get; set; }
        public string DoneesBarangay { get; set; }
        public string DoneesProfilePicturePath { get; set; }
        public string DoneesProofPicturePath { get; set; }
        public bool? DoneesAccountApproved { get; set; }
        public string Availability { get; set; }
        public string DonorsEmail { get; set; }
        public string DonorsFirstName { get; set; }
        public string DonorsLastName { get; set; }
        public string DonorsBusinesName { get; set; }
        public string DonorsProvince { get; set; }
        public string DonorsCity { get; set; }
        public string DonorsBarangay { get; set; }
        public string DonorsProfilePicturePath { get; set; }
        public string BusinessProofPicturePath { get; set; }
        public bool? DonorsAccountApproved { get; set; }
        public int ChatId { get; set; }
        public string ChatMessage { get; set; }
        public int? SenderId { get; set; }
        public DateTime? SentAt { get; set; }
        public int LoggedInUserId { get; set; }
    }
}
