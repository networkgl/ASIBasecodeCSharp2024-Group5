using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class RecommendedFood
    {
        public int Id { get; set; }
        public string FoodName { get; set; }
        public string RecommendedRecipe { get; set; }
        public string IncludedFoods { get; set; }
        public string Steps { get; set; }
    }
}
