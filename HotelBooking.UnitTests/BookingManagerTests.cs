using System;
using System.Collections.Generic;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Xunit;
using Moq;
using System.Linq;
using System.Threading.Tasks;


namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private Mock<IRepository<Booking>> mockBookingRepo;
        private Mock<IRepository<Room>> mockRoomRepo;
        private IBookingManager bookingManager;

        public BookingManagerTests(){
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
            var rooms = new List<Room>
            {
                new Room { Id = 1, Description = "Room A" },
                new Room { Id = 2, Description = "Room B" },
                new Room { Id = 3, Description = "Room C" },
            };
            mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
            
            var bookings = new List<Booking>
            {
                new Booking
                {
                    Id=1, 
                    StartDate=DateTime.Today.AddDays(1), 
                    EndDate=DateTime.Today.AddDays(1), 
                    IsActive=true, 
                    CustomerId=1, 
                    RoomId=1
                },
                new Booking
                {
                    Id=2, 
                    StartDate=DateTime.Today.AddDays(10), 
                    EndDate=DateTime.Today.AddDays(20), 
                    IsActive=true, 
                    CustomerId=1, 
                    RoomId=1
                },
                new Booking
                {
                    Id=3, 
                    StartDate=DateTime.Today.AddDays(10), 
                    EndDate=DateTime.Today.AddDays(20), 
                    IsActive=true, 
                    CustomerId=2, 
                    RoomId=2
                },
                new Booking
                {
                    Id=4, 
                    StartDate=DateTime.Today.AddDays(10), 
                    EndDate=DateTime.Today.AddDays(20), 
                    IsActive=true, 
                    CustomerId=2, 
                    RoomId=3
                }
            };
            mockBookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);
        }

        /*
         * Tests for FindAvailableRoom Method
         */
        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);
            
            // Assert
            Assert.NotEqual(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomNotAvailable_ReturnsMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(10);
            
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);
            
            // Assert
            Assert.Equal(-1, roomId);
            
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_ReturnsAvailableRoom()
        {

            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);

            var allBookings = await mockBookingRepo.Object.GetAllAsync();
            var bookingForReturnedRoomId = allBookings.
                Where(b => b.RoomId == roomId
                           && b.StartDate <= date
                           && b.EndDate >= date
                           && b.IsActive);
            
            // Assert
            Assert.Empty(bookingForReturnedRoomId);
            mockBookingRepo.Verify(r => r.GetAllAsync(), Times.AtLeastOnce());
            mockRoomRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }
        
        /*
         * Data Driven Tests that checks for invalid start dates
         * End date is set to be 1 day after today
         */
        [Theory]
        [InlineData(-10)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(10)]
        public async Task FindAvailableRoom_InvalidStartDate_ThrowsArgumentException(int startDate)
        {
            // Arrange
            DateTime invalidDate = DateTime.Today.AddDays(startDate);
            DateTime endDate = DateTime.Today.AddDays(1);
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await bookingManager.FindAvailableRoom(invalidDate, endDate));
        }
        
        /*
         * Data Driven Tests that checks for invalid end dates
         * Start date is set to be 1 day after today
         */
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task FindAvailableRoom_InvalidEndDate_ThrowsArgumentException(int endDate)
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime invalidEndDate = DateTime.Today.AddDays(endDate);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await bookingManager.FindAvailableRoom(startDate, invalidEndDate));
        }

        [Theory]
        [InlineData(9, 11)]   // Starts before, ends during booking period
        [InlineData(15, 25)]  // Starts during, ends after booking period
        [InlineData(8, 25)]   // Completely encompasses existing booking
        [InlineData(12, 18)]  // Completely within existing booking period
        public async Task FindAvailableRoom_OverlappingDates_ReturnsMinusOne(int startDate, int endDate)
        {
            // Arrange 
            DateTime start = DateTime.Today.AddDays(startDate);
            DateTime end = DateTime.Today.AddDays(endDate);
            
            // Act
            int roomId = await bookingManager.FindAvailableRoom(start, end);
            
            // Assert
            Assert.Equal(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_NoRoomsExist_ReturnsMinusOne()
        {
            // Arrange
            mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room>());
            DateTime start = DateTime.Today.AddDays(1);
            DateTime end = DateTime.Today.AddDays(2);

            // Act
            int roomId = await bookingManager.FindAvailableRoom(start, end);

            // Assert
            Assert.Equal(-1, roomId);
            mockRoomRepo.Verify(r => r.GetAllAsync(), Times.AtLeastOnce());
        }
        
        /*
         * Tests for GetFullyOccupiedDates Method
         */
        [Fact]
        public async Task GetFullyOccupiedDates_InvalidStartDates_ThrowsArgumentException()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(2);
            DateTime endDate = DateTime.Today.AddDays(1);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await bookingManager.GetFullyOccupiedDates(startDate, endDate));
        }

        [Fact]
        public async Task GetFullyOccupiedDates_NoBookings_ReturnsEmptyList()
        {
            // Arrange
            mockBookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Booking>());
            DateTime start = DateTime.Today.AddDays(1);
            DateTime end = DateTime.Today.AddDays(2);
            
            // Act
            var fullyOccupiedDates = await bookingManager.GetFullyOccupiedDates(start, end);
            
            // Assert
            Assert.Empty(fullyOccupiedDates);
            mockBookingRepo.Verify(r => r.GetAllAsync(), Times.AtLeastOnce());
        }
        
        [Theory]
        [MemberData(nameof(GetFullyOccupiedDatesData))]
        public async Task GetFullyOccupiedDates_BookingsExist_ReturnsFullyOccupiedDates(
            DateTime startDate,
            DateTime endDate,
            int expectedCount,
            List<DateTime> expectedDates
            )
        
        {
            // Act
            var fullyOccupiedDates = await bookingManager.GetFullyOccupiedDates(startDate, endDate);
            
            // Assert
            Assert.Equal(expectedCount, fullyOccupiedDates.Count);
            Assert.Equal(expectedDates, fullyOccupiedDates);
            
            mockBookingRepo.Verify(r => r.GetAllAsync(), Times.AtLeastOnce());
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
                11,                          // expectedCount
                expectedDates                // expectedDates
            };
        }
            
        // Test for CreateBooking method

        [Fact]
        public async Task CreateBooking_RoomAvailable_ReturnsTrue()
        {
            // Arrage
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
            mockBookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Once);

        }
        
        [Fact]
        public async Task CreateBooking_RoomNotAvailable_ReturnsFalse()
        {
            // Arrage
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
            mockBookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);

        }

        [Fact]
        public async Task CreateBooking_RoomAvailable_AssignsRoom1()
        {
            // Arrage
            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(7),
                EndDate = DateTime.Today.AddDays(7),
                CustomerId = 1
            };
            
            // Act
            bool result = await bookingManager.CreateBooking(booking);
            
            // Assert
            Assert.True(result);
            Assert.Equal(1, booking.RoomId);
            mockBookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Once);
        }
    }
}
