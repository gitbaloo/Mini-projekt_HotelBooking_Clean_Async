using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.CucumberTests.Support;
using Reqnroll;
using Xunit;

namespace HotelBooking.CucumberTests.StepDefinitions
{
    [Binding]
    public class FullyOccupiedDatesSteps
    {
        private readonly BookingContext _ctx;

        public FullyOccupiedDatesSteps(BookingContext ctx)
        {
            _ctx = ctx;
        }

        [When("I request fully occupied dates from {string} to {string}")]
        public async Task WhenIRequestFullyOccupiedDatesFromTo(string startExpr, string endExpr)
        {
            _ctx.CaughtException = null;
            try
            {
                _ctx.FullyOccupiedResult = await _ctx.BookingManager.GetFullyOccupiedDates(
                    BookingContext.ParseRelativeDate(startExpr),
                    BookingContext.ParseRelativeDate(endExpr));
            }
            catch (Exception ex)
            {
                _ctx.CaughtException = ex;
            }
        }

        [Then("{int} fully occupied dates should be returned")]
        public void ThenFullyOccupiedDatesShouldBeReturned(int expectedCount)
        {
            Assert.Null(_ctx.CaughtException);
            Assert.NotNull(_ctx.FullyOccupiedResult);
            Assert.Equal(expectedCount, _ctx.FullyOccupiedResult!.Count);
        }

        [Then("all dates from {string} to {string} should be in the result")]
        public void ThenAllDatesFromToShouldBeInTheResult(string startExpr, string endExpr)
        {
            var start = BookingContext.ParseRelativeDate(startExpr);
            var end = BookingContext.ParseRelativeDate(endExpr);

            for (var d = start; d <= end; d = d.AddDays(1))
            {
                Assert.Contains(d, _ctx.FullyOccupiedResult!);
            }
        }

        [Then("an ArgumentException should be thrown for fully occupied dates")]
        public void ThenAnArgumentExceptionShouldBeThrownForFullyOccupiedDates()
        {
            Assert.NotNull(_ctx.CaughtException);
            Assert.IsType<ArgumentException>(_ctx.CaughtException);
        }
    }
}

