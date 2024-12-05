using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class VwNeedApprovalArticle
    {
        public int ArticleId { get; set; }
        public int? UserId { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int? UpdatedBy { get; set; }
        public string Approved { get; set; }
        public string PreviousContent { get; set; }
        public string PreviousTitle { get; set; }
    }
}
