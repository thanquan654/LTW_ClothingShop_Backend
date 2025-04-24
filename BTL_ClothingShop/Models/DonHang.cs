using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BTL_ClothingShop.Models;

public partial class DonHang
{
    public string MaDonHang { get; set; } = null!;

    public string? MaUser { get; set; }

    [ForeignKey("MaUser")]
    public virtual User? User { get; set; }

    public string? PhuongThucThanhToan { get; set; }

    public decimal? TongTien { get; set; }

    public DateTime? NgayDatHang { get; set; }

    public string? TrangThaiDonHang { get; set; }

    public string? DiaChi { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
}
