// Controllers/ProductController.cs

using BTL_ClothingShop.DTOs;
using BTL_ClothingShop.Helpers;
using BTL_ClothingShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace BTL_ClothingShop.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly CsdlshopThoiTrangContext _context = new CsdlshopThoiTrangContext();

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
        public async Task<IActionResult> GetProductByIdOptimized(int id)
        {
            try
            {
                var productDetail = await _context.SanPhams
                    .Where(p => p.MaSanPham == id)
                    .Select(p => new // Tạo một anonymous type hoặc DTO tạm thời trong query
                    {
                        Product = p,
                        CategoryName = p.MaDanhMucNavigation.TenDanhMuc, // Lấy tên danh mục
                        Images = p.AnhSanPhams.Select(a => new { a.MaAnhSanPham, a.LinkAnh }).ToList(), // Lấy ảnh
                        Variants = p.BienTheSanPhams.Select(bt => new // Lấy thông tin biến thể cần thiết
                        {
                            bt.SoLuongTon,
                            bt.LuotBan,
                            Size = bt.MaKichCoNavigation == null ? null : new { bt.MaKichCoNavigation.MaKichCo, bt.MaKichCoNavigation.TenKichCo },
                            Color = bt.MaMauNavigation == null ? null : new { bt.MaMauNavigation.MaMau, bt.MaMauNavigation.TenMau }
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(); // Chỉ 1 lần gọi DB

                if (productDetail == null || productDetail.Product == null)
                {
                    return ApiResponseFactory.Error("Sản phẩm không tồn tại", 404);
                }

                // Xử lý dữ liệu đã lấy được trong bộ nhớ (ít hơn so với cách cũ)
                var uniqueSizes = productDetail.Variants
                    .Where(v => v.Size != null)
                    .GroupBy(v => v.Size.MaKichCo)
                    .Select(g => g.First().Size) // Lấy object Size duy nhất
                    .Select(s => new SizeDTO { Id = s.MaKichCo.ToString(), KichCo = s.TenKichCo ?? "" })
                    .ToList();

                var uniqueColors = productDetail.Variants
                    .Where(v => v.Color != null)
                    .GroupBy(v => v.Color.MaMau)
                    .Select(g => g.First().Color) // Lấy object Color duy nhất
                    .Select(c => new ColorDTO { Id = c.MaMau.ToString(), Mau = c.TenMau ?? "" })
                    .ToList();

                int totalStock = productDetail.Variants.Sum(v => v.SoLuongTon ?? 0);
                int totalSales = productDetail.Variants.Sum(v => v.LuotBan ?? 0);

                // Map vào DTO cuối cùng
                var finalDto = new ProductDetailDTO
                {
                    Id = productDetail.Product.MaSanPham.ToString(),
                    TenSanPham = productDetail.Product.TenSanPham ?? "",
                    GiaTien = productDetail.Product.GiaTien ?? 0,
                    MoTaSanPham = productDetail.Product.MoTaSanPham ?? "",
                    TenDanhMuc = productDetail.CategoryName ?? "",
                    SoLuongTon = totalStock,
                    LuotBan = totalSales,
                    AnhSanPham = productDetail.Images.Select(img => new ProductImageDTO
                    {
                        Id = img.MaAnhSanPham,
                        LinkAnh = img.LinkAnh ?? ""
                    }).ToList(),
                    KichCo = uniqueSizes,
                    MauSac = uniqueColors
                };

                return ApiResponseFactory.Success(finalDto);
            }
            catch (Exception ex)
            {
                // Sử dụng logger thay vì Console.WriteLine
                 //_logger.LogError(ex, "Error in GetProductByIdOptimized for ID {ProductId}", id);
                Console.WriteLine($"Error in GetProductById: {ex.Message}"); // Tạm thời giữ lại
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return ApiResponseFactory.Error("Có lỗi xảy ra khi lấy thông tin sản phẩm", 500);
            }
        }

        public class UploadImageDto
        {
            public IFormFile file { get; set; }
        }


        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage([FromForm] UploadImageDto file)
        {
            if (file == null || file.file.Length == 0)
                return ApiResponseFactory.Error("No file uploaded", 400);

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.file.FileName)}";
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.file.CopyToAsync(stream);
            }
            string imageUrl = $"/uploads/{fileName}";

            return ApiResponseFactory.Success(new { imageUrl });
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductTextDto dto)
        {
            // 1. Tìm hoặc tạo danh mục
            var category = await _context.DanhMucs.FirstOrDefaultAsync(d => d.TenDanhMuc == dto.danhMuc);
            if (category == null)
            {
                category = new DanhMuc { TenDanhMuc = dto.danhMuc };
                _context.DanhMucs.Add(category);
                await _context.SaveChangesAsync();
            }

            // 2. Tạo sản phẩm mới
            var product = new SanPham
            {
                TenSanPham = dto.tenSanPham,
                GiaTien = decimal.TryParse(dto.giaTien, out var g) ? g : 0,
                MoTaSanPham = dto.moTa,
                MaDanhMuc = category.MaDanhMuc,
                AnhDaiDien = dto.imageUrl
            };
            _context.SanPhams.Add(product);
            await _context.SaveChangesAsync();

            // 3. Thêm biến thể
            foreach (var v in dto.variants)
            {
                var size = await _context.KichCos.FirstOrDefaultAsync(s => s.TenKichCo == v.size);
                if (size == null)
                {
                    size = new KichCo { TenKichCo = v.size };
                    _context.KichCos.Add(size);
                    await _context.SaveChangesAsync();
                }
                var color = await _context.MauSacs.FirstOrDefaultAsync(m => m.TenMau == v.color);
                if (color == null)
                {
                    color = new MauSac { TenMau = v.color };
                    _context.MauSacs.Add(color);
                    await _context.SaveChangesAsync();
                }
                var variant = new BienTheSanPham
                {
                    MaSanPham = product.MaSanPham,
                    MaKichCo = size.MaKichCo,
                    MaMau = color.MaMau,
                    Sku = v.sku,
                    SoLuongTon = int.TryParse(v.stock, out var stock) ? stock : 0,
                };
                _context.BienTheSanPhams.Add(variant);
            }
            await _context.SaveChangesAsync();

            return ApiResponseFactory.Success(new
            {
                message = "Thêm sản phẩm thành công",
                productId = product.MaSanPham,
                imageUrl = product.AnhDaiDien
            });
        }

        // DTO cho variants
        public class VariantDto
        {
            public string size { get; set; }
            public string color { get; set; }
            public string sku { get; set; }
            public string stock { get; set; }
            public string price { get; set; }
        }

        public class CreateProductTextDto
        {
            public string tenSanPham { get; set; }
            public string giaTien { get; set; }
            public string moTa { get; set; }
            public string danhMuc { get; set; }
            public string imageUrl { get; set; } // link ảnh đã upload
            public List<VariantDto> variants { get; set; }
        }
    }
}
