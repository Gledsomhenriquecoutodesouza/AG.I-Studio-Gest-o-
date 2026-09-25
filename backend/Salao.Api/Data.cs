using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
namespace Salao.Api;
public class SalonDb(DbContextOptions<SalonDb> options) : DbContext(options) {
 public DbSet<Client> Clients => Set<Client>(); public DbSet<CatalogService> Services => Set<CatalogService>(); public DbSet<Product> Products => Set<Product>(); public DbSet<Appointment> Appointments => Set<Appointment>(); public DbSet<AppointmentItem> AppointmentItems => Set<AppointmentItem>(); public DbSet<Receivable> Receivables => Set<Receivable>();
 protected override void OnModelCreating(ModelBuilder b) {
  b.Entity<Client>(e=>{e.ToTable("clients");e.HasKey(x=>x.Id);e.Property(x=>x.Id).UseIdentityByDefaultColumn();e.Property(x=>x.Name).HasMaxLength(160).IsRequired();});
  b.Entity<CatalogService>(e=>{e.ToTable("services");e.HasKey(x=>x.Id);e.Property(x=>x.Id).UseIdentityByDefaultColumn();e.Property(x=>x.Price).HasPrecision(12,2);e.Property(x=>x.Name).HasMaxLength(180);});
  b.Entity<Product>(e=>{e.ToTable("products");e.HasKey(x=>x.Id);e.Property(x=>x.Id).UseIdentityByDefaultColumn();e.Property(x=>x.Price).HasPrecision(12,2);});
  b.Entity<Appointment>(e=>{e.ToTable("appointments");e.HasKey(x=>x.Id);e.Property(x=>x.Id).UseIdentityByDefaultColumn();e.HasOne(x=>x.Client).WithMany().HasForeignKey(x=>x.ClientId);});
  b.Entity<AppointmentItem>(e=>{e.ToTable("appointment_items");e.HasKey(x=>x.Id);e.Property(x=>x.Id).UseIdentityByDefaultColumn();e.Property(x=>x.UnitPrice).HasPrecision(12,2);});
  b.Entity<Receivable>(e=>{e.ToTable("receivables");e.HasKey(x=>x.Id);e.Property(x=>x.Id).UseIdentityByDefaultColumn();e.Property(x=>x.Amount).HasPrecision(12,2);e.Property(x=>x.PaymentMethod).HasMaxLength(20);e.HasOne(x=>x.Appointment).WithMany(x=>x.Receivables).HasForeignKey(x=>x.AppointmentId);});
 }
}
