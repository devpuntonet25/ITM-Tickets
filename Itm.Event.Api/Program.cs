using Itm.Event.Api.Dtos;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Me permite genera la documentación de la API con Swagger/OpenAPI

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Habilita el middleware de Swagger para generar la documentación en tiempo de ejecución
    app.UseSwaggerUI(); // Habilita la interfaz de usuario de Swagger para visualizar la documentación
}

app.UseHttpsRedirection(); // Redirige las solicitudes HTTP a HTTPS

// Aquí simulamos la base de datos para los eventos
var events = new List<EventItemDto>
{
    new EventItemDto(1, "Concierto ITM", 50000, 100)
};

app.MapGet("/api/events/{id}", (int id) =>
{
    var e = events.FirstOrDefault(e => e.EventId == id);
    return e is not null ? Results.Ok(e) : Results.NotFound();
})
.WithName("GetEventInfo")
.WithOpenApi();

app.MapPost("/api/events/reserve", (TicketRequestDto request) =>
{
    if (request.EventId <= 0 || request.Quantity <= 0)
    {
        return Results.BadRequest("EventId y Quantity deben ser mayores a 0");
    }

    var e = events.FirstOrDefault(e => e.EventId == request.EventId);
    
    if (e is null)
    {
        return Results.BadRequest($"No se encontró el evento con id{request.EventId}");
    }

    if (e.AvailableSeats < request.Quantity)
    {
        return Results.BadRequest("No hay suficientes sillas");
    }

    e.AvailableSeats = e.AvailableSeats - request.Quantity;

    return Results.Ok(new { Message = "Reserva exitosa" });

})
.WithName("SetReserve")
.WithOpenApi();

app.MapPost("/api/events/release", (TicketRequestDto request) =>
{
    if (request.EventId <= 0 || request.Quantity <= 0)
    {
        return Results.BadRequest("EventId y Quantity deben ser mayores a 0");
    }

    var e = events.FirstOrDefault(e => e.EventId == request.EventId);

    if (e is null)
    {
        return Results.BadRequest($"No se encontró el evento con id{request.EventId}");
    }

    e.AvailableSeats = e.AvailableSeats + request.Quantity;

    return Results.Ok(new { Message = "Liberación de sillas exitosa" });

})
.WithName("EventRelease")
.WithOpenApi();

app.Run();

record TicketRequestDto(int EventId, int Quantity);