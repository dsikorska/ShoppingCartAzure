using System.Collections.Generic;

namespace ShoppingCart.DurableFunction.Models
{
    public class CartDTO
    {
        public IEnumerable<CartProductDTO> Products { get; set; } = new List<CartProductDTO>();
    }
}