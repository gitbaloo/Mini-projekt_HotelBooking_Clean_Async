using HotelBooking.BddTests.Context;
using HotelBooking.BddTests.Services;
using HotelBooking.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Reqnroll.Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.BddTests;

public static class TestDependencies
{
    [ScenarioDependencies]
    public static IServiceCollection CreateServices()
    {
        var services = new ServiceCollection();

        services.AddDbContext<HotelBookingContext>(options =>
        {
            options.UseSqlServer(
                @"Server=(localdb)\MSSQLLocalDB;Database=HotelBooking_BddTests;Trusted_Connection=True;MultipleActiveResultSets=true");
        });

        services.AddScoped<BookingTestState>();
        services.AddScoped<DatabaseResetService>();
        services.AddScoped<DatabaseSeedService>();
        services.AddScoped<PlaywrightDriver>();

        return services;
    }
}