using HotelBooking.Core;
using HotelBooking.Infrastructure;

namespace HotelBooking.BddTests.Services;

public class DatabaseSeedService
{
    private readonly HotelBookingContext _context;

    public DatabaseSeedService(HotelBookingContext context)
    {
        _context = context;
    }

    public async Task SeedBaselineAsync()
    {
        var customers = new List<Customer>
        {
            new Customer { Name = "John Smith", Email = "js@gmail.com" },
            new Customer { Name = "Jane Doe", Email = "jd@gmail.com" }
        };

        var rooms = new List<Room>
        {
            new Room { Description = "A" },
            new Room { Description = "B" },
            new Room { Description = "C" }
        };

        _context.Customer.AddRange(customers);
        _context.Room.AddRange(rooms);
        await _context.SaveChangesAsync();

        DateTime date = DateTime.Today.AddDays(4);

        var bookings = new List<Booking>
        {
            new Booking
            {
                StartDate = date,
                EndDate = date.AddDays(14),
                IsActive = true,
                CustomerId = customers[0].Id,
                RoomId = rooms[0].Id
            },
            new Booking
            {
                StartDate = date,
                EndDate = date.AddDays(14),
                IsActive = true,
                CustomerId = customers[1].Id,
                RoomId = rooms[1].Id
            },
            new Booking
            {
                StartDate = date,
                EndDate = date.AddDays(14),
                IsActive = true,
                CustomerId = customers[0].Id,
                RoomId = rooms[2].Id
            }
        };

        _context.Booking.AddRange(bookings);
        await _context.SaveChangesAsync();
    }
}