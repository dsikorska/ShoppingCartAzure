using System.Collections.Generic;

namespace ShoppingCart.DurableFunction.DataAccess.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public ICollection<CartProduct> Products { get; set; } = new List<CartProduct>();
    }
}