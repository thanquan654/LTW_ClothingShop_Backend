using System.ComponentModel.DataAnnotations;

namespace BTL_ClothingShop.DTOs
{
    // DTO cho việc tạo đơn hàng mới
    public class CreateOrderRequestDto
    {
        [Required]
        public string MaUser { get; set; }
        [Required]
        public string PhuongThucThanhToan { get; set; }
        [Required]
        public string DiaChi { get; set; }
        [Required]
        [MinLength(1)]
        public List<OrderItemDto> Items { get; set; }
    }

    public class OrderItemDto
    {
        [Required]
        public int MaBienThe { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int SoLuong { get; set; }
    }

    // DTO để hiển thị thông tin tóm tắt đơn hàng
    public class OrderSummaryDto
    {
        public string MaDonHang { get; set; }
        public string MaUser { get; set; }
        public decimal TongTien { get; set; }
        public DateTime NgayDatHang { get; set; }
        public string TrangThaiDonHang { get; set; }
        public string DiaChi { get; set; }
    }

    // DTO để hiển thị chi tiết đơn hàng
    public class OrderDetailDto : OrderSummaryDto // Kế thừa từ Summary
    {
        public string PhuongThucThanhToan { get; set; }
        public UserInfoDto NguoiDat { get; set; } // Thêm thông tin người đặt nếu cần
        public List<OrderDetailItemDto> ChiTietDonHang { get; set; }
    }

    public class UserInfoDto
    {
        public string MaUser { get; set; }
        public string HoVaTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
    }


    public class OrderDetailItemDto
    {
        public int MaChiTietDonHang { get; set; }
        public int MaBienThe { get; set; }
        public int SoLuong { get; set; }
        public string Sku { get; set; } // Lấy từ Biến thể
        public string TenSanPham { get; set; } // Lấy từ Sản phẩm
        public string TenMau { get; set; } // Lấy từ Màu sắc
        public string TenKichCo { get; set; } // Lấy từ Kích cỡ
        public decimal GiaLucDat { get; set; } // Nên lưu giá tại thời điểm đặt hàng vào ChiTietDonHang
                                               // Hoặc lấy giá hiện tại từ SanPham nếu chấp nhận thay đổi
        public string AnhDaiDienSanPham { get; set; } // Lấy từ sản phẩm
    }

    // DTO cho việc thay đổi trạng thái đơn hàng
    public class ChangeOrderStatusRequestDto
    {
        [Required]
        public string NewStatus { get; set; }
    }
}
