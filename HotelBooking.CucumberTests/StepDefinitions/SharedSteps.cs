using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Core;
using Moq;
using Reqnroll;

namespace HotelBooking.CucumberTests.StepDefinitions
{
    [Binding]
    public class SharedSteps
    {
        private readonly Support.BookingContext _ctx;

        public SharedSteps(Support.BookingContext ctx)
        {
            _ctx = ctx;
        }

        [Given("the hotel has {int} rooms")]
        public void GivenTheHotelHasRooms(int roomCount)
        {
            _ctx.Rooms = Enumerable.Range(1, roomCount)
                .Select(i => new Room { Id = i, Description = $"Room {(char)('A' + i - 1)}" })
                .ToList();

            _ctx.MockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(_ctx.Rooms);
            _ctx.MockBookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(_ctx.Bookings);
            _ctx.MockBookingRepo.Setup(r => r.AddAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            _ctx.BookingManager = new BookingManager(
                _ctx.MockBookingRepo.Object,
                _ctx.MockRoomRepo.Object);
        }

        [Given("room {int} is booked from {string} to {string}")]
        public void GivenRoomIsBookedFromTo(int roomId, string startExpr, string endExpr)
        {
            var booking = new Booking
            {
                Id = _ctx.Bookings.Count + 1,
                StartDate = Support.BookingContext.ParseRelativeDate(startExpr),
                EndDate = Support.BookingContext.ParseRelativeDate(endExpr),
                IsActive = true,
                CustomerId = 1,
                RoomId = roomId
            };
            _ctx.Bookings.Add(booking);

            _ctx.MockBookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(_ctx.Bookings);
        }
    }
}

