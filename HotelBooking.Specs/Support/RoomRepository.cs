using HotelBooking.Core;

namespace HotelBooking.Specs.Support
{
    /// <summary>
    /// A self-contained in-memory implementation of IRepository&lt;Room&gt;.
    /// Seeded from the test context's Rooms list — never touches the real database.
    /// </summary>
    public class RoomRepository(IEnumerable<Room> seed) : IRepository<Room>
    {
        private readonly List<Room> _store = [.. seed];

        public Task<IEnumerable<Room>> GetAllAsync()
            => Task.FromResult<IEnumerable<Room>>([.. _store]);

        public Task<Room> GetAsync(int id)
            => Task.FromResult(_store.First(r => r.Id == id));

        public Task AddAsync(Room entity)
        {
            entity.Id = _store.Count == 0 ? 1 : _store.Max(r => r.Id) + 1;
            _store.Add(entity);
            return Task.CompletedTask;
        }

        public Task EditAsync(Room entity)
        {
            var index = _store.FindIndex(r => r.Id == entity.Id);
            if (index >= 0) _store[index] = entity;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(int id)
        {
            _store.RemoveAll(r => r.Id == id);
            return Task.CompletedTask;
        }
    }
}
