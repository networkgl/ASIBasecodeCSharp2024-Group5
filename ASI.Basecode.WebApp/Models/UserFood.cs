using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public partial class UserFood
    {
        public int UserFoodId { get; set; }
        public int? UserId { get; set; }
        public int? FoodId { get; set; }
        public bool? Display { get; set; }

        public virtual Food Food { get; set; }
        public virtual User User { get; set; }
    }
}
