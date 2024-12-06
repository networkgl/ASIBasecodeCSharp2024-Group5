using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class Food
    {
        public Food()
        {
            FoodIngredients = new HashSet<FoodIngredient>();
            Notifcations = new HashSet<Notifcation>();
            UserFoods = new HashSet<UserFood>();
        }

        public int FoodId { get; set; }
        public int? FoodCategoryId { get; set; }
        public string FoodName { get; set; }
        public int? Quantity { get; set; }
        public string Unit { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string FoodPicturePath { get; set; }

        public virtual ICollection<FoodIngredient> FoodIngredients { get; set; }
        public virtual ICollection<Notifcation> Notifcations { get; set; }
        public virtual ICollection<UserFood> UserFoods { get; set; }
    }
}
