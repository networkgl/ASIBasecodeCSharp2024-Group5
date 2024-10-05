using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class Article
    {
        public int ArticleId { get; set; }
        public int? UserId { get; set; }
        public string Content { get; set; }

        public virtual User User { get; set; }
    }
}
