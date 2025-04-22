using BTL_ClothingShop.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BTL_ClothingShop.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] object userRegistrationForm)
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] object userLoginForm)
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }

    }
}
