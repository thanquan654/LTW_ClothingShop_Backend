using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using BTL_ClothingShop.Models;

namespace BTL_ClothingShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly CsdlshopThoiTrangContext _context = new CsdlshopThoiTrangContext();

        [HttpGet("overview")]
        public async Task<IActionResult> GetDashboardOverview()
        {
            var now = DateTime.Now;
            var firstDay = new DateTime(now.Year, now.Month, 1);
            var lastDay = firstDay.AddMonths(1);

            // Doanh thu tháng
            var monthlyRevenue = await _context.DonHangs
                .Where(d => d.NgayDatHang >= firstDay && d.NgayDatHang < lastDay)
                .SumAsync(d => d.TongTien ?? 0);

            // Số đơn trong tháng
            var monthlyOrders = await _context.DonHangs
                .CountAsync(d => d.NgayDatHang >= firstDay && d.NgayDatHang < lastDay);

            // Danh sách đơn gần nhất (5 đơn mới nhất)
            var recentOrders = await _context.DonHangs
                .OrderByDescending(d => d.NgayDatHang)
                .Take(5)
                .Select(d => new {
                    d.MaDonHang,
                    d.NgayDatHang,
                    d.TongTien,
                    d.TrangThaiDonHang
                })
                .ToListAsync();

            // Sản phẩm bán chạy (top 5 theo số lượng bán ra trong tháng)
            var bestSellingProducts = await _context.ChiTietDonHangs
                .Where(ct => ct.MaDonHangNavigation != null && ct.MaDonHangNavigation.NgayDatHang >= firstDay && ct.MaDonHangNavigation.NgayDatHang < lastDay)
                .GroupBy(ct => ct.MaBienThe)
                .Select(g => new {
                    MaBienThe = g.Key,
                    SoLuongBan = g.Sum(x => x.SoLuong ?? 0),
                    TenSanPham = g.FirstOrDefault().MaBienTheNavigation.MaSanPhamNavigation.TenSanPham,
                    TenMau = g.FirstOrDefault().MaBienTheNavigation.MaMauNavigation.TenMau,
                    TenKichCo = g.FirstOrDefault().MaBienTheNavigation.MaKichCoNavigation.TenKichCo
                })
                .OrderByDescending(x => x.SoLuongBan)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                monthlyRevenue,
                monthlyOrders,
                recentOrders,
                bestSellingProducts
            });
        }
    }
}
