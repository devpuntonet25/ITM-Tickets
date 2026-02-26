namespace Itm.Booking.Api.Exceptions;
public class PaymentFailedException : Exception
{
    public int EventId { get; }
    public int Tickets { get; }

    public PaymentFailedException(int eventId, int tickets)
        : base($"Error procesando el pago para el evento {eventId}")
    {
        EventId = eventId;
        Tickets = tickets;
    }
}