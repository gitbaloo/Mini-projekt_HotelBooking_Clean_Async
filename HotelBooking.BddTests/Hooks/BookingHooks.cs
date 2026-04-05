using HotelBooking.BddTests.Services;
using Reqnroll;

namespace HotelBooking.BddTests.Hooks;

[Binding]
public class BookingHooks
{
    private readonly DatabaseResetService _resetService;
    private readonly DatabaseSeedService _seedService;
    private readonly PlaywrightDriver _driver;

    public BookingHooks(
        DatabaseResetService resetService,
        DatabaseSeedService seedService,
        PlaywrightDriver driver)
    {
        _resetService = resetService;
        _seedService = seedService;
        _driver = driver;
    }

    [BeforeScenario("@ui", Order = 0)]
    public async Task ResetDatabaseAsync()
    {
        await _resetService.ResetAsync();
    }

    [BeforeScenario("@ui", Order = 10)]
    public async Task SeedDatabaseAsync()
    {
        await _seedService.SeedBaselineAsync();
    }

    [BeforeScenario("@ui", Order = 20)]
    public async Task StartBrowserAsync()
    {
        await _driver.StartAsync();
    }

    [AfterScenario("@ui", Order = 100)]
    public async Task StopBrowserAsync()
    {
        await _driver.StopAsync();
    }
}