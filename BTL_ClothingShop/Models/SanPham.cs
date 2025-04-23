using System;
using System.Collections.Generic;

namespace BTL_ClothingShop.Models;

public partial class SanPham
{
    public int MaSanPham { get; set; }

    public string? TenSanPham { get; set; }

    public string? AnhDaiDien { get; set; }

    public decimal? GiaTien { get; set; }

    public string? MoTaSanPham { get; set; }

    public string? MaDanhMuc { get; set; }

    public virtual ICollection<AnhSanPham> AnhSanPhams { get; set; } = new List<AnhSanPham>();

    public virtual ICollection<BienTheSanPham> BienTheSanPhams { get; set; } = new List<BienTheSanPham>();

    public virtual DanhMuc? MaDanhMucNavigation { get; set; }
}
