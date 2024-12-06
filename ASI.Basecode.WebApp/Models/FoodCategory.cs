using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class FoodCategory
    {
        public FoodCategory()
        {
            StorageTipForFoodCategories = new HashSet<StorageTipForFoodCategory>();
        }

        public int FoodCategoryId { get; set; }
        public string FoodCategoryName { get; set; }

        public virtual ICollection<StorageTipForFoodCategory> StorageTipForFoodCategories { get; set; }
    }
}
