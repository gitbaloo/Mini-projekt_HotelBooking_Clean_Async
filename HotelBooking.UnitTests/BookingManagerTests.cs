using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Core;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private readonly Mock<IRepository<Booking>> mockBookingRepo;
        private readonly Mock<IRepository<Room>> mockRoomRepo;
        private readonly IBookingManager bookingManager;

        // Keep the shared fixture data as fields so tests don't have to call the mock
        // (prevents "self-fulfilling" Verify and improves readability).
        private List<Room> defaultRooms = new();
        private List<Booking> defaultBookings = new();

        public BookingManagerTests()
        {
            mockBookingRepo = new Mock<IRepository<Booking>>();
            mockRoomRepo = new Mock<IRepository<Room>>();

            SetupMocks();

            bookingManager = new BookingManager(
                mockBookingRepo.Object,
                mockRoomRepo.Object
            );
        }

        private void SetupMocks()
        {
            // Shared setup (mental map):
            // Rooms: 1..3
            // Bookings: Room1 booked on Today+1, and all rooms booked on Today+10..Today+20 (fully occupied period)
            defaultRooms = new List<Room>
            {
                new Room { Id = 1, Description = "Room A" },
                new Room { Id = 2, Description = "Room B" },
                new Room { Id = 3, Description = "Room C" },
            };
            mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(defaultRooms);

            defaultBookings = new List<Booking>
            {
                new Booking
                {
                    Id = 1,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    IsActive = true,
                    CustomerId = 1,
                    RoomId = 1
                },
                new Booking
                {
                    Id = 2,
                    StartDate = DateTime.Today.AddDays(10),
                    EndDate = DateTime.Today.AddDays(20),
                    IsActive = true,
                    CustomerId = 1,
                    RoomId = 1
                },
                new Booking
                {
                    Id = 3,
                    StartDate = DateTime.Today.AddDays(10),
                    EndDate = DateTime.Today.AddDays(20),
                    IsActive = true,
                    CustomerId = 2,
                    RoomId = 2
                },
                new Booking
                {
                    Id = 4,
                    StartDate = DateTime.Today.AddDays(10),
                    EndDate = DateTime.Today.AddDays(20),
                    IsActive = true,
                    CustomerId = 2,
                    RoomId = 3
                }
            };
            mockBookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(defaultBookings);

            // Important for async mocks: if AddAsync is called, it must return a Task so 'await' works.
            mockBookingRepo.Setup(r => r.AddAsync(It.IsAny<Booking>()))
                           .Returns(Task.CompletedTask);
        }

        // ----------------------------
        // FindAvailableRoom tests
        // ----------------------------

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_ReturnsRoomThatIsActuallyFree()
        {
            // Arrange
            // Shared setup: Today+1 => Room1 is booked, but Room2/Room3 are free.
            DateTime date = DateTime.Today.AddDays(1);

            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);

            // Assert
            Assert.NotEqual(-1, roomId);
            Assert.Contains(defaultRooms, r => r.Id == roomId);

            // Strong assertion: returned room must have no active booking overlapping this date
            var overlaps = defaultBookings.Where(b =>
                b.IsActive &&
                b.RoomId == roomId &&
                b.StartDate <= date &&
                b.EndDate >= date);

            Assert.Empty(overlaps);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomNotAvailable_ReturnsMinusOne()
        {
            // Arrange
            // Shared setup: Today+10 is within the fully occupied period (10..20) for all rooms.
            DateTime date = DateTime.Today.AddDays(10);

            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);

            // Assert
            Assert.Equal(-1, roomId);
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task FindAvailableRoom_StartNotInFuture_ThrowsArgumentException(int startOffset)
        {
            // Arrange
            // Shared setup not relevant: validation should fail before repository data matters.
            DateTime start = DateTime.Today.AddDays(startOffset);
            DateTime end = DateTime.Today.AddDays(1);

            // Act
            Func<Task> act = () => bookingManager.FindAvailableRoom(start, end);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(act);
        }

        [Theory]
        [InlineData(10, 1)]
        [InlineData(5, 4)]
        public async Task FindAvailableRoom_StartAfterEnd_Throws(int startOffset, int endOffset)
        {
            // Arrange
            // Shared setup not relevant: this is the "start > end" validation partition.
            DateTime start = DateTime.Today.AddDays(startOffset);
            DateTime end = DateTime.Today.AddDays(endOffset);

            // Act
            Func<Task> act = () => bookingManager.FindAvailableRoom(start, end);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(act);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task FindAvailableRoom_EndNotAfterStart_ThrowsArgumentException(int endOffset)
        {
            // Arrange
            // Shared setup not relevant: start is in the future, end is not valid.
            DateTime start = DateTime.Today.AddDays(1);
            DateTime end = DateTime.Today.AddDays(endOffset);

            // Act
            Func<Task> act = () => bookingManager.FindAvailableRoom(start, end);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(act);
        }

        [Theory]
        [InlineData(9, 11)]   // Starts before, ends during fully booked period
        [InlineData(15, 25)]  // Starts during, ends after fully booked period
        [InlineData(8, 25)]   // Encompasses fully booked period
        [InlineData(12, 18)]  // Fully inside fully booked period
        public async Task FindAvailableRoom_OverlappingDates_ReturnsMinusOne(int startOffset, int endOffset)
        {
            // Arrange
            // Shared setup: any overlap with 10..20 should fail because all rooms are booked there.
            DateTime start = DateTime.Today.AddDays(startOffset);
            DateTime end = DateTime.Today.AddDays(endOffset);

            // Act
            int roomId = await bookingManager.FindAvailableRoom(start, end);

            // Assert
            Assert.Equal(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_NoRoomsExist_ReturnsMinusOne()
        {
            // Arrange
            // Override shared setup: 0 rooms => no available room can exist.
            mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room>());
            DateTime start = DateTime.Today.AddDays(1);
            DateTime end = DateTime.Today.AddDays(2);

            // Act
            int roomId = await bookingManager.FindAvailableRoom(start, end);

            // Assert
            Assert.Equal(-1, roomId);
        }

        // ----------------------------
        // GetFullyOccupiedDates tests
        // ----------------------------

        [Fact]
        public async Task GetFullyOccupiedDates_InvalidRange_ThrowsArgumentException()
        {
            // Arrange
            // Shared setup not relevant: start > end is invalid input.
            DateTime start = DateTime.Today.AddDays(2);
            DateTime end = DateTime.Today.AddDays(1);

            // Act
            Func<Task> act = () => bookingManager.GetFullyOccupiedDates(start, end);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(act);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_NoBookings_ReturnsEmptyList()
        {
            // Arrange
            // Override shared setup: no bookings => no fully occupied dates.
            mockBookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Booking>());
            DateTime start = DateTime.Today.AddDays(1);
            DateTime end = DateTime.Today.AddDays(2);

            // Act
            var fullyOccupiedDates = await bookingManager.GetFullyOccupiedDates(start, end);

            // Assert
            Assert.Empty(fullyOccupiedDates);
        }

        [Theory]
        [MemberData(nameof(GetFullyOccupiedDatesData))]
        public async Task GetFullyOccupiedDates_BookingsExist_ReturnsFullyOccupiedDates(
            DateTime startDate,
            DateTime endDate,
            int expectedCount,
            List<DateTime> expectedDates)
        {
            // Arrange
            // Shared setup: days 10..20 are booked in all 3 rooms => exactly those dates are fully occupied.
            // (Arrange data comes from shared setup + MemberData parameters.)

            // Act
            var fullyOccupiedDates = await bookingManager.GetFullyOccupiedDates(startDate, endDate);

            // Assert
            Assert.Equal(expectedCount, fullyOccupiedDates.Count);
            Assert.Equal(expectedDates, fullyOccupiedDates);
        }

        public static IEnumerable<object[]> GetFullyOccupiedDatesData()
        {
            var expectedDates = new List<DateTime>();
            for (DateTime d = DateTime.Today.AddDays(10); d <= DateTime.Today.AddDays(20); d = d.AddDays(1))
            {
                expectedDates.Add(d);
            }

            yield return new object[]
            {
                DateTime.Today.AddDays(9),   // start
                DateTime.Today.AddDays(25),  // end
                11,                          // expectedCount (10..20 inclusive)
                expectedDates
            };
        }

        // ----------------------------
        // CreateBooking tests
        // ----------------------------

        [Fact]
        public async Task CreateBooking_RoomAvailable_SetsState_PersistsAndReturnsTrue()
        {
            // Arrange
            // Shared setup: booking on days 2..8 should be possible (fully booked is 10..20).
            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(2),
                EndDate = DateTime.Today.AddDays(8),
                CustomerId = 1
            };

            // Act
            bool result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.True(result);

            // Observable state changes (stronger than only "result == true")
            Assert.True(booking.IsActive);
            Assert.Contains(defaultRooms, r => r.Id == booking.RoomId);

            // Avoid over-specifying exact room ID; assert the assigned room is actually free for the requested period.
            var overlaps = defaultBookings.Where(b =>
                b.IsActive &&
                b.RoomId == booking.RoomId &&
                !(booking.EndDate < b.StartDate || booking.StartDate > b.EndDate));

            Assert.Empty(overlaps);

            // Verify persistence interaction with meaningful constraints (not blind)
            mockBookingRepo.Verify(r => r.AddAsync(It.Is<Booking>(b =>
                b.IsActive == true &&
                b.RoomId == booking.RoomId &&
                b.StartDate == booking.StartDate &&
                b.EndDate == booking.EndDate &&
                b.CustomerId == booking.CustomerId
            )), Times.Once);
        }

        [Fact]
        public async Task CreateBooking_RoomNotAvailable_DoesNotPersistAndReturnsFalse()
        {
            // Arrange
            // Shared setup: booking on day 10..11 overlaps fully booked period => should fail.
            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(10),
                EndDate = DateTime.Today.AddDays(11),
                CustomerId = 1
            };

            // Act
            bool result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.False(result);

            // Observable state: on failure, booking should not be activated or assigned a room
            Assert.False(booking.IsActive);
            Assert.Equal(0, booking.RoomId);

            mockBookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
        }
    }
}