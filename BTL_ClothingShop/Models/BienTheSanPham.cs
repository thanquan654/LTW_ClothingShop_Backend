using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_ClothingShop.Models;

public partial class BienTheSanPham
{
    public int MaBienThe { get; set; }

    public int? MaSanPham { get; set; }

    public virtual SanPham? MaSanPhamNavigation { get; set; }

    public int? MaKichCo { get; set; }

    public int? MaMau { get; set; }

    public string? Sku { get; set; }

    public int? SoLuongTon { get; set; }

    public int? LuotBan { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    public virtual KichCo? MaKichCoNavigation { get; set; }

    public virtual MauSac? MaMauNavigation { get; set; }
}
