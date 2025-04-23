using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_ClothingShop.Models;

public partial class ChiTietDonHang
{
    public int MaChiTietDonHang { get; set; }

    public string? MaDonHang { get; set; }

    [ForeignKey("maDonHang")]
    public virtual DonHang DonHang { get; set; }

    public int? MaBienThe { get; set; }

    [ForeignKey("MaBienThe")]
    public virtual BienTheSanPham? BienTheSanPham { get; set; }

    public int? SoLuong { get; set; }

    public virtual BienTheSanPham? MaBienTheNavigation { get; set; }

    public virtual DonHang? MaDonHangNavigation { get; set; }
}
