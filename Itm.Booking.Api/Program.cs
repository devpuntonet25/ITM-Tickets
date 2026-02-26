using Itm.Booking.Api.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registro del cliente para Eventos
builder.Services.AddHttpClient("EventClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5021");
    client.Timeout = TimeSpan.FromSeconds(6);
}).AddStandardResilienceHandler();

// Registro del cliente para Descuentos
builder.Services.AddHttpClient("DiscountClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5096");
    client.Timeout = TimeSpan.FromSeconds(5);
}).AddStandardResilienceHandler();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/bookings", async (TicketBookingDto request, IHttpClientFactory httpClientFactory, ILogger<Program> logger) =>
{
    var eventClient = httpClientFactory.CreateClient("EventClient");
    var discountClient = httpClientFactory.CreateClient("DiscountClient");
    var eventPayload = new TicketRequestDto(request.EventId, request.Tickets);

    try
    {
        var eventTask = eventClient.PostAsJsonAsync($"/api/events/reserve", eventPayload);
        var discountTask = discountClient.GetAsync($"/api/discounts/{request.DiscountCode}");

        await Task.WhenAll(eventTask, discountTask);

        var eventResponse = eventTask.Result;
        var discountResponse = discountTask.Result;

        EventItemDto? eventResult = null;
        if (eventResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var errorContent = await eventResponse.Content.ReadAsStringAsync();
            
            return Results.BadRequest(new { Error = $"Error reservando el evento: {errorContent}" });
        }

        eventResult = await eventResponse.Content.ReadFromJsonAsync<EventItemDto>();

        var discountPercentage = 0m;
        DiscountItemDto? discountResult = null;
        if (discountResponse.IsSuccessStatusCode)
        {
            discountResult = await discountResponse.Content.ReadFromJsonAsync<DiscountItemDto>();
            discountPercentage = discountResult.Percentage;
        }

        var successPayment = new Random().Next(1, 10) > 5;

        if (!successPayment) throw new PaymentFailedException(request.EventId, request.Tickets);

        logger.LogInformation($"Pago procesado exitosamente para el evento {request.EventId}, aplicando descuento del {discountPercentage}%");

        var priceWithoutDiscount = request.Tickets * eventResult?.BasePrice;
        var totalDiscount = ((priceWithoutDiscount) * discountPercentage) / 100;

        // Resto de lógica
        return Results.Ok(new {
            EventReserved = true,
            discountApplied = discountPercentage > 0,
            eventResult?.EventName,
            eventResult?.BasePrice,
            DiscountPercentage = $"{discountPercentage}%",
            TotalDiscount = totalDiscount,
            Total = priceWithoutDiscount - totalDiscount
              }); // Mientras se define la lógica
              
    } catch (PaymentFailedException ex) 
    {
            logger.LogError($"Error procesando el pago, realizando liberación de {ex.Tickets} sillas para el evento {ex.EventId}");
            var releaseResponse = await eventClient.PostAsJsonAsync($"/api/events/release", eventPayload);

            return Results.Problem("Tu pago fue rechazado. No te preocupes, no te cobramos y tus sillas fueron liberadas.");
    
    } catch (Exception ex)
    {
        return Results.Problem($"Error registrando ticket: {ex.Message}");
    }
})
.WithName("Booking");
// .WithOpenApi();


app.Run();

record TicketBookingDto(int EventId , int Tickets, string DiscountCode);
record TicketRequestDto(int EventId, int Quantity);
record EventItemDto(int EventId, string EventName, decimal BasePrice, int AvailableSeats);
public record DiscountItemDto(string Message, decimal Percentage);