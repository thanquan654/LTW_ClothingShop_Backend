using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_ClothingShop.Models;

public partial class ChiTietDonHang
{
    public int MaChiTietDonHang { get; set; }
    public string? MaDonHang { get; set; }
    public int? MaBienThe { get; set; }
    public int? SoLuong { get; set; }

    // CHỈ GIỮ LẠI 1 navigation property cho mỗi FK
    public virtual DonHang? MaDonHangNavigation { get; set; }
    public virtual BienTheSanPham? MaBienTheNavigation { get; set; }
}
