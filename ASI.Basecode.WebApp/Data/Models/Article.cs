using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Data.Models
{
    public partial class Article
    {
        public int ArticleId { get; set; }
        public int? UserId { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }

        public virtual User User { get; set; }
    }
}
