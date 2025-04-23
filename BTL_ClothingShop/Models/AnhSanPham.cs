using System;
using System.Collections.Generic;

namespace BTL_ClothingShop.Models;

public partial class AnhSanPham
{
    public string MaAnhSanPham { get; set; } = null!;

    public int? MaSanPham { get; set; }

    public string? LinkAnh { get; set; }

    public virtual SanPham? MaSanPhamNavigation { get; set; }
}
