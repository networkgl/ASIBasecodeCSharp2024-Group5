using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class UserAgent
    {
        public int UserAgentId { get; set; }
        public int? AgentId { get; set; }
        public string Expertise { get; set; }

        public virtual User Agent { get; set; }
    }
}
