using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Salao.Api.Controllers;
[ApiController, Route("api")]
public class SalonController(SalonDb db,IConfiguration configuration) : ControllerBase {
 private static DateTime BrazilDayStartUtc(DateOnly date)=>new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue),TimeSpan.FromHours(-3)).UtcDateTime;
 private bool HasAdminAccess()=>Request.Headers["X-Admin-Access-Code"]==configuration["Admin:AccessCode"];
 private bool HasAdminChangeCode()=>Request.Headers["X-Admin-Change-Code"]==configuration["Admin:ChangeCode"];
 private bool HasAdminMutationAuthorization()=>HasAdminAccess()&&HasAdminChangeCode();
 [HttpPost("admin/access")] public IActionResult ValidateAdminAccess([FromBody]AdminAccessInput input)=>input.AccessCode==configuration["Admin:AccessCode"]?Ok(new{authorized=true}):Unauthorized();
 [HttpGet("services")] public async Task<IActionResult> Services()=>Ok(await db.Services.OrderBy(x=>x.Category).ThenBy(x=>x.Name).ToListAsync());
 [HttpGet("clients")] public async Task<IActionResult> Clients()=>Ok(await db.Clients.OrderBy(x=>x.Name).ToListAsync());
 [HttpPost("clients")] public async Task<IActionResult> CreateClient(Client c){c.Id=0;if(string.IsNullOrWhiteSpace(c.Name))return BadRequest("Nome é obrigatório.");db.Clients.Add(c);await db.SaveChangesAsync();return Created($"/api/clients/{c.Id}",c);}
 [HttpGet("products")] public async Task<IActionResult> Products()=>Ok(await db.Products.OrderBy(x=>x.Name).ToListAsync());
 [HttpPost("products")] public async Task<IActionResult> CreateProduct(Product p){if(!HasAdminMutationAuthorization())return Unauthorized();if(string.IsNullOrWhiteSpace(p.Name)||p.Price<=0||p.Stock<0)return BadRequest("Informe nome, preço positivo e estoque não negativo.");p.Id=0;p.Name=p.Name.Trim();db.Products.Add(p);await db.SaveChangesAsync();return Created($"/api/products/{p.Id}",p);}
 [HttpPost("services")] public async Task<IActionResult> CreateService([FromBody]CreateCatalogServiceInput input){if(!HasAdminMutationAuthorization())return Unauthorized();if(string.IsNullOrWhiteSpace(input.Category)||string.IsNullOrWhiteSpace(input.Name)||input.Price<=0)return BadRequest("Informe categoria, nome e preço positivo para o serviço.");var service=new CatalogService{Category=input.Category.Trim(),Name=input.Name.Trim(),Variant=string.IsNullOrWhiteSpace(input.Variant)?null:input.Variant.Trim(),HairLength=string.IsNullOrWhiteSpace(input.HairLength)?null:input.HairLength.Trim(),Price=input.Price,PriceIsStartingAt=input.PriceIsStartingAt};db.Services.Add(service);await db.SaveChangesAsync();return Created($"/api/services/{service.Id}",service);}
 [HttpPatch("products/{id:long}")] public async Task<IActionResult> UpdateProduct(long id,[FromBody]UpdateProductInput input){if(!HasAdminMutationAuthorization())return Unauthorized();var p=await db.Products.FindAsync(id);if(p is null)return NotFound();if(string.IsNullOrWhiteSpace(input.Name)||input.Price<=0||input.Stock<0)return BadRequest("Informe nome, preço positivo e estoque não negativo.");p.Name=input.Name.Trim();p.Price=input.Price;p.Stock=input.Stock;await db.SaveChangesAsync();return Ok(p);}
 [HttpDelete("products/{id:long}")] public async Task<IActionResult> DeleteProduct(long id){if(!HasAdminMutationAuthorization())return Unauthorized();var p=await db.Products.FindAsync(id);if(p is null)return NotFound();var pendingUse=await db.AppointmentItems.AnyAsync(i=>i.ProductId==id&&db.Appointments.Any(a=>a.Id==i.AppointmentId&&a.Status=="Agendado"));if(pendingUse)return Conflict("Este produto está em um agendamento pendente e não pode ser excluído.");var historicalItems=await db.AppointmentItems.Where(i=>i.ProductId==id).ToListAsync();foreach(var item in historicalItems)item.ProductId=null;db.Products.Remove(p);await db.SaveChangesAsync();return NoContent();}
 [HttpPatch("services/{id:long}")] public async Task<IActionResult> UpdateServicePrice(long id,[FromBody]UpdateServicePriceInput input){if(!HasAdminMutationAuthorization())return Unauthorized();if(input.Price<=0)return BadRequest("Informe um preço positivo.");var service=await db.Services.FindAsync(id);if(service is null)return NotFound();service.Price=input.Price;await db.SaveChangesAsync();return Ok(service);}
 [HttpGet("appointments")] public async Task<IActionResult> Appointments([FromQuery]DateOnly? from,[FromQuery]DateOnly? to){var q=db.Appointments.Include(a=>a.Client).Include(a=>a.Items).Include(a=>a.Receivables).AsQueryable();if(from is not null){var start=BrazilDayStartUtc(from.Value);q=q.Where(a=>a.StartsAt>=start);}if(to is not null){var end=BrazilDayStartUtc(to.Value.AddDays(1));q=q.Where(a=>a.StartsAt<end);}var rows=await q.OrderBy(a=>a.StartsAt).ToListAsync();return Ok(rows.Select(a=>new{id=a.Id,a.ClientId,client=new{id=a.Client!.Id,a.Client.Name,a.Client.Phone},a.StartsAt,a.Status,a.BookingFeePaid,items=a.Items.Select(i=>new{i.Id,i.ServiceId,i.ProductId,i.Description,i.UnitPrice,i.Quantity}),receivables=a.Receivables.Select(r=>new{r.Id,r.Amount,r.InstallmentNumber,r.DueDate,r.Paid,r.PaymentMethod,r.PaidAt})}));}
 [HttpPost("appointments")] public async Task<IActionResult> CreateAppointment(CreateAppointment input){
  if(input.Items.Count==0)return BadRequest("Informe ao menos um serviço ou produto."); if(!await db.Clients.AnyAsync(c=>c.Id==input.ClientId))return BadRequest("Cliente não encontrado.");
  var startsAt=input.StartsAt.Kind==DateTimeKind.Unspecified?new DateTimeOffset(input.StartsAt,TimeSpan.FromHours(-3)).UtcDateTime:input.StartsAt.ToUniversalTime(); var lines=new List<AppointmentItem>(); foreach(var i in input.Items){var price=i.UnitPrice;if(i.ServiceId is long sid){var s=await db.Services.FindAsync(sid);if(s is null)return BadRequest("Serviço não encontrado.");price??=s.Price;lines.Add(new(){ServiceId=sid,Description=s.Name+(s.Variant is null?"":$" ({s.Variant})")+(s.HairLength is null?"":$" ({s.HairLength})"),UnitPrice=price.Value,Quantity=Math.Max(1,i.Quantity)});}else if(i.ProductId is long pid){var p=await db.Products.FindAsync(pid);if(p is null)return BadRequest("Produto não encontrado.");price??=p.Price;if(p.Stock<Math.Max(1,i.Quantity))return BadRequest($"Estoque insuficiente para {p.Name}.");p.Stock-=Math.Max(1,i.Quantity);lines.Add(new(){ProductId=pid,Description=p.Name,UnitPrice=price.Value,Quantity=Math.Max(1,i.Quantity)});}else if(price is not null)lines.Add(new(){Description=i.Description??"Item avulso",UnitPrice=price.Value,Quantity=Math.Max(1,i.Quantity)});else return BadRequest("Item inválido.");}
  var total=lines.Sum(x=>x.UnitPrice*x.Quantity);var a=new Appointment{ClientId=input.ClientId,StartsAt=startsAt,Items=lines,BookingFeePaid=input.BookingFeePaid};if(input.BookingFeePaid)a.Receivables.Add(new(){Amount=Math.Min(30m,total),InstallmentNumber=0,DueDate=DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-3)),Paid=true,PaymentMethod="Sinal",PaidAt=DateTime.UtcNow});db.Appointments.Add(a);await db.SaveChangesAsync();return Created($"/api/appointments/{a.Id}",new{id=a.Id,total,bookingFeePaid=a.BookingFeePaid});
 }
 [HttpPatch("appointments/{id:long}/items")]
 public async Task<IActionResult> UpdateAppointmentItems(long id,[FromBody] List<SaleItemInput> items){
  if(items is null||items.Count==0)return BadRequest("Mantenha pelo menos um servico ou produto no agendamento.");
  var appointment=await db.Appointments.Include(a=>a.Items).Include(a=>a.Receivables).SingleOrDefaultAsync(a=>a.Id==id);
  if(appointment is null)return NotFound();
  if(appointment.Status!="Agendado")return Conflict("Somente agendamentos pendentes podem ser editados.");
  if(appointment.Receivables.Any(r=>r.Paid&&r.InstallmentNumber!=0))return Conflict("Nao e possivel alterar os itens depois que uma parcela foi paga.");
  var lines=new List<AppointmentItem>();
  foreach(var item in items){
   var quantity=Math.Max(1,item.Quantity);
   if(item.ServiceId is long serviceId&&item.ProductId is null){var service=await db.Services.FindAsync(serviceId);if(service is null)return BadRequest("Servico nao encontrado.");var description=service.Name+(service.Variant is null?"":$" ({service.Variant})")+(service.HairLength is null?"":$" ({service.HairLength})");lines.Add(new(){AppointmentId=id,ServiceId=serviceId,Description=description,UnitPrice=service.Price,Quantity=quantity});}
   else if(item.ProductId is long productId&&item.ServiceId is null){var product=await db.Products.FindAsync(productId);if(product is null)return BadRequest("Produto nao encontrado.");lines.Add(new(){AppointmentId=id,ProductId=productId,Description=product.Name,UnitPrice=product.Price,Quantity=quantity});}
   else if(item.ServiceId is null&&item.ProductId is null&&item.UnitPrice is >0){lines.Add(new(){AppointmentId=id,Description=item.Description??"Item avulso",UnitPrice=item.UnitPrice.Value,Quantity=quantity});}
   else return BadRequest("Cada item deve indicar um servico ou produto valido.");
  }
  var oldProducts=appointment.Items.Where(i=>i.ProductId is not null).GroupBy(i=>i.ProductId!.Value).ToDictionary(g=>g.Key,g=>g.Sum(i=>i.Quantity));
  var newProducts=lines.Where(i=>i.ProductId is not null).GroupBy(i=>i.ProductId!.Value).ToDictionary(g=>g.Key,g=>g.Sum(i=>i.Quantity));
  foreach(var productId in oldProducts.Keys.Union(newProducts.Keys)){var product=await db.Products.FindAsync(productId);if(product is null)return BadRequest("Produto nao encontrado.");var delta=oldProducts.GetValueOrDefault(productId)-newProducts.GetValueOrDefault(productId);if(delta<0&&product.Stock < -delta)return BadRequest($"Estoque insuficiente para {product.Name}.");product.Stock+=delta;}
  db.AppointmentItems.RemoveRange(appointment.Items);appointment.Items=lines;
  var installments=appointment.Receivables.Where(r=>r.InstallmentNumber>0&&!r.Paid).OrderBy(r=>r.InstallmentNumber).ToList();
  var total=lines.Sum(i=>i.UnitPrice*i.Quantity);if(installments.Count>0){var installmentValue=Math.Round(total/installments.Count,2);for(var index=0;index<installments.Count;index++)installments[index].Amount=index==installments.Count-1?total-installmentValue*(installments.Count-1):installmentValue;}
  await db.SaveChangesAsync();return Ok(new{id=appointment.Id,total,items=lines.Select(i=>new{i.Description,i.UnitPrice,i.Quantity}),installments=installments.Select(r=>new{r.InstallmentNumber,r.Amount,r.DueDate})});
 }
 [HttpGet("dashboard")] public async Task<IActionResult> Dashboard(){var today=DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-3));var due=await db.Receivables.Include(r=>r.Appointment!).ThenInclude(a=>a.Client).Where(r=>!r.Paid&&r.DueDate==today.AddDays(1)).OrderBy(r=>r.Appointment!.Client!.Name).ToListAsync();var start=BrazilDayStartUtc(today);var end=BrazilDayStartUtc(today.AddDays(1));var todayAppointments=await db.Appointments.CountAsync(a=>a.StartsAt>=start&&a.StartsAt<end);var receivedToday=await db.Receivables.Where(r=>r.Paid&&r.PaidAt!=null&&r.PaidAt>=start&&r.PaidAt<end).SumAsync(r=>(decimal?)r.Amount)??0m;return Ok(new{tomorrow=due.Select(r=>new{r.Id,r.Amount,r.InstallmentNumber,r.DueDate,client=r.Appointment!.Client!.Name}),todayAppointments,todayRevenue=receivedToday});}
 [HttpPatch("receivables/{id:long}/paid")] public async Task<IActionResult> MarkPaid(long id,[FromBody]MarkReceivablePaidInput input){var r=await db.Receivables.FindAsync(id);if(r is null)return NotFound();if(!ValidMethod(input.PaymentMethod))return BadRequest("Escolha Pix, Débito ou Crédito.");r.Paid=true;r.PaymentMethod=input.PaymentMethod;r.PaidAt=DateTime.UtcNow;await db.SaveChangesAsync();return NoContent();}
 [HttpGet("reports")] public async Task<IActionResult> Reports([FromQuery]DateOnly from,[FromQuery]DateOnly to){var a=BrazilDayStartUtc(from);var b=BrazilDayStartUtc(to.AddDays(1));var appointments=await db.Appointments.CountAsync(x=>x.Status=="Concluído"&&x.StartsAt>=a&&x.StartsAt<b);var revenue=await db.Receivables.Where(r=>r.Paid&&r.PaidAt!=null&&r.PaidAt>=a&&r.PaidAt<b).SumAsync(r=>(decimal?)r.Amount)??0m;return Ok(new{appointments,revenue,from,to});}
 [HttpPost("appointments/{id:long}/checkout")]
 public async Task<IActionResult> Checkout(long id,[FromBody]CheckoutInput input){
  var a=await db.Appointments.Include(x=>x.Items).Include(x=>x.Receivables).SingleOrDefaultAsync(x=>x.Id==id);
  if(a is null)return NotFound();if(a.Status!="Agendado")return Conflict("Este agendamento já foi concluído.");
  var total=a.Items.Sum(i=>i.UnitPrice*i.Quantity);var paid=a.Receivables.Where(r=>r.Paid).Sum(r=>r.Amount);var balance=Math.Max(0,total-paid);var today=DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-3));
  decimal deferredTotal=0;
  if(input.Mode=="paid"){
   if(!ValidMethod(input.PaymentMethod))return BadRequest("Escolha Pix, Débito ou Crédito.");
   if(balance>0)a.Receivables.Add(new(){Amount=balance,InstallmentNumber=1,DueDate=today,Paid=true,PaymentMethod=input.PaymentMethod,PaidAt=DateTime.UtcNow});
  }else if(input.Mode=="deferred"){
   var parts=input.Installments??[];
   if(parts.Count is <1 or >5||parts.Any(p=>p.Amount<=0||p.DueDate<today))return BadRequest("Informe de 1 a 5 parcelas positivas com vencimento válido.");
   if(parts.Count(p=>p.IsEntry)>1||parts.Select((p,index)=>(p,index)).Any(x=>x.p.IsEntry&&x.index!=0))return BadRequest("A entrada deve ser única e ser a primeira parcela.");
   if(parts.Any(p=>p.IsEntry&&p.DueDate!=today))return BadRequest("A entrada vence na data do atendimento.");
   var totalCents=decimal.ToInt64(decimal.Round(balance*100m,0,MidpointRounding.AwayFromZero));var hasEntry=parts[0].IsEntry;if(hasEntry&&parts.Count<2)return BadRequest("Com entrada, informe ao menos uma parcela futura.");var entryCents=hasEntry?decimal.ToInt64(decimal.Round(parts[0].Amount*100m,0,MidpointRounding.AwayFromZero)):0;if(entryCents>=totalCents&&hasEntry)return BadRequest("A entrada deve ser menor que o saldo para permitir o parcelamento.");var financedCents=totalCents-entryCents;var futureCount=parts.Count-(hasEntry?1:0);var rate=parts.Count>2?0.03m:0m;decimal installmentAmount;
   if(rate==0)installmentAmount=decimal.Round((financedCents/100m)/futureCount,2,MidpointRounding.AwayFromZero);
   else{var discount=(decimal)Math.Pow((double)(1m+rate),-futureCount);installmentAmount=decimal.Round((financedCents/100m)*rate/(1m-discount),2,MidpointRounding.AwayFromZero);}
   for(var index=0;index<parts.Count;index++){
    var expected=hasEntry&&index==0?entryCents/100m:installmentAmount;
    if(Math.Abs(parts[index].Amount-expected)>0.01m)return BadRequest("Os valores das parcelas foram alterados. Confira o plano recalculado na tela.");
    deferredTotal+=parts[index].Amount;
   }
   db.Receivables.RemoveRange(a.Receivables.Where(r=>!r.Paid));
   for(var index=0;index<parts.Count;index++)a.Receivables.Add(new(){Amount=parts[index].Amount,InstallmentNumber=index+1,DueDate=parts[index].DueDate,Paid=false});
  }else return BadRequest("Opção de pagamento inválida.");
  a.Status="Concluído";await db.SaveChangesAsync();
  return Ok(new{id=a.Id,total,paid=paid+(input.Mode=="paid"?balance:0),outstanding=input.Mode=="paid"?0:deferredTotal});
 }
 [HttpGet("clients/{id:long}/financial")] public async Task<IActionResult> ClientFinancial(long id){var c=await db.Clients.FindAsync(id);if(c is null)return NotFound();var rows=await db.Appointments.Include(a=>a.Items).Include(a=>a.Receivables).Where(a=>a.ClientId==id).OrderByDescending(a=>a.StartsAt).ToListAsync();return Ok(new{client=new{id=c.Id,c.Name,c.Phone},appointments=rows.Select(a=>new{id=a.Id,a.StartsAt,a.Status,total=a.Items.Sum(i=>i.UnitPrice*i.Quantity),paid=a.Receivables.Where(r=>r.Paid).Sum(r=>r.Amount),outstanding=(a.Receivables.Where(r=>!r.Paid).Sum(r=>r.Amount)>0?a.Receivables.Where(r=>!r.Paid).Sum(r=>r.Amount):Math.Max(0,a.Items.Sum(i=>i.UnitPrice*i.Quantity)-a.Receivables.Where(r=>r.Paid).Sum(r=>r.Amount))),items=a.Items.Select(i=>new{i.Description,i.UnitPrice,i.Quantity}),payments=a.Receivables.OrderBy(r=>r.DueDate).Select(r=>new{r.Id,r.Amount,r.InstallmentNumber,r.DueDate,r.Paid,r.PaymentMethod,r.PaidAt})})});}
 private static bool ValidMethod(string? method)=>method is "Pix" or "Débito" or "Crédito";
 [HttpPatch("appointments/{id:long}/complete")] public IActionResult Complete(long id)=>BadRequest("Use a tela de fechamento para registrar o pagamento ou as datas das parcelas.");
}
