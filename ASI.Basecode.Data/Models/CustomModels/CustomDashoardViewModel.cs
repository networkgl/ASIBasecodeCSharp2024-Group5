namespace ASI.Basecode.Data.Models.CustomModels
{
    public class CustomDashoardViewModel
    {
        public int? UserCount { get; set; }
        public int? AgentCount { get; set; }
        public int? TicketsResolvedCount{ get; set; }
        public int? TicketsAssignedByMeCount { get; set; }
        public int? TicketsAssignedToYouCount { get; set; }
        public int? TotalTicketCreatedByMe { get; set; }
        public int? UnresolvedTicketForReporterCount { get; set; }
        public int? ResolvedTicketForReporterCount { get; set; }
    }
}
