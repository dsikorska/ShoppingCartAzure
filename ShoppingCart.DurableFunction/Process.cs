using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShoppingCart.DurableFunction.DataAccess.Models;
using ShoppingCart.DurableFunction.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShoppingCart.DurableFunction
{
    public class Process
    {
        private readonly AppDbContext _context;

        public Process(AppDbContext context)
        {
            _context = context;
        }

        [FunctionName("Process")]
        public async Task<List<Product>> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var input = context.GetInput<CartDTO>();
            int cartId = await context.CallActivityAsync<int>("SaveCart", input);

            return null;
        }

        [FunctionName("SaveCart")]
        public async Task<int> SaveCart([ActivityTrigger] CartDTO input, ILogger log)
        {
            log.LogInformation($"Saving cart.");

            var cart = new Cart();
            var products = input.Products.Select(x => new CartProduct { Cart = cart, ProductId = x.ProductId, Quantity = x.Quantity }).ToList();
            cart.Products = products;

            _context.Add(cart);
            await _context.SaveChangesAsync();

            return cart.Id;
        }

        [FunctionName("GenerateSummary")]
        public void GenerateSummary([ActivityTrigger] int cartId, [Blob("summaries/{name}", FileAccess.Write)] Stream summary, ILogger log)
        {
            var cart = _context.Carts.FirstOrDefaultAsync(x => x.Id == cartId);

            if (cart == null)
            {
                log.LogError($"Cart {cartId} not found.");
            }
        }

        [FunctionName("Order")]
        public async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var body = await req.Content.ReadAsAsync<CartDTO>(default(CancellationToken));

            string instanceId = await starter.StartNewAsync("Process", null, body);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}