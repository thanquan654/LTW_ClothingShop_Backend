// DTOs/AuthDTOs.cs
using System.ComponentModel.DataAnnotations;

namespace BTL_ClothingShop.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string HoVaTen { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("MatKhau", ErrorMessage = "Xác nhận mật khẩu không khớp")]
        public string XacNhanMatKhau { get; set; }
    }

    public class LoginDTO
    {
        [Required(ErrorMessage = "Email hoặc số điện thoại là bắt buộc")]
        public string EmailOrPhone { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string MatKhau { get; set; }
    }

    public class UserUpdateDTO
    {
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string HoVaTen { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDienThoai { get; set; }
    }

    public class UserDTO
    {
        public string Id { get; set; }
        public string HoVaTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string VaiTro { get; set; }
    }
}