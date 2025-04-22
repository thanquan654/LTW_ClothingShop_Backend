using BTL_ClothingShop.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_ClothingShop.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UserController : ControllerBase
    {

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetUserProfile ()
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }

        [HttpPut("me")]
        [Authorize]
        public IActionResult UpdateUserProfile([FromBody] object userProfileUpdateForm)
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }


        // Địa chỉ
        [HttpGet("me/addresses")]
        [Authorize]
        public IActionResult GetUserAddresses()
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }

        [HttpPost("me/addresses")]
        [Authorize]
        public IActionResult AddUserAddress([FromBody] object addressForm)
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }

        [HttpPut("me/addresses/{addressId}")]
        [Authorize]
        public IActionResult UpdateUserAddress(int addressId, [FromBody] object addressForm)
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }

        [HttpDelete("me/addresses/{addressId}")]
        [Authorize]
        public IActionResult DeleteUserAddress(int addressId)
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }

        [HttpPatch("me/addresses/{addressId}/set-default")]
        [Authorize]
        public IActionResult SetDefaultUserAddress(int addressId)
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }
    }
}
