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
    new DiscountItemDto("ITM50", 5m)
};

app.MapGet("/api/discounts/{code}", (string code) =>
{
    var discountItem = discounts.FirstOrDefault(d => d.Code == code);

    if (discountItem is null)
    {
        return Results.NotFound(new { Error = $"El código {code} no existe" });
    }
    return Results.Ok(discountItem);
})
.WithName("GetDiscountByCode")
.WithOpenApi();

app.Run();