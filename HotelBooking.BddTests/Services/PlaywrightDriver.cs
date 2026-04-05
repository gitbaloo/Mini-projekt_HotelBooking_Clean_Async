using Microsoft.Playwright;

namespace HotelBooking.BddTests.Services;

public class PlaywrightDriver
{
    public IPlaywright? Playwright { get; private set; }
    public IBrowser? Browser { get; private set; }
    public IBrowserContext? BrowserContext { get; private set; }
    public IPage? Page { get; private set; }

    public async Task StartAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        BrowserContext = await Browser.NewContextAsync();
        Page = await BrowserContext.NewPageAsync();
    }

    public async Task StopAsync()
    {
        if (BrowserContext != null)
            await BrowserContext.CloseAsync();

        if (Browser != null)
            await Browser.CloseAsync();

        Playwright?.Dispose();
    }
}