// Controllers/AuthController.cs
using BTL_ClothingShop.DTOs;
using BTL_ClothingShop.Helpers;
using BTL_ClothingShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BTL_ClothingShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly CsdlshopThoiTrangContext _context = new CsdlshopThoiTrangContext();

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            try
            {
                // Kiểm tra email tồn tại
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    return ApiResponseFactory.Error("Email đã được sử dụng", 400);
                }

                // Kiểm tra số điện thoại tồn tại
                if (await _context.Users.AnyAsync(u => u.SoDienThoai == model.SoDienThoai))
                {
                    return ApiResponseFactory.Error("Số điện thoại đã được sử dụng", 400);
                }

                // Hash mật khẩu
                string hashedPassword = HashPassword(model.MatKhau);

                // Tạo user mới
                var user = new User
                {
                    MaUser = Guid.NewGuid().ToString(),
                    HoVaTen = model.HoVaTen,
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    MatKhau = hashedPassword,
                    VaiTro = "Customer"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Tạo response
                var userDto = new UserDTO
                {
                    Id = user.MaUser,
                    HoVaTen = user.HoVaTen,
                    Email = user.Email,
                    SoDienThoai = user.SoDienThoai,
                    VaiTro = user.VaiTro
                };

                return ApiResponseFactory.Created(userDto, $"/api/users/{user.MaUser}", "Đăng ký thành công");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Error("Có lỗi xảy ra khi đăng ký", 500);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            try
            {
                // Tìm user theo email hoặc số điện thoại
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.EmailOrPhone ||
                                            u.SoDienThoai == model.EmailOrPhone);

                if (user == null)
                {
                    return ApiResponseFactory.Error("Thông tin đăng nhập không chính xác", 400);
                }

                // Kiểm tra mật khẩu
                if (!VerifyPassword(model.MatKhau, user.MatKhau))
                {
                    return ApiResponseFactory.Error("Thông tin đăng nhập không chính xác", 400);
                }

                var userDto = new UserDTO
                {
                    Id = user.MaUser,
                    HoVaTen = user.HoVaTen,
                    Email = user.Email,
                    SoDienThoai = user.SoDienThoai,
                    VaiTro = user.VaiTro
                };

                return ApiResponseFactory.Success(new { User = userDto }, "Đăng nhập thành công");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Error("Có lỗi xảy ra khi đăng nhập", 500);
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }
    }
}