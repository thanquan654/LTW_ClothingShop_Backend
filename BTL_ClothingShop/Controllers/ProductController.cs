// Controllers/ProductController.cs

using BTL_ClothingShop.DTOs;
using BTL_ClothingShop.Helpers;
using BTL_ClothingShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_ClothingShop.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly CsdlshopThoiTrangContext _context;

        public ProductController(CsdlshopThoiTrangContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] ProductFilterDTO filter)
        {
            var query = _context.SanPhams
                .Include(p => p.AnhSanPhams)
                .Include(p => p.BienTheSanPhams)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(p => p.TenSanPham.Contains(filter.Search) || 
                                       p.MoTaSanPham.Contains(filter.Search));
            }

            if (!string.IsNullOrEmpty(filter.TenDanhMuc))
            {
                query = query.Where(p => p.MaDanhMucNavigation.TenDanhMuc == filter.TenDanhMuc);
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.GiaTien >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.GiaTien <= filter.MaxPrice.Value);
            }

            // Apply sorting
            query = filter.SortBy.ToLower() switch
            {
                "giatien" => filter.SortDirection.ToLower() == "desc" 
                    ? query.OrderByDescending(p => p.GiaTien)
                    : query.OrderBy(p => p.GiaTien),
                "luotban" => filter.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.BienTheSanPhams.Sum(bt => bt.LuotBan))
                    : query.OrderBy(p => p.BienTheSanPhams.Sum(bt => bt.LuotBan)),
                _ => filter.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.TenSanPham)
                    : query.OrderBy(p => p.TenSanPham)
            };

            // Apply pagination
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);

            var products = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new ProductListItemDTO
                {
                    Id = p.MaSanPham.ToString(),
                    TenSanPham = p.TenSanPham ?? "",
                    GiaTien = p.GiaTien ?? 0,
                    AnhDaiDien = p.AnhSanPhams.Any() ? p.AnhSanPhams.First().LinkAnh ?? "" : "",
                    LuotBan = p.BienTheSanPhams.Sum(bt => bt.LuotBan ?? 0)
                })
                .ToListAsync();

            var pagination = new PaginationMetadata
            {
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };

            return ApiResponseFactory.Success(products, pagination: pagination);
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            return ApiResponseFactory.Error("This endpoint is not implemented yet.", 500);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try 
            {
                // Tải sản phẩm cơ bản
                var product = await _context.SanPhams
                    .Include(p => p.MaDanhMucNavigation)
                    .FirstOrDefaultAsync(p => p.MaSanPham == id);

                if (product == null)
                {
                    return ApiResponseFactory.Error("Sản phẩm không tồn tại", 404);
                }

                // Tải danh sách ảnh
                var anhSanPhams = await _context.AnhSanPhams
                    .Where(a => a.MaSanPham == id)
                    .ToListAsync();

                // Tải biến thể sản phẩm với thông tin kích cỡ và màu sắc
                var bienTheSanPhams = await _context.BienTheSanPhams
                    .Where(bt => bt.MaSanPham == id)
                    .Include(bt => bt.MaKichCoNavigation)
                    .Include(bt => bt.MaMauNavigation)
                    .ToListAsync();

                // Lấy danh sách kích cỡ riêng biệt
                var kichCos = bienTheSanPhams
                    .Where(bt => bt.MaKichCoNavigation != null)
                    .Select(bt => bt.MaKichCoNavigation)
                    .GroupBy(k => k.MaKichCo)
                    .Select(g => g.First())
                    .ToList();

                // Lấy danh sách màu sắc riêng biệt
                var mauSacs = bienTheSanPhams
                    .Where(bt => bt.MaMauNavigation != null)
                    .Select(bt => bt.MaMauNavigation)
                    .GroupBy(m => m.MaMau)
                    .Select(g => g.First())
                    .ToList();

                // Tính tổng số lượng tồn và lượt bán
                int soLuongTon = bienTheSanPhams.Sum(bt => bt.SoLuongTon ?? 0);
                int luotBan = bienTheSanPhams.Sum(bt => bt.LuotBan ?? 0);

                // Tạo DTO
                var productDetail = new ProductDetailDTO
                {
                    Id = product.MaSanPham.ToString(),
                    TenSanPham = product.TenSanPham ?? "",
                    GiaTien = product.GiaTien ?? 0,
                    MoTaSanPham = product.MoTaSanPham ?? "",
                    TenDanhMuc = product.MaDanhMucNavigation?.TenDanhMuc ?? "",
                    SoLuongTon = soLuongTon,
                    LuotBan = luotBan,
                    AnhSanPham = anhSanPhams.Select(a => new ProductImageDTO
                    {
                        Id = a.MaAnhSanPham,
                        LinkAnh = a.LinkAnh ?? ""
                    }).ToList(),
                    KichCo = kichCos.Select(s => new SizeDTO
                    {
                        Id = s.MaKichCo.ToString(),
                        KichCo = s.TenKichCo ?? ""
                    }).ToList(),
                    MauSac = mauSacs.Select(c => new ColorDTO
                    {
                        Id = c.MaMau.ToString(),
                        Mau = c.TenMau ?? ""
                    }).ToList()
                };

                return ApiResponseFactory.Success(productDetail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProductById: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return ApiResponseFactory.Error("Có lỗi xảy ra khi lấy thông tin sản phẩm", 500);
            }
        }
    }
                
            
}