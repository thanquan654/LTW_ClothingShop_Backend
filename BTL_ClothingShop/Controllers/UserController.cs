// Controllers/UserController.cs
using BTL_ClothingShop.DTOs;
using BTL_ClothingShop.Helpers;
using BTL_ClothingShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_ClothingShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly CsdlshopThoiTrangContext _context;

        public UserController(CsdlshopThoiTrangContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserProfile(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return ApiResponseFactory.Error("Người dùng không tồn tại", 404);
                }

                var userDto = new UserDTO
                {
                    Id = user.MaUser,
                    HoVaTen = user.HoVaTen,
                    Email = user.Email,
                    SoDienThoai = user.SoDienThoai,
                    VaiTro = user.VaiTro
                };

                return ApiResponseFactory.Success(userDto);
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Error("Có lỗi xảy ra khi lấy thông tin người dùng", 500);
            }
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUserProfile(string userId, [FromBody] UserUpdateDTO model)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return ApiResponseFactory.Error("Người dùng không tồn tại", 404);
                }

                // Kiểm tra số điện thoại tồn tại
                if (!string.IsNullOrEmpty(model.SoDienThoai) &&
                    model.SoDienThoai != user.SoDienThoai &&
                    await _context.Users.AnyAsync(u => u.SoDienThoai == model.SoDienThoai))
                {
                    return ApiResponseFactory.Error("Số điện thoại đã được sử dụng", 400);
                }

                // Cập nhật thông tin
                if (!string.IsNullOrEmpty(model.HoVaTen))
                    user.HoVaTen = model.HoVaTen;

                if (!string.IsNullOrEmpty(model.SoDienThoai))
                    user.SoDienThoai = model.SoDienThoai;

                await _context.SaveChangesAsync();

                var userDto = new UserDTO
                {
                    Id = user.MaUser,
                    HoVaTen = user.HoVaTen,
                    Email = user.Email,
                    SoDienThoai = user.SoDienThoai,
                    VaiTro = user.VaiTro
                };

                return ApiResponseFactory.Success(userDto, "Cập nhật thông tin thành công");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Error("Có lỗi xảy ra khi cập nhật thông tin người dùng", 500);
            }
        }
    }
}