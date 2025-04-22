using BTL_ClothingShop.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BTL_ClothingShop.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ProductController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAllProducts()
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }
        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }

        // Chưa kiểm tra lại
        [HttpPost]
        public IActionResult CreateProduct([FromBody] object productForm)
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] object productForm)
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }
    }    {
        
    }
}
