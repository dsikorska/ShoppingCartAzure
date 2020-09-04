using ShoppingCart.DurableFunction.Shared.Models;
using System.Collections.Generic;

namespace ShoppingCart.DurableFunction.DataAccess.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public ICollection<CartProduct> Products { get; set; } = new List<CartProduct>();

        public Status Status { get; set; }

        public string Email { get; set; }
    }
}