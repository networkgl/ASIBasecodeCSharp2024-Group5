using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class StorageTip
    {
        public StorageTip()
        {
            StorageTipForFoodCategories = new HashSet<StorageTipForFoodCategory>();
        }

        public int StorageTipId { get; set; }
        public string TipText { get; set; }

        public virtual ICollection<StorageTipForFoodCategory> StorageTipForFoodCategories { get; set; }
    }
}
