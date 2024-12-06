using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class FoodIngredient
    {
        public int FoodIngredientId { get; set; }
        public int? FoodId { get; set; }
        public string IngredientName { get; set; }

        public virtual Food Food { get; set; }
    }
}
