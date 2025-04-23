namespace BTL_ClothingShop.DTOs
{
    public class ProductListItemDTO
    {
        public string Id { get; set; } = "";
        public string TenSanPham { get; set; } = "";
        public decimal GiaTien { get; set; }
        public string AnhDaiDien { get; set; } = "";
        public int LuotBan { get; set; }
    }

    public class ProductDetailDTO
    {
        public string Id { get; set; } = "";
        public string TenSanPham { get; set; } = "";
        public decimal GiaTien { get; set; }
        public string MoTaSanPham { get; set; } = "";
        public string TenDanhMuc { get; set; } = "";
        public int SoLuongTon { get; set; }
        public int LuotBan { get; set; }
        public List<ProductImageDTO> AnhSanPham { get; set; } = new List<ProductImageDTO>();
        public List<SizeDTO> KichCo { get; set; } = new List<SizeDTO>();
        public List<ColorDTO> MauSac { get; set; } = new List<ColorDTO>();
    }

    public class ProductImageDTO
    {
        public string Id { get; set; } = "";
        public string LinkAnh { get; set; } = "";
    }

    public class SizeDTO
    {
        public string Id { get; set; } = "";
        public string KichCo { get; set; } = "";
    }

    public class ColorDTO
    {
        public string Id { get; set; } = "";
        public string Mau { get; set; } = "";
    }

    public class ProductFilterDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "TenSanPham";
        public string SortDirection { get; set; } = "asc";
        public string TenDanhMuc { get; set; } = "";
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string Search { get; set; } = "";
    }
}