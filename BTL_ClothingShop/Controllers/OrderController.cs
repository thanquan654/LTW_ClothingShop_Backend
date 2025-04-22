using BTL_ClothingShop.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BTL_ClothingShop.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class OrderController : ControllerBase
    {
        // POST: api/orders
        // This endpoint is used to create a new order.
        [HttpPost("/")]
        public IActionResult CreateOrder([FromBody] object createOrderBody)
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }

        //GET: api/orders?userId=...
        // This endpoint is used to get a list of orders for a specific user.
        [HttpGet("/")]
        public IActionResult GetOrders([FromQuery] string? userId)
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }


        // GET: api/orders/{orderId}?userId=...
        // This endpoint is used to get the details of a specific order.
        [HttpGet("{orderId}")]
        public IActionResult GetOrderDetails([FromRoute] string orderId, [FromQuery] string? userId)
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }

        // PUT: api/orders/{orderId}/cancel
        // This endpoint is used to cancel a specific order.
        [HttpPut("{orderId}/cancel")]
        public IActionResult CancelOrder([FromRoute] string orderId, [FromBody] object reqBody)
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }

        // PUT: api/orders/{orderId}/change-status
        // This endpoint is used to change the status of a specific order.
        [HttpPut("{orderId}/change-status")]
        public IActionResult ChangeOrderStatus([FromRoute] string orderId, [FromBody] object reqBody)
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }

    }
}
