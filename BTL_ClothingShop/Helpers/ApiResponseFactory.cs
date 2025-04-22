using BTL_ClothingShop.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BTL_ClothingShop.Helpers
{
    public static class ApiResponseFactory
    {
        public static IActionResult Success<T>(T data, string message = "", PaginationMetadata? pagination = null)
        {
            var result = new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Pagination = pagination
            };
            return new OkObjectResult(result);
        }

        public static IActionResult Created<T>(T data, string location, string message = "")
        {
            var result = new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
            return new CreatedResult(location, result);
        }

        public static IActionResult NoContentSuccess()
        {
            return new NoContentResult();
        }

        public static IActionResult Error(string message, int statusCode = 400)
        {
            var result = new ApiResponse<string>
            {
                Success = false,
                Message = message
            };
            return new ObjectResult(result) { StatusCode = statusCode };
        }
    }

}
