using System;
using System.Collections.Generic;

namespace BTL_ClothingShop.Models;

public partial class DanhMuc
{
    public string MaDanhMuc { get; set; } = null!;

    public string? TenDanhMuc { get; set; }

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
