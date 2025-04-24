using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
        public List<OrderItemRequestDto> Items { get; set; }
    }
    public class OrderItemRequestDto // DTO cho mỗi object trong mảng 'items'
    {
        // Sử dụng JsonPropertyName để khớp chính xác với tên key trong JSON từ frontend
        [JsonPropertyName("id")]
        [Required]
        public int MaSanPham { get; set; } // Nhận 'id' từ frontend và map vào MaSanPham

        [JsonPropertyName("color")]
        [Required]
        public int MaMau { get; set; } // Nhận 'color' từ frontend và map vào MaMau

        [JsonPropertyName("size")]
        [Required]
        public int MaKichCo { get; set; } // Nhận 'size' từ frontend và map vào MaKichCo

        [JsonPropertyName("quantity")]
        [Required]
        [Range(1, int.MaxValue)]
        public int SoLuong { get; set; } // Nhận 'quantity' từ frontend và map vào SoLuong
    }

    // DTO để hiển thị thông tin tóm tắt đơn hàng

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

    // DTO cho mỗi item trong request từ frontend
    public class CreateOrderItemFromJsDto
    {
        [Required]
        public int Id { get; set; } // Đây sẽ là MaSanPham

        [Required]
        public string Color { get; set; } // Đây sẽ là TenMau

        [Required]
        public string Size { get; set; } // Đây sẽ là TenKichCo

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } // Đây sẽ là SoLuong
    }

    // DTO cho toàn bộ request body từ frontend


    
    // DTO trả về (Đảm bảo có TongTien)
    public class OrderSummaryDto
    {
        public string MaDonHang { get; set; }
        public string MaUser { get; set; }
        public decimal TongTien { get; set; } // Thêm trường này
        public DateTime NgayDatHang { get; set; }
        public string TrangThaiDonHang { get; set; }
        public string DiaChi { get; set; }
    }


}
