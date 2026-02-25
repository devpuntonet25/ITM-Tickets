using Itm.Discount.Api.Dtos;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var discounts = new List<DiscountItemDto>
{
    new DiscountItemDto("ITM50", 0.5m)
};

app.MapGet("/api/discounts/{code}", (string code) =>
{
    var discountItem = discounts.FirstOrDefault(d => d.Code == code);

    return discountItem is null ? Results.NotFound($"El código {code} no existe") : Results.Ok(new { DiscountItem = discountItem, Message = "Se ha encontrado el código de descuento" });
})
.WithName("GetDiscountByCode")
.WithOpenApi();

app.Run();