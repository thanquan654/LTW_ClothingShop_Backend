using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BTL_ClothingShop.Models;

public partial class CsdlshopThoiTrangContext : DbContext
{
    public CsdlshopThoiTrangContext()
    {
    }

    public CsdlshopThoiTrangContext(DbContextOptions<CsdlshopThoiTrangContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AnhSanPham> AnhSanPhams { get; set; }

    public virtual DbSet<BienTheSanPham> BienTheSanPhams { get; set; }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

    public virtual DbSet<DanhMuc> DanhMucs { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<GioHang> GioHangs { get; set; }

    public virtual DbSet<KichCo> KichCos { get; set; }

    public virtual DbSet<MauSac> MauSacs { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Sử dụng tên cấu hình (Name=...) như Microsoft khuyến nghị
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnhSanPham>(entity =>
        {
            entity.HasKey(e => e.MaAnhSanPham).HasName("PK__AnhSanPh__173BC950E65D7B62");

            entity.ToTable("AnhSanPham");

            entity.Property(e => e.MaAnhSanPham)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("maAnhSanPham");
            entity.Property(e => e.LinkAnh)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("linkAnh");
            entity.Property(e => e.MaSanPham).HasColumnName("maSanPham");

            entity.HasOne(d => d.MaSanPhamNavigation).WithMany(p => p.AnhSanPhams)
                .HasForeignKey(d => d.MaSanPham)
                .HasConstraintName("FK__AnhSanPha__maSan__5EBF139D");
        });

        modelBuilder.Entity<BienTheSanPham>(entity =>
        {
            entity.HasKey(e => e.MaBienThe).HasName("PK__BienTheS__CFD6B0B9865A877C");

            entity.ToTable("BienTheSanPham");

            entity.HasIndex(e => e.Sku, "UQ__BienTheS__DDDF4BE77769F19A").IsUnique();

            entity.Property(e => e.MaBienThe).HasColumnName("maBienThe");
            entity.Property(e => e.LuotBan)
                .HasDefaultValue(0)
                .HasColumnName("luotBan");
            entity.Property(e => e.MaKichCo).HasColumnName("maKichCo");
            entity.Property(e => e.MaMau).HasColumnName("maMau");
            entity.Property(e => e.MaSanPham).HasColumnName("maSanPham");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("sku");
            entity.Property(e => e.SoLuongTon)
                .HasDefaultValue(0)
                .HasColumnName("soLuongTon");

            entity.HasOne(d => d.MaKichCoNavigation).WithMany(p => p.BienTheSanPhams)
                .HasForeignKey(d => d.MaKichCo)
                .HasConstraintName("FK__BienTheSa__maKic__5AEE82B9");

            entity.HasOne(d => d.MaMauNavigation).WithMany(p => p.BienTheSanPhams)
                .HasForeignKey(d => d.MaMau)
                .HasConstraintName("FK__BienTheSa__maMau__5BE2A6F2");

            entity.HasOne(d => d.MaSanPhamNavigation).WithMany(p => p.BienTheSanPhams)
                .HasForeignKey(d => d.MaSanPham)
                .HasConstraintName("FK__BienTheSa__maSan__59FA5E80");
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => e.MaChiTietDonHang).HasName("PK__ChiTietD__F581B90569A02CCF");

            entity.ToTable("ChiTietDonHang");

            entity.Property(e => e.MaChiTietDonHang).HasColumnName("maChiTietDonHang");
            entity.Property(e => e.MaBienThe).HasColumnName("maBienThe");
            entity.Property(e => e.MaDonHang)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("maDonHang");
            entity.Property(e => e.SoLuong).HasColumnName("soLuong");

            entity.HasOne(d => d.MaBienTheNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaBienThe)
                .HasConstraintName("FK__ChiTietDo__maBie__656C112C");

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaDonHang)
                .HasConstraintName("FK__ChiTietDo__maDon__6477ECF3");
        });

        modelBuilder.Entity<ChiTietGioHang>(entity =>
        {
            entity.HasKey(e => e.MaChiTietGioHang).HasName("PK__ChiTietG__69B06FC9AF6B8C98");

            entity.ToTable("ChiTietGioHang");

            entity.Property(e => e.MaChiTietGioHang).HasColumnName("maChiTietGioHang");
            entity.Property(e => e.MaBienThe).HasColumnName("maBienThe");
            entity.Property(e => e.MaGioHang)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("maGioHang");
            entity.Property(e => e.SoLuong).HasColumnName("soLuong");

            entity.HasOne(d => d.MaBienTheNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaBienThe)
                .HasConstraintName("FK__ChiTietGi__maBie__6C190EBB");

            entity.HasOne(d => d.MaGioHangNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaGioHang)
                .HasConstraintName("FK__ChiTietGi__maGio__6B24EA82");
        });

        modelBuilder.Entity<DanhMuc>(entity =>
        {
            entity.HasKey(e => e.MaDanhMuc).HasName("PK__DanhMuc__6B0F914CB08FD41A");

            entity.ToTable("DanhMuc");

            entity.HasIndex(e => e.TenDanhMuc, "UQ__DanhMuc__ED84A4B12D9557E9").IsUnique();

            entity.Property(e => e.MaDanhMuc)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("maDanhMuc");
            entity.Property(e => e.TenDanhMuc)
                .HasMaxLength(100)
                .HasColumnName("tenDanhMuc");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHang__871D3819DE84AEC7");

            entity.ToTable("DonHang");

            entity.Property(e => e.MaDonHang)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("maDonHang");
            entity.Property(e => e.DiaChi)
                .HasMaxLength(255)
                .HasColumnName("diaChi");
            entity.Property(e => e.MaUser)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("maUser");
            entity.Property(e => e.NgayDatHang)
                .HasColumnType("datetime")
                .HasColumnName("ngayDatHang");
            entity.Property(e => e.PhuongThucThanhToan)
                .HasMaxLength(50)
                .HasColumnName("phuongThucThanhToan");
            entity.Property(e => e.TongTien)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("tongTien");
            entity.Property(e => e.TrangThaiDonHang)
                .HasMaxLength(50)
                .HasColumnName("trangThaiDonHang");

            entity.HasOne(d => d.User).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaUser)
                .HasConstraintName("FK__DonHang__maUser__619B8048");
        });

        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => e.MaGioHang).HasName("PK__GioHang__2C76D20310061826");

            entity.ToTable("GioHang");

            entity.Property(e => e.MaGioHang)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("maGioHang");
            entity.Property(e => e.MaUser)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("maUser");

            entity.HasOne(d => d.MaUserNavigation).WithMany(p => p.GioHangs)
                .HasForeignKey(d => d.MaUser)
                .HasConstraintName("FK__GioHang__maUser__68487DD7");
        });

        modelBuilder.Entity<KichCo>(entity =>
        {
            entity.HasKey(e => e.MaKichCo).HasName("PK__KichCo__9941EF6471A5DF57");

            entity.ToTable("KichCo");

            entity.HasIndex(e => e.TenKichCo, "UQ__KichCo__8D9AB22949677EC3").IsUnique();

            entity.Property(e => e.MaKichCo).HasColumnName("maKichCo");
            entity.Property(e => e.TenKichCo)
                .HasMaxLength(10)
                .HasColumnName("tenKichCo");
        });

        modelBuilder.Entity<MauSac>(entity =>
        {
            entity.HasKey(e => e.MaMau).HasName("PK__MauSac__27572EAEB1C48790");

            entity.ToTable("MauSac");

            entity.HasIndex(e => e.TenMau, "UQ__MauSac__A88AB9EE3A033A72").IsUnique();

            entity.Property(e => e.MaMau).HasColumnName("maMau");
            entity.Property(e => e.TenMau)
                .HasMaxLength(50)
                .HasColumnName("tenMau");
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.MaSanPham).HasName("PK__SanPham__5B439C4339468C64");

            entity.ToTable("SanPham");

            entity.Property(e => e.MaSanPham).HasColumnName("maSanPham");
            entity.Property(e => e.AnhDaiDien)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("anhDaiDien");
            entity.Property(e => e.GiaTien)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("giaTien");
            entity.Property(e => e.MaDanhMuc)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("maDanhMuc");
            entity.Property(e => e.MoTaSanPham).HasColumnName("moTaSanPham");
            entity.Property(e => e.TenSanPham)
                .HasMaxLength(255)
                .HasColumnName("tenSanPham");

            entity.HasOne(d => d.MaDanhMucNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.MaDanhMuc)
                .HasConstraintName("FK__SanPham__maDanhM__4E88ABD4");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.MaUser).HasName("PK__Users__18B21FF196E53283");

            entity.Property(e => e.MaUser)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("maUser");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.HoVaTen)
                .HasMaxLength(100)
                .HasColumnName("hoVaTen");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("matKhau");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("soDienThoai");
            entity.Property(e => e.VaiTro)
                .HasMaxLength(50)
                .HasColumnName("vaiTro");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
