using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class VwFoodBeforeExpirationDay
    {
        public int FoodId { get; set; }
        public int? FoodCategoryId { get; set; }
        public string FoodName { get; set; }
        public int? Quantity { get; set; }
        public string Unit { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string FoodPicturePath { get; set; }
        public int UserFoodId { get; set; }
        public string FoodCategoryName { get; set; }
        public int UserId { get; set; }
        public decimal? DaysToExpire { get; set; }
    }
}
