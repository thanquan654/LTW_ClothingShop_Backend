using System;
using System.Collections.Generic;

namespace BTL_ClothingShop.Models;

public partial class MauSac
{
    public int MaMau { get; set; }

    public string? TenMau { get; set; }

    public virtual ICollection<BienTheSanPham> BienTheSanPhams { get; set; } = new List<BienTheSanPham>();
}
