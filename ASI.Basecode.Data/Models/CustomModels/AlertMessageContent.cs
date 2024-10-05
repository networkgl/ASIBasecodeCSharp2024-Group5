using ASI.Basecode.Data.Interfaces;

namespace ASI.Basecode.Data.Models.CustomModels
{
    public class AlertMessageContent
    {
        public ErrorCode Status { get; set; }
        public string Message { get; set; }
    }
}
