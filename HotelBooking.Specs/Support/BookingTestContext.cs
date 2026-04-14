using HotelBooking.Core;

namespace HotelBooking.Specs.Support
{
    /// <summary>
    /// Shared test state injected into step definition classes via SpecFlow's DI.
    /// Uses fully self-contained in-memory repositories
    /// </summary>
    public class BookingTestContext
    {
        // ── Data ──────────────────────────────────────────────────────────────
        public List<Room> Rooms { get; set; } = [];
        public List<Booking> Bookings { get; set; } = [];

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
        /// Builds the BookingManager backed by fresh in-memory repositories
        /// seeded from the current Rooms and Bookings lists.
        /// Call once both lists are populated.
        /// </summary>
        public void BuildBookingManager()
        {
            var bookingRepo = new BookingRepository(Bookings);
            var roomRepo = new RoomRepository(Rooms);

            BookingManager = new BookingManager(bookingRepo, roomRepo);
        }

        // ── Date helper ───────────────────────────────────────────────────────

        /// <summary>
        /// Parses date tokens like "Today+2", "Today-1", "Today", or ISO dates.
        /// Returns DateTime.MinValue for the token "NULL" (simulates a missing date).
        /// </summary>
        public static DateTime ParseDate(string token)
        {
            token = token.Trim();

            if (token.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                return DateTime.MinValue;

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
