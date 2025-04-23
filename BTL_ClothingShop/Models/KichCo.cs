using System;
using System.Collections.Generic;

namespace BTL_ClothingShop.Models;

public partial class KichCo
{
    public int MaKichCo { get; set; }

    public string? TenKichCo { get; set; }

    public virtual ICollection<BienTheSanPham> BienTheSanPhams { get; set; } = new List<BienTheSanPham>();
}
