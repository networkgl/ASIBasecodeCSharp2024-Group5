using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class ChatConversation
    {
        public ChatConversation()
        {
            Chats = new HashSet<Chat>();
        }

        public int ChatConversationId { get; set; }
        public int? DonorId { get; set; }
        public int? DoneeId { get; set; }

        public virtual User Donee { get; set; }
        public virtual User Donor { get; set; }
        public virtual ICollection<Chat> Chats { get; set; }
    }
}
