namespace Itm.Event.Api.Dtos;
public record EventItemDto(int EventId, string EventName, decimal BasePrice, int AvailableSeats)
{
    public int AvailableSeats { get; set; } = AvailableSeats;
}