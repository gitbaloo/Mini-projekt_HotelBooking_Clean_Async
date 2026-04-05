using HotelBooking.BddTests.Services;
using Reqnroll;

namespace HotelBooking.BddTests.Bindings;

[Binding]
public class NavigationSteps
{
    private readonly PlaywrightDriver _driver;

    public NavigationSteps(PlaywrightDriver driver)
    {
        _driver = driver;
    }

    [Given("the booking form is open")]
    public async Task GivenTheBookingFormIsOpen()
    {
        await _driver.Page!.GotoAsync("https://localhost:44360/Bookings/Create");
    }
}