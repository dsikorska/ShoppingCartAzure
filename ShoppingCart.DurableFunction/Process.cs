using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using ShoppingCart.DurableFunction.DataAccess.Models;
using ShoppingCart.DurableFunction.Models;
using ShoppingCart.DurableFunction.Shared.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShoppingCart.DurableFunction
{
    public class Process
    {
        private const string ACCEPT_ORDER = "AcceptOrder";

        private readonly AppDbContext _context;

        public Process(AppDbContext context)
        {
            _context = context;
        }

        [FunctionName("Process")]
        public async Task<CartDTO> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var input = context.GetInput<CartDTO>();
            int cartId = await context.CallActivityAsync<int>(nameof(SaveCart), input);

            using (var cts = new CancellationTokenSource())
            {
                DateTime timer = context.CurrentUtcDateTime.AddMinutes(10);
                Task timeout = context.CreateTimer(timer, cts.Token);
                Task<bool> orderConfirmed = context.WaitForExternalEvent<bool>(ACCEPT_ORDER);

                if (orderConfirmed == await Task.WhenAny(orderConfirmed, timeout))
                {
                    cts.Cancel();
                    await context.CallActivityAsync(nameof(StatusProcessingCart), cartId);
                }
                else
                {
                    await context.CallActivityAsync(nameof(StatusRejectedCart), cartId);
                }
            }

            return input;
        }

        [FunctionName("SaveCart")]
        public async Task<int> SaveCart([ActivityTrigger] CartDTO input, ILogger log)
        {
            log.LogInformation($"Saving cart.");

            var cart = new Cart { Status = Status.New, Email = input.Email };
            var products = input.Products.Select(x => new CartProduct { Cart = cart, ProductId = x.ProductId, Quantity = x.Quantity }).ToList();
            cart.Products = products;

            _context.Add(cart);
            await _context.SaveChangesAsync();

            return cart.Id;
        }

        [FunctionName("StatusProcessingCart")]
        public async Task StatusProcessingCart([ActivityTrigger] int cartId, ILogger log)
        {
            Cart cart = _context.Find<Cart>(cartId) ?? throw new ArgumentException($"Cart ({cartId}) not found.");

            cart.Status = Status.Processing;

            _context.Update(cart);
            await _context.SaveChangesAsync();
        }

        [FunctionName("StatusRejectedCart")]
        public async Task StatusRejectedCart([ActivityTrigger] int cartId, ILogger log)
        {
            Cart cart = _context.Find<Cart>(cartId) ?? throw new ArgumentException($"Cart ({cartId}) not found.");
            cart.Status = Status.Rejected;

            _context.Update(cart);
            await _context.SaveChangesAsync();
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