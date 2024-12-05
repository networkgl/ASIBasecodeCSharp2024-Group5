using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class VwAverageResolutionTime
    {
        public int? AgentId { get; set; }
        public string AgentName { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public int? TotalResolvedCount { get; set; }
        public decimal? AvgResolutionTime { get; set; }
    }
}
