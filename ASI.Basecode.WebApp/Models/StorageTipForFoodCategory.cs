using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class StorageTipForFoodCategory
    {
        public int StorageTipFoFoodCategoryId { get; set; }
        public int? StorageTipId { get; set; }
        public int? FoodCategoryId { get; set; }

        public virtual FoodCategory FoodCategory { get; set; }
        public virtual StorageTip StorageTip { get; set; }
    }
}
