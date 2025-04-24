using System.Linq;
using BTL_ClothingShop.DTOs;
using BTL_ClothingShop.Helpers;
using BTL_ClothingShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_ClothingShop.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly CsdlshopThoiTrangContext _context = new CsdlshopThoiTrangContext();

        // POST: api/orders
        // This endpoint is used to create a new order.
        [HttpPost("")] // Hoặc [HttpPost("/")] nếu bạn cấu hình route base khác
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto createOrderBody)
        {
            // --- Validation cơ bản ---
            if (!ModelState.IsValid)
            {
                return ApiResponseFactory.Error(string.Join("; "), 400);
            }

            // --- Kiểm tra User tồn tại ---
            // Sử dụng AnyAsync hiệu quả hơn FindAsync nếu chỉ cần kiểm tra tồn tại
            var userExists = await _context.Users.AnyAsync(u => u.MaUser == createOrderBody.MaUser);
            if (!userExists)
            {
                // Sử dụng ApiResponseFactory của bạn
                return ApiResponseFactory.Error($"User with id '{createOrderBody.MaUser}' not found.", 400); // 400 Bad Request hợp lý hơn 404 ở đây
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal tongTien = 0;
                var chiTietDonHangs = new List<ChiTietDonHang>();
                var bienTheUpdates = new List<BienTheSanPham>(); // List để theo dõi các biến thể cần cập nhật

                // --- Xử lý từng item trong đơn hàng ---
                foreach (var item in createOrderBody.Items)
                {
                    // Tìm Biến Thể Sản Phẩm dựa trên MaSanPham, MaMau, MaKichCo
                    var bienThe = await _context.BienTheSanPhams
                        .Include(bt => bt.MaSanPhamNavigation) // Include SanPham để lấy giá (đảm bảo tên Navigation đúng)
                        .FirstOrDefaultAsync(bt =>
                            bt.MaSanPham == item.MaSanPham &&
                            bt.MaMau == item.MaMau &&
                            bt.MaKichCo == item.MaKichCo);

                    if (bienThe == null)
                    {
                        await transaction.RollbackAsync(); // Quan trọng: Rollback trước khi return lỗi
                        return ApiResponseFactory.Error($"Product variant not found for Product ID: {item.MaSanPham}, Color ID: {item.MaMau}, Size ID: {item.MaKichCo}", 400);
                    }

                    // Kiểm tra số lượng tồn kho
                    if (bienThe.SoLuongTon < item.SoLuong)
                    {
                        await transaction.RollbackAsync();
                        return ApiResponseFactory.Error($"Insufficient stock for product variant (SKU: {bienThe.Sku ?? "N/A"}). Available: {bienThe.SoLuongTon}, Requested: {item.SoLuong}", 400);
                    }

                    // Kiểm tra xem có lấy được giá không
                    if (bienThe.MaSanPhamNavigation == null)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Error: Could not load SanPham navigation property for BienTheSanPham ID {bienThe.MaBienThe}"); // Log lỗi
                        return ApiResponseFactory.Error("Could not retrieve product price information.", 500); // Lỗi server vì không load được data cần thiết
                    }
                    var giaTien = bienThe.MaSanPhamNavigation.GiaTien ?? 0; // Lấy giá từ SanPham
                    if (giaTien <= 0)
                    {
                        // Cân nhắc xem có cho phép đặt hàng sản phẩm giá 0 hoặc âm không
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Warning: Attempted to order product with zero or negative price. Product ID: {bienThe.MaSanPham}");
                        return ApiResponseFactory.Error($"Product price is invalid for Product ID: {bienThe.MaSanPham}.", 400);
                    }

                    // Tính tổng tiền
                    tongTien += giaTien * item.SoLuong;

                    // Cập nhật số lượng tồn và lượt bán cho biến thể
                    bienThe.SoLuongTon -= item.SoLuong;
                    bienThe.LuotBan = (bienThe.LuotBan ?? 0) + item.SoLuong; // Giả sử LuotBan có thể null
                                                                             // Không cần gọi _context.Update(bienThe) vì EF Core tự động theo dõi thay đổi của đối tượng đã tải

                    // Thêm vào danh sách biến thể cần cập nhật (nếu bạn muốn UpdateRange sau)
                    // Hoặc cứ để EF Core tự theo dõi từng cái cũng được

                    // Tạo đối tượng ChiTietDonHang
                    chiTietDonHangs.Add(new ChiTietDonHang
                    {
                        // MaChiTietDonHang tự tăng
                        MaBienThe = bienThe.MaBienThe, // Lấy MaBienThe từ biến thể tìm được
                        SoLuong = item.SoLuong
                    });
                }

                // --- Tạo đơn hàng mới ---
                var donHang = new DonHang
                {
                    // Tạo mã đơn hàng phù hợp VARCHAR(50), ví dụ:
                    MaDonHang = $"DH{DateTime.UtcNow:yyyyMMddHHmmssfff}{new Random().Next(100, 999)}",
                    MaUser = createOrderBody.MaUser,
                    PhuongThucThanhToan = createOrderBody.PhuongThucThanhToan,
                    TongTien = tongTien, // *** GÁN TỔNG TIỀN VÀO ĐƠN HÀNG ***
                    NgayDatHang = DateTime.UtcNow, // Sử dụng UTC
                    TrangThaiDonHang = "Chờ xác nhận", // Trạng thái ban đầu
                    DiaChi = createOrderBody.DiaChi,
                    ChiTietDonHangs = chiTietDonHangs // Gán danh sách chi tiết
                };

                // Thêm đơn hàng vào context (bao gồm cả chi tiết)
                await _context.DonHangs.AddAsync(donHang); // Sử dụng bảng DonHangs (hoặc tên DbSet của bạn)

                // Lưu tất cả thay đổi (DonHang, ChiTietDonHang, cập nhật BienTheSanPham)
                await _context.SaveChangesAsync();

                // Commit transaction nếu mọi thứ thành công
                await transaction.CommitAsync();

                // --- Tạo DTO trả về ---
                var orderSummary = new OrderSummaryDto
                {
                    MaDonHang = donHang.MaDonHang,
                    MaUser = donHang.MaUser, // Lấy từ donHang đã lưu
                    TongTien = (decimal)donHang.TongTien, // Lấy từ donHang đã lưu
                    NgayDatHang = (DateTime)donHang.NgayDatHang, // Lấy từ donHang đã lưu
                    TrangThaiDonHang = donHang.TrangThaiDonHang, // Lấy từ donHang đã lưu
                    DiaChi = donHang.DiaChi // Lấy từ donHang đã lưu
                };

                return Ok(ApiResponseFactory.Success(orderSummary)); // Giả sử Success trả về cấu trúc chuẩn

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error creating order: {ex.ToString()}");
                return ApiResponseFactory.Error($"An unexpected error occurred while creating the order. {ex.Message}", 500);
            }
        }


        // GET: api/orders?userId=...
        // This endpoint is used to get a list of orders for a specific user.
        [HttpGet("")]
        public async Task<IActionResult> GetOrders([FromQuery] string? userId)
        {
            try
            {
                var query = _context.DonHangs.AsQueryable();

                if (!string.IsNullOrEmpty(userId))
                {
                    // Kiểm tra User tồn tại (tùy chọn, nhưng nên làm)
                    var userExists = await _context.Users.AnyAsync(u => u.MaUser == userId);
                    if (!userExists && !string.IsNullOrEmpty(userId)) // Chỉ báo lỗi nếu userId được cung cấp và không tồn tại
                    {
                         return Ok(new List<OrderSummaryDto>());
                    }
                    query = query.Where(d => d.MaUser == userId);
                }
                // Nếu không có userId, lấy tất cả đơn hàng (cho admin hoặc tình huống không cần lọc)

                var orders = await query
                    .OrderByDescending(d => d.NgayDatHang) // Sắp xếp theo ngày đặt mới nhất
                    .Select(d => new OrderSummaryDto // Sử dụng DTO tóm tắt
                    {
                        MaDonHang = d.MaDonHang,
                        MaUser = d.User.HoVaTen,
                        TongTien = (decimal)d.TongTien,
                        NgayDatHang = (DateTime)d.NgayDatHang,
                        TrangThaiDonHang = d.TrangThaiDonHang,
                        DiaChi = d.DiaChi
                    })
                    .ToListAsync();

                if (orders == null || !orders.Any())
                {
                    return Ok(new List<OrderSummaryDto>()); // Trả về danh sách rỗng nếu không có đơn hàng nào
                }

                return Ok(orders);
            }
            catch (Exception ex)
            {
                // Log lỗi ex
                return StatusCode(500, "An error occurred while retrieving orders.");
            }
        }

        // GET: api/orders/{orderId}?userId=...
        // This endpoint is used to get the details of a specific order.
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(string orderId)
        {
            try
            {
                var order = await _context.DonHangs
                    .Include(d => d.User)
                    .Include(d => d.ChiTietDonHangs)
                        .ThenInclude(ct => ct.MaBienTheNavigation)
                            .ThenInclude(bt => bt.MaSanPhamNavigation)
                    .Include(d => d.ChiTietDonHangs)
                        .ThenInclude(ct => ct.MaBienTheNavigation)
                            .ThenInclude(bt => bt.MaKichCoNavigation)
                    .Include(d => d.ChiTietDonHangs)
                        .ThenInclude(ct => ct.MaBienTheNavigation)
                            .ThenInclude(bt => bt.MaMauNavigation)
                    .FirstOrDefaultAsync(d => d.MaDonHang == orderId);

                if (order == null)
                {
                    return ApiResponseFactory.Error("Order not found", 404);
                }

                decimal tongTien = 0;
                var orderDetail = new OrderDetailDto
                {
                    MaDonHang = order.MaDonHang,
                    MaUser = order.MaUser ?? "",
                    NgayDatHang = order.NgayDatHang ?? DateTime.Now,
                    TrangThaiDonHang = order.TrangThaiDonHang ?? "",
                    DiaChi = order.DiaChi ?? "",
                    PhuongThucThanhToan = order.PhuongThucThanhToan ?? "",
                    NguoiDat = new UserInfoDto
                    {
                        MaUser = order.User?.MaUser ?? "",
                        HoVaTen = order.User?.HoVaTen ?? "",
                        Email = order.User?.Email ?? "",
                        SoDienThoai = order.User?.SoDienThoai ?? ""
                    },
                    ChiTietDonHang = order.ChiTietDonHangs.Select(ct =>
                    {
                        var sanPham = ct.MaBienTheNavigation?.MaSanPhamNavigation;
                        var giaTien = sanPham?.GiaTien ?? 0;
                        tongTien += giaTien * (ct.SoLuong ?? 0);

                        return new OrderDetailItemDto
                        {
                            MaChiTietDonHang = ct.MaChiTietDonHang,
                            MaBienThe = ct.MaBienThe ?? 0,
                            SoLuong = ct.SoLuong ?? 0,
                            Sku = ct.MaBienTheNavigation?.Sku ?? "",
                            TenSanPham = sanPham?.TenSanPham ?? "",
                            TenMau = ct.MaBienTheNavigation?.MaMauNavigation?.TenMau ?? "",
                            TenKichCo = ct.MaBienTheNavigation?.MaKichCoNavigation?.TenKichCo ?? "",
                            GiaLucDat = giaTien,
                            AnhDaiDienSanPham = sanPham?.AnhDaiDien ?? ""
                        };
                    }).ToList()
                };

                orderDetail.TongTien = tongTien;
                return ApiResponseFactory.Success(orderDetail);
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Error($"Error retrieving order: {ex.Message}", 500);
            }
        }

        // PUT: api/orders/{orderId}/cancel
        // This endpoint is used to cancel a specific order.
        [HttpPut("{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder([FromRoute] string orderId /*, [FromBody] object reqBody - Bỏ đi nếu không cần*/)
        {
            // --- Tìm đơn hàng ---
            var order = await _context.DonHangs
                                    .Include(d => d.ChiTietDonHangs) // Cần chi tiết để hoàn trả số lượng
                                    .FirstOrDefaultAsync(d => d.MaDonHang == orderId);

            if (order == null)
            {
                return NotFound($"Order with id '{orderId}' not found.");
            }

            // --- Kiểm tra trạng thái có cho phép hủy không ---
            // Ví dụ: Chỉ cho hủy khi đang ở trạng thái "Đang xử lý" hoặc "Chờ xác nhận"
            var cancellableStatuses = new List<string> { "Đang xử lý", "Chờ xác nhận" }; // Định nghĩa các trạng thái có thể hủy
            if (!cancellableStatuses.Contains(order.TrangThaiDonHang))
            {
                return BadRequest($"Order cannot be cancelled because its current status is '{order.TrangThaiDonHang}'.");
            }

            // --- Xử lý logic hủy đơn hàng ---
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Cập nhật trạng thái đơn hàng
                order.TrangThaiDonHang = "Đã hủy";

                // Hoàn trả số lượng tồn kho cho các biến thể sản phẩm
                if (order.TrangThaiDonHang != null && order.ChiTietDonHangs.Any())
                {
                    var bienTheIds = order.ChiTietDonHangs.Select(cd => cd.MaBienThe).ToList();
                    var bienThesToUpdate = await _context.BienTheSanPhams
                                                        .Where(bt => bienTheIds.Contains(bt.MaBienThe))
                                                        .ToListAsync();

                    foreach (var detail in order.ChiTietDonHangs)
                    {
                        var bienThe = bienThesToUpdate.FirstOrDefault(bt => bt.MaBienThe == detail.MaBienThe);
                        if (bienThe != null)
                        {
                            bienThe.SoLuongTon += detail.SoLuong; // Cộng lại số lượng
                            bienThe.LuotBan -= detail.SoLuong;   // Giảm lượt bán (nếu cần)
                            if (bienThe.LuotBan < 0) bienThe.LuotBan = 0; // Đảm bảo không âm
                        }
                        else
                        {
                            // Log warning: Không tìm thấy biến thể để hoàn trả số lượng cho chi tiết đơn hàng này
                            // Điều này không nên xảy ra nếu dữ liệu toàn vẹn
                        }
                    }
                    _context.BienTheSanPhams.UpdateRange(bienThesToUpdate); // Cập nhật các biến thể
                }

                _context.DonHangs.Update(order); // Đánh dấu đơn hàng cần update
                await _context.SaveChangesAsync(); // Lưu thay đổi (trạng thái đơn hàng, số lượng biến thể)
                await transaction.CommitAsync();

                return Ok($"Order '{orderId}' cancelled successfully."); // Hoặc trả về 204 No Content
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log lỗi ex
                return StatusCode(500, "An error occurred while cancelling the order.");
            }
        }

        // PUT: api/orders/{orderId}/change-status
        // This endpoint is used to change the status of a specific order.
        [HttpPut("{orderId}/change-status")]
        public async Task<IActionResult> ChangeOrderStatus([FromRoute] string orderId, [FromBody] ChangeOrderStatusRequestDto reqBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // --- Tìm đơn hàng ---
            var order = await _context.DonHangs.FirstOrDefaultAsync(d => d.MaDonHang == orderId);

            if (order == null)
            {
                return NotFound($"Order with id '{orderId}' not found.");
            }

            // --- (Tùy chọn) Validate trạng thái mới ---
            // Ví dụ: Kiểm tra xem trạng thái mới có hợp lệ không   



            // --- Cập nhật trạng thái ---
            try
            {
                order.TrangThaiDonHang = reqBody.NewStatus;
                _context.DonHangs.Update(order);
                await _context.SaveChangesAsync();

                return Ok($"Status of order '{orderId}' changed to '{reqBody.NewStatus}'."); // Hoặc 204 No Content
            }
            catch (Exception ex)
            {
                // Log lỗi ex
                return StatusCode(500, "An error occurred while changing the order status.");
            }
        }


        // --- Helper Method để lấy chi tiết đơn hàng (tránh lặp code) ---
        private async Task<OrderDetailDto?> GetOrderDetailsDto(string orderId, string? userId = null)
        {
            var query = _context.DonHangs.AsQueryable();

            // Lọc theo user ID nếu được cung cấp
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(d => d.MaUser == userId);
            }

            var order = await query
               .Where(d => d.MaDonHang == orderId)
               .Include(d => d.User) // Include thông tin người dùng
               .Include(d => d.ChiTietDonHangs)
                   .ThenInclude(cd => cd.MaBienTheNavigation) // Include Biến thể từ Chi tiết
                       .ThenInclude(bt => bt.MaSanPhamNavigation) // Include Sản phẩm từ Biến thể
                .Include(d => d.ChiTietDonHangs)
                   .ThenInclude(cd => cd.MaBienTheNavigation)
                       .ThenInclude(bt => bt.MaMau) // Include Màu sắc từ Biến thể
                .Include(d => d.ChiTietDonHangs)
                   .ThenInclude(cd => cd.MaBienTheNavigation)
                       .ThenInclude(bt => bt.MaKichCo) // Include Kích cỡ từ Biến thể
               .Select(d => new OrderDetailDto // Map sang DTO chi tiết
               {
                   // Thông tin cơ bản từ OrderSummaryDto
                   MaDonHang = d.MaDonHang,
                   MaUser = d.MaUser,
                   TongTien = (decimal)d.TongTien,
                   NgayDatHang = (DateTime)d.NgayDatHang,
                   TrangThaiDonHang = d.TrangThaiDonHang,
                   DiaChi = d.DiaChi,
                   // Thông tin chi tiết hơn
                   PhuongThucThanhToan = d.PhuongThucThanhToan,
                   NguoiDat = d.User == null ? null : new UserInfoDto // Kiểm tra null trước khi truy cập User
                   {
                       MaUser = d.User.MaUser,
                       HoVaTen = d.User.HoVaTen,
                       Email = d.User.Email,
                       SoDienThoai = d.User.SoDienThoai
                   },
                   ChiTietDonHang = d.ChiTietDonHangs.Select(cd => new OrderDetailItemDto
                   {
                       MaChiTietDonHang = cd.MaChiTietDonHang,
                       MaBienThe = (int)cd.MaBienThe,
                       SoLuong = (int)cd.SoLuong,
                       // Lấy thông tin từ các bảng liên quan (đã Include)
                       Sku = cd.MaBienTheNavigation == null ? "N/A" : cd.MaBienTheNavigation.Sku,
                       TenSanPham = cd.MaBienTheNavigation == null || cd.MaBienTheNavigation.MaSanPhamNavigation == null ? "N/A" : cd.MaBienTheNavigation.MaSanPhamNavigation.TenSanPham,
                       TenMau = cd.MaBienTheNavigation == null || cd.MaBienTheNavigation.MaMau == null ? "N/A" : cd.MaBienTheNavigation.MaMauNavigation.TenMau,
                       TenKichCo = cd.MaBienTheNavigation == null || cd.MaBienTheNavigation.MaKichCoNavigation == null ? "N/A" : cd.MaBienTheNavigation.MaKichCoNavigation.TenKichCo,
                       AnhDaiDienSanPham = cd.MaBienTheNavigation == null || cd.MaBienTheNavigation.MaSanPhamNavigation == null ? null : cd.MaBienTheNavigation.MaSanPhamNavigation.AnhDaiDien, // Lấy ảnh đại diện
                                                                                                                                                         // Lấy giá từ SanPham (giá hiện tại). Nên có trường GiaLucDat trong ChiTietDonHang
                       GiaLucDat = (decimal)(cd.MaBienTheNavigation == null || cd.MaBienTheNavigation.MaSanPhamNavigation == null ? 0 : cd.MaBienTheNavigation.MaSanPhamNavigation.GiaTien)
                   }).ToList()
               })
               .FirstOrDefaultAsync();

            return order; // Trả về DTO hoặc null nếu không tìm thấy
        }

    }
}
