using System;
using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.CucumberTests.Support;
using Reqnroll;
using Xunit;

namespace HotelBooking.CucumberTests.StepDefinitions
{
    [Binding]
    public class CreateBookingSteps
    {
        private readonly BookingContext _ctx;

        public CreateBookingSteps(BookingContext ctx)
        {
            _ctx = ctx;
        }

        [When("I create a booking from {string} to {string} for customer {int}")]
        public async Task WhenICreateABookingFromToForCustomer(string startExpr, string endExpr, int customerId)
        {
            _ctx.LastBooking = new Booking
            {
                StartDate = BookingContext.ParseRelativeDate(startExpr),
                EndDate = BookingContext.ParseRelativeDate(endExpr),
                CustomerId = customerId
            };

            _ctx.CaughtException = null;
            try
            {
                _ctx.BookingResult = await _ctx.BookingManager.CreateBooking(_ctx.LastBooking);
            }
            catch (Exception ex)
            {
                _ctx.CaughtException = ex;
            }
        }

        [When("I try to create a booking from {string} to {string} for customer {int}")]
        public async Task WhenITryToCreateABookingFromToForCustomer(string startExpr, string endExpr, int customerId)
        {
            _ctx.LastBooking = new Booking
            {
                StartDate = BookingContext.ParseRelativeDate(startExpr),
                EndDate = BookingContext.ParseRelativeDate(endExpr),
                CustomerId = customerId
            };

            _ctx.CaughtException = null;
            try
            {
                _ctx.BookingResult = await _ctx.BookingManager.CreateBooking(_ctx.LastBooking);
            }
            catch (Exception ex)
            {
                _ctx.CaughtException = ex;
            }
        }

        [Then("the booking should be created successfully")]
        public void ThenTheBookingShouldBeCreatedSuccessfully()
        {
            Assert.Null(_ctx.CaughtException);
            Assert.True(_ctx.BookingResult);
        }

        [Then("the booking should not be created")]
        public void ThenTheBookingShouldNotBeCreated()
        {
            Assert.Null(_ctx.CaughtException);
            Assert.False(_ctx.BookingResult);
        }

        [Then("the booking should be marked as active")]
        public void ThenTheBookingShouldBeMarkedAsActive()
        {
            Assert.True(_ctx.LastBooking!.IsActive);
        }

        [Then("the booking should be assigned to an available room")]
        public void ThenTheBookingShouldBeAssignedToAnAvailableRoom()
        {
            Assert.Contains(_ctx.Rooms, r => r.Id == _ctx.LastBooking!.RoomId);

            var overlaps = _ctx.Bookings.Where(b =>
                b.IsActive &&
                b.RoomId == _ctx.LastBooking!.RoomId &&
                !(_ctx.LastBooking.EndDate < b.StartDate || _ctx.LastBooking.StartDate > b.EndDate));

            Assert.Empty(overlaps);
        }

        [Then("an ArgumentException should be thrown")]
        public void ThenAnArgumentExceptionShouldBeThrown()
        {
            Assert.NotNull(_ctx.CaughtException);
            Assert.IsType<ArgumentException>(_ctx.CaughtException);
        }
    }
}

