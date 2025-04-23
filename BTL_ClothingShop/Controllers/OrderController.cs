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
        [HttpPost("")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto createOrderBody)
        {
            // --- Validation cơ bản ---
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Trả về lỗi validation nếu DTO không hợp lệ
            }

            // Kiểm tra User tồn tại
            var userExists = await _context.Users.AnyAsync(u => u.MaUser == createOrderBody.MaUser);
            if (!userExists)
            {
                return BadRequest($"User with id '{createOrderBody.MaUser}' not found.");
            }

            // --- Xử lý logic tạo đơn hàng ---
            using var transaction = await _context.Database.BeginTransactionAsync(); // Sử dụng Transaction
            try
            {
                decimal tongTien = 0;
                var chiTietDonHangs = new List<ChiTietDonHang>();
                var bienTheUpdates = new List<BienTheSanPham>(); // Lưu các biến thể cần cập nhật SL

                foreach (var item in createOrderBody.Items)
                {
                    // Tìm biến thể sản phẩm và sản phẩm tương ứng để lấy giá và kiểm tra tồn kho
                    var bienThe = await _context.BienTheSanPhams
                                                .Include(bt => bt.SanPham) // Include SanPham để lấy giá
                                                .FirstOrDefaultAsync(bt => bt.MaBienThe == item.MaBienThe);

                    if (bienThe == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest($"Product variant with id '{item.MaBienThe}' not found.");
                    }
                    if (bienThe.SoLuongTon < item.SoLuong)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest($"Not enough stock for product variant SKU '{bienThe.Sku}'. Available: {bienThe.SoLuongTon}, Requested: {item.SoLuong}");
                    }
                    if (bienThe.SanPham != null) // Kiểm tra nếu không include được SanPham
                    {
                        // Giảm số lượng tồn kho và tăng lượt bán
                        bienThe.SoLuongTon -= item.SoLuong;
                        bienThe.LuotBan += item.SoLuong;
                        bienTheUpdates.Add(bienThe); // Thêm vào danh sách cần update

                        // Tính tổng tiền (Lấy giá từ bảng SanPham)
                        tongTien += (decimal)(bienThe.SanPham.GiaTien * item.SoLuong);

                        // Tạo chi tiết đơn hàng
                        chiTietDonHangs.Add(new ChiTietDonHang
                        {
                            // maChiTietDonHang tự tăng
                            MaBienThe = item.MaBienThe,
                            SoLuong = item.SoLuong
                            // maDonHang sẽ được gán sau khi DonHang được tạo
                            // Cân nhắc thêm trường GiaLucDat vào ChiTietDonHang để lưu giá tại thời điểm mua
                        });
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        // Ghi log lỗi ở đây
                        return StatusCode(500, "Could not retrieve product details for variant calculation.");
                    }
                }

                // Tạo đơn hàng mới
                var newOrder = new DonHang
                {
                    MaDonHang = $"DH{DateTime.Now:yyyyMMddHHmmssfff}{new Random().Next(100, 999)}", // Tạo mã đơn hàng duy nhất
                    MaUser = createOrderBody.MaUser,
                    PhuongThucThanhToan = createOrderBody.PhuongThucThanhToan,
                    TongTien = tongTien,
                    NgayDatHang = DateTime.UtcNow, // Sử dụng UTC cho server
                    TrangThaiDonHang = "Đang xử lý", // Trạng thái mặc định
                    DiaChi = createOrderBody.DiaChi,
                    ChiTietDonHangs = chiTietDonHangs // Gán danh sách chi tiết (EF Core sẽ tự xử lý khóa ngoại)
                };

                _context.DonHangs.Add(newOrder);
                _context.BienTheSanPhams.UpdateRange(bienTheUpdates); // Cập nhật nhiều biến thể cùng lúc

                await _context.SaveChangesAsync(); // Lưu tất cả thay đổi (Đơn hàng, Chi tiết, Biến thể)
                await transaction.CommitAsync(); // Hoàn tất transaction thành công

                
                // Trả về mã đơn hàng
                return CreatedAtAction(nameof(GetOrderDetails), new { orderId = newOrder.MaDonHang }, new { maDonHang = newOrder.MaDonHang });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Hoàn tác nếu có lỗi
                                                   // Log lỗi ex chi tiết ở đây (quan trọng!)
                return StatusCode(500, "An error occurred while creating the order.");
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
                        MaUser = d.MaDonHang,
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
        public async Task<IActionResult> GetOrderDetails([FromRoute] string orderId, [FromQuery] string? userId)
        {
            try
            {
                var orderDetailDto = await GetOrderDetailsDto(orderId, userId); // Gọi hàm helper

                if (orderDetailDto == null)
                {
                    return NotFound($"Order with id '{orderId}' not found" + (userId != null ? $" for user '{userId}'." : "."));
                }

                return Ok(orderDetailDto);
            }
            catch (Exception ex)
            {
                // Log lỗi ex
                return StatusCode(500, "An error occurred while retrieving order details.");
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
            var validStatuses = new List<string> { "Đang xử lý", "Chờ xác nhận", "Đang giao hàng", "Đã giao hàng", "Hoàn thành", "Đã hủy", "Thất bại" }; // Danh sách trạng thái hợp lệ
            if (!validStatuses.Contains(reqBody.NewStatus))
            {
                return BadRequest($"Invalid status value: '{reqBody.NewStatus}'.");
            }



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
                   .ThenInclude(cd => cd.BienTheSanPham) // Include Biến thể từ Chi tiết
                       .ThenInclude(bt => bt.SanPham) // Include Sản phẩm từ Biến thể
                .Include(d => d.ChiTietDonHangs)
                   .ThenInclude(cd => cd.BienTheSanPham)
                       .ThenInclude(bt => bt.MaMau) // Include Màu sắc từ Biến thể
                .Include(d => d.ChiTietDonHangs)
                   .ThenInclude(cd => cd.BienTheSanPham)
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
                       Sku = cd.BienTheSanPham == null ? "N/A" : cd.BienTheSanPham.Sku,
                       TenSanPham = cd.BienTheSanPham == null || cd.BienTheSanPham.SanPham == null ? "N/A" : cd.BienTheSanPham.SanPham.TenSanPham,
                       TenMau = cd.BienTheSanPham == null || cd.BienTheSanPham.MaMau == null ? "N/A" : cd.BienTheSanPham.MaMauNavigation.TenMau,
                       TenKichCo = cd.BienTheSanPham == null || cd.BienTheSanPham.MaKichCoNavigation == null ? "N/A" : cd.BienTheSanPham.MaKichCoNavigation.TenKichCo,
                       AnhDaiDienSanPham = cd.BienTheSanPham == null || cd.BienTheSanPham.SanPham == null ? null : cd.BienTheSanPham.SanPham.AnhDaiDien, // Lấy ảnh đại diện
                                                                                                                                                         // Lấy giá từ SanPham (giá hiện tại). Nên có trường GiaLucDat trong ChiTietDonHang
                       GiaLucDat = (decimal)(cd.BienTheSanPham == null || cd.BienTheSanPham.SanPham == null ? 0 : cd.BienTheSanPham.SanPham.GiaTien)
                   }).ToList()
               })
               .FirstOrDefaultAsync();

            return order; // Trả về DTO hoặc null nếu không tìm thấy
        }

    }
}
