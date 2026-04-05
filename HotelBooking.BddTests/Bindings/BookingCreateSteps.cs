using HotelBooking.BddTests.Services;
using Microsoft.Playwright;
using Reqnroll;
using System.Text.RegularExpressions;
using Xunit;

namespace HotelBooking.BddTests.Bindings;

[Binding]
public class BookingCreateSteps
{
    private readonly PlaywrightDriver _driver;

    public BookingCreateSteps(PlaywrightDriver driver)
    {
        _driver = driver;
    }

    [When(@"the user submits a booking without a start date and with end date ""(.*)""")]
    public async Task WhenTheUserSubmitsABookingWithoutAStartDateAndWithEndDate(string endDate)
    {
        var page = _driver.Page!;

        await page.FillAsync("#EndDate", endDate);
        await page.SelectOptionAsync("#CustomerId", new[] { "1" });
        await page.ClickAsync("input[type='submit']");
    }

    [Then("the booking is not created")]
    public async Task ThenTheBookingIsNotCreated()
    {
        var page = _driver.Page!;

        await Assertions.Expect(page).ToHaveURLAsync(new Regex(".*/Bookings/Create$"));
    }

    [Then(@"the user sees ""(.*)"" below the start date field")]
    public async Task ThenTheUserSeesBelowTheStartDateField(string expectedMessage)
    {
        var page = _driver.Page!;

        var validation = page.Locator("span[data-valmsg-for='StartDate']");
        await Assertions.Expect(validation).ToHaveTextAsync(expectedMessage);
    }
}