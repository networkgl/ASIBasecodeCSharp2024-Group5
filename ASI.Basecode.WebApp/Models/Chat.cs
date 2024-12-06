using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class Chat
    {
        public int ChatId { get; set; }
        public string ChatMessage { get; set; }
        public DateTime? SentAt { get; set; }
        public int? ChatConversationId { get; set; }
        public int? SenderId { get; set; }

        public virtual ChatConversation ChatConversation { get; set; }
        public virtual User Sender { get; set; }
    }
}
