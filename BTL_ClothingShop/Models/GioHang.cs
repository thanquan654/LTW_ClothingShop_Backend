using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_ClothingShop.Models;

public partial class GioHang
{
    public string MaGioHang { get; set; } = null!;

    public string? MaUser { get; set; }

    [ForeignKey("MaUser")]
    public virtual User? User { get; set; }

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    public virtual User? MaUserNavigation { get; set; }
}
