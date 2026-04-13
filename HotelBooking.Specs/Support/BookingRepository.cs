using HotelBooking.Core;

namespace HotelBooking.Specs.Support
{
    /// <summary>
    /// A self-contained in-memory implementation of IRepository&lt;Booking&gt;.
    /// Seeded from the test context's Bookings list — never touches the real database.
    /// </summary>
    public class BookingRepository(IEnumerable<Booking> seed) : IRepository<Booking>
    {
        private readonly List<Booking> _store = [.. seed];

        public Task<IEnumerable<Booking>> GetAllAsync()
            => Task.FromResult<IEnumerable<Booking>>([.. _store]);

        public Task<Booking> GetAsync(int id)
            => Task.FromResult(_store.First(b => b.Id == id));

        public Task AddAsync(Booking booking)
        {
            booking.Id = _store.Count == 0 ? 1 : _store.Max(b => b.Id) + 1;
            _store.Add(booking);
            return Task.CompletedTask;
        }

        public Task EditAsync(Booking booking)
        {
            var index = _store.FindIndex(b => b.Id == booking.Id);
            if (index >= 0) _store[index] = booking;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(int id)
        {
            _store.RemoveAll(b => b.Id == id);
            return Task.CompletedTask;
        }
    }
}
