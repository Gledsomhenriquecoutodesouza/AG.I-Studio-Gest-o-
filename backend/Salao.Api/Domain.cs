namespace Salao.Api;

public class Client { public long Id { get; set; } public string Name { get; set; } = ""; public string? Phone { get; set; } }
public class CatalogService { public long Id { get; set; } public string Category { get; set; } = ""; public string Name { get; set; } = ""; public string? Variant { get; set; } public string? HairLength { get; set; } public decimal Price { get; set; } public bool PriceIsStartingAt { get; set; } }
public class Product { public long Id { get; set; } public string Name { get; set; } = ""; public decimal Price { get; set; } public int Stock { get; set; } }
public class Appointment { public long Id { get; set; } public long ClientId { get; set; } public Client? Client { get; set; } public DateTime StartsAt { get; set; } public string Status { get; set; } = "Agendado"; public bool BookingFeePaid { get; set; } public List<AppointmentItem> Items { get; set; } = []; public List<Receivable> Receivables { get; set; } = []; }
public class AppointmentItem { public long Id { get; set; } public long AppointmentId { get; set; } public long? ServiceId { get; set; } public long? ProductId { get; set; } public string Description { get; set; } = ""; public decimal UnitPrice { get; set; } public int Quantity { get; set; } }
public class Receivable { public long Id { get; set; } public long AppointmentId { get; set; } public Appointment? Appointment { get; set; } public decimal Amount { get; set; } public int InstallmentNumber { get; set; } public DateOnly DueDate { get; set; } public bool Paid { get; set; } public string? PaymentMethod { get; set; } public DateTime? PaidAt { get; set; } }

public record CreateAppointment(long ClientId, DateTime StartsAt, List<SaleItemInput> Items, bool BookingFeePaid = false);
public record CheckoutInput(string Mode, string? PaymentMethod, List<DeferredPaymentInput>? Installments);
public record DeferredPaymentInput(decimal Amount, DateOnly DueDate, bool IsEntry = false);
public record MarkReceivablePaidInput(string PaymentMethod);
public record AdminAccessInput(string AccessCode);
public record UpdateProductInput(string Name, decimal Price, int Stock);
public record UpdateServicePriceInput(decimal Price);
public record CreateCatalogServiceInput(string Category, string Name, string? Variant, string? HairLength, decimal Price, bool PriceIsStartingAt = false);
public record SaleItemInput(long? ServiceId, long? ProductId, string? Description, decimal? UnitPrice, int Quantity = 1);

public static class CatalogSeed {
 public static readonly (string Cat,string Name,string? Variant,string? Length,decimal Price,bool Starting)[] Items = [
 ("Alisamentos","Progressiva Orgânica",null,"Curto",180,false),("Alisamentos","Progressiva Orgânica",null,"Médio",200,false),("Alisamentos","Progressiva Orgânica",null,"Longo",280,false),
 ("Alisamentos","Progressiva c/ Formol",null,"Curto",150,false),("Alisamentos","Progressiva c/ Formol",null,"Médio",170,false),("Alisamentos","Progressiva c/ Formol",null,"Longo",200,false),
 ("Alisamentos","Selagem Térmica",null,"Curto",120,false),("Alisamentos","Selagem Térmica",null,"Médio",140,false),("Alisamentos","Selagem Térmica",null,"Longo",200,false),
 ("Alisamentos","Botox Capilar","Botox Formol",null,80,false),("Alisamentos","Botox Capilar","Botox Orgânico",null,100,false),
 ("Outros","Cortes",null,null,70,true),("Outros","Escova ou Babyliss",null,null,70,true),("Outros","Penteados",null,null,130,true),
 ("Coloração","Mechas (Touca)",null,"Curto",200,false),("Coloração","Mechas (Touca)",null,"Médio",260,false),("Coloração","Mechas (Touca)",null,"Longo",320,false),
 ("Coloração","Mechas no Papel / Iluminados",null,null,280,true),("Coloração","Mechas no Papel / Iluminados",null,"Curto",270,false),("Coloração","Mechas no Papel / Iluminados",null,"Médio",350,false),("Coloração","Mechas no Papel / Iluminados",null,"Longo",450,false),
 ("Coloração","Tonalização","Tinta da cliente",null,60,false),("Coloração","Tonalização","Tinta do salão",null,110,false),("Coloração","Banho de Brilho","Tinta da cliente",null,100,false),("Coloração","Banho de Brilho","Tinta do salão",null,130,false),
 ("Tratamentos","Hidratação Essencial",null,null,70,false),("Tratamentos","Nutrição Essencial",null,null,90,false),("Tratamentos","Reconstrução Essencial",null,null,90,false),("Tratamentos","Cauterização Essencial",null,null,120,false),("Tratamentos","Detox Capilar (Essencial)",null,null,80,false),("Tratamentos","Cronograma Capilar","Pacote de 4 sessões",null,280,false),
 ("Combos","Combo Hidratação + Corte + Escova",null,null,69.90m,false),("Combos","Combo Corte + Selagem",null,null,129.90m,false)
 ];
}
