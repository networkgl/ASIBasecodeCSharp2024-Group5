using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class FoodShelfLife
    {
        public int Id { get; set; }
        public string FoodName { get; set; }
        public string Category { get; set; }
        public int? ShelfLifeInDays { get; set; }
        public string ContainedIngredients { get; set; }
    }
}
