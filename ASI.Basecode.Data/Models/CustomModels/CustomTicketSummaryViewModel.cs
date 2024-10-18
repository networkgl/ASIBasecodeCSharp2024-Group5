using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Models.CustomModels
{
    public class CustomTicketSummaryViewModel
    {
        public List<CategoryDataModel> Categories { get; set; } = new List<CategoryDataModel>();
        public List<StatusDataModel> Statuses { get; set; } = new List<StatusDataModel>();
        public List<PriorityDataModel> Priorities { get; set; } = new List<PriorityDataModel>();
        public List<UserActivityDataModel> UserActivities { get; set; } = new List<UserActivityDataModel>();
    }

    public class CategoryDataModel
    {
        public string CategoryName { get; set; }
        public int TicketCount { get; set; }
    }

    public class StatusDataModel
    {
        public string StatusName { get; set; }
        public int TicketCount { get; set; }
    }

    public class PriorityDataModel
    {
        public string PriorityName { get; set; }
        public int TicketCount { get; set; }
    }

    public class UserActivityDataModel
    {
        public string UserName { get; set; }
        public int TicketCount { get; set; }
    }
}
