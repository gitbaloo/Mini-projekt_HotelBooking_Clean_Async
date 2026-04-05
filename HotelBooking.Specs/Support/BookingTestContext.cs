using HotelBooking.Core;
using Moq;

namespace HotelBooking.Specs.Support
{
    /// <summary>
    /// Shared test state injected into step definition classes via SpecFlow's DI.
    /// Holds mock setup, the system under test, and captured results.
    /// </summary>
    public class BookingTestContext
    {
        // ── Mocks ─────────────────────────────────────────────────────────────
        public Mock<IRepository<Booking>> MockBookingRepo { get; } = new();
        public Mock<IRepository<Room>> MockRoomRepo { get; } = new();

        // ── Data ──────────────────────────────────────────────────────────────
        public List<Room> Rooms { get; set; } = new();
        public List<Booking> Bookings { get; set; } = new();

        // ── System under test ─────────────────────────────────────────────────
        public IBookingManager BookingManager { get; private set; } = null!;

        // ── Captured results ──────────────────────────────────────────────────
        public Booking? LastBooking { get; set; }
        public bool? CreateBookingResult { get; set; }
        public List<DateTime>? OccupiedDatesResult { get; set; }
        public Exception? ThrownException { get; set; }

        // ── Query inputs ──────────────────────────────────────────────────────
        public DateTime OccupiedQueryStart { get; set; }
        public DateTime OccupiedQueryEnd { get; set; }

        /// <summary>
        /// Call once the Rooms and Bookings lists are populated to (re)build the SUT.
        /// </summary>
        public void BuildBookingManager()
        {
            MockRoomRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(() => Rooms.ToList()); // lambda so later mutations are picked up

            MockBookingRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(() => Bookings.ToList());

            MockBookingRepo
                .Setup(r => r.AddAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            BookingManager = new BookingManager(MockBookingRepo.Object, MockRoomRepo.Object);
        }

        // ── Date helper ───────────────────────────────────────────────────────

        /// <summary>
        /// Parses date tokens like "Today+2", "Today-1", "Today", or ISO dates.
        /// </summary>
        public static DateTime ParseDate(string token)
        {
            token = token.Trim();

            if (token.StartsWith("Today+", StringComparison.OrdinalIgnoreCase))
                return DateTime.Today.AddDays(int.Parse(token[6..]));

            if (token.StartsWith("Today-", StringComparison.OrdinalIgnoreCase))
                return DateTime.Today.AddDays(-int.Parse(token[6..]));

            if (token.Equals("Today", StringComparison.OrdinalIgnoreCase))
                return DateTime.Today;

            return DateTime.Parse(token);
        }
    }
}
