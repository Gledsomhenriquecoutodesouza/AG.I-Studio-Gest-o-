using Microsoft.EntityFrameworkCore;
using Salao.Api;
var builder = WebApplication.CreateBuilder(args);
var renderPort=Environment.GetEnvironmentVariable("PORT");
if(!string.IsNullOrWhiteSpace(renderPort))builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");
builder.Services.AddDbContext<SalonDb>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("Salon") ?? "Host=localhost;Port=5432;Database=salao;Username=salao;Password=salao"));
var allowedOrigins=builder.Configuration["Cors:AllowedOrigins"]?.Split(';',StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries)??new[]{"http://localhost:3000"};
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddControllers(); builder.Services.AddEndpointsApiExplorer(); builder.Services.AddSwaggerGen();
var app = builder.Build(); app.UseCors(); app.UseSwagger(); app.UseSwaggerUI(); app.MapControllers(); app.MapGet("/health",()=>Results.Ok(new{status="ok"}));
using (var scope = app.Services.CreateScope()) { var db=scope.ServiceProvider.GetRequiredService<SalonDb>(); await db.Database.EnsureCreatedAsync(); await db.Database.ExecuteSqlRawAsync("ALTER TABLE appointments ADD COLUMN IF NOT EXISTS \"BookingFeePaid\" boolean NOT NULL DEFAULT FALSE; ALTER TABLE receivables ADD COLUMN IF NOT EXISTS \"PaymentMethod\" character varying(20); ALTER TABLE receivables ADD COLUMN IF NOT EXISTS \"PaidAt\" timestamp with time zone;"); if(!await db.Services.AnyAsync()) { db.Services.AddRange(CatalogSeed.Items.Select(x=>new CatalogService { Category=x.Cat,Name=x.Name,Variant=x.Variant,HairLength=x.Length,Price=x.Price,PriceIsStartingAt=x.Starting })); await db.SaveChangesAsync(); } }
app.Run();
