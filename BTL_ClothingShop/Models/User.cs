using System;
using System.Collections.Generic;

namespace BTL_ClothingShop.Models;

public partial class User
{
    public string MaUser { get; set; } = null!;

    public string? HoVaTen { get; set; }

    public string? Email { get; set; }

    public string? SoDienThoai { get; set; }

    public string? MatKhau { get; set; }

    public string? VaiTro { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();
}
