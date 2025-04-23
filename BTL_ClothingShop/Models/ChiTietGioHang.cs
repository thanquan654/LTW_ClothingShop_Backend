using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_ClothingShop.Models;

public partial class ChiTietGioHang
{
    public int MaChiTietGioHang { get; set; }

    public string? MaGioHang { get; set; }

    public int? MaBienThe { get; set; }

    public int? SoLuong { get; set; }

    public virtual BienTheSanPham? MaBienTheNavigation { get; set; }

    public virtual GioHang? MaGioHangNavigation { get; set; }

    [ForeignKey("MaGioHang")]
    public virtual GioHang? GioHang { get; set; } // Đến Giỏ hàng (Many-to-One)

    [ForeignKey("MaBienThe")]
    public virtual BienTheSanPham? BienTheSanPham { get; set; } // Đến Biến thể (Many-to-One)
}
