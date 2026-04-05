using HotelBooking.Infrastructure;

namespace HotelBooking.BddTests.Services;

public class DatabaseResetService
{
    private readonly HotelBookingContext _context;

    public DatabaseResetService(HotelBookingContext context)
    {
        _context = context;
    }

    public async Task ResetAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
    }
}