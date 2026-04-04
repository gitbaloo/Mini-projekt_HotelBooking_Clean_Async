using System.Globalization;
using FluentAssertions;
using Moq;

namespace HotelBooking.Core.Specs.StepDefinitions;

[Binding]
public class BookingManagerSteps
{
    private readonly Mock<IRepository<Booking>> mockBookingRepo;
    private readonly Mock<IRepository<Room>> mockRoomRepo;
    private readonly IBookingManager bookingManager;
    private Booking? booking;
    private bool result;
    private Exception? caughtException;
    
    private List<Room> defaultRooms = new();
    private List<Booking> defaultBookings = new();

    public BookingManagerSteps() {
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
        mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(defaultRooms);
        mockBookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(defaultBookings);
    
        // Simulate persistence so later assertions can see created bookings.
        mockBookingRepo
            .Setup(r => r.AddAsync(It.IsAny<Booking>()))
            .Callback<Booking>(b => defaultBookings.Add(b))
            .Returns(Task.CompletedTask);
    }


    [Given(@"there is an available room")]
    public void GivenThereIsAnAvailableRoom()
    {
        result = false;
        caughtException = null;
        booking = null;
        
        defaultRooms.Clear();
        defaultBookings.Clear();

        defaultRooms.Add(new Room
        {
            Id = 1,
            Description = "Room A",
        });
    }

    [Given("there are no available rooms")]
    public void GivenThereAreNoAvailableRooms() {
        result = false;
        caughtException = null;
        booking = null;

        defaultRooms.Clear();
        defaultBookings.Clear();

        defaultRooms.Add(new Room { Id = 1, Description = "Room A" });
        defaultRooms.Add(new Room { Id = 2, Description = "Room B" });

        var start = DateTime.Today.AddDays(1);
        var end = DateTime.Today.AddYears(1);

        defaultBookings.Add(new Booking { Id = 1, RoomId = 1, CustomerId = 1, StartDate = start, EndDate = end, IsActive = true });
        defaultBookings.Add(new Booking { Id = 2, RoomId = 2, CustomerId = 2, StartDate = start, EndDate = end, IsActive = true });
    }
    
    
    [When(@"I create a booking as a customer with id {int}, start date {string} and end date {string}")]
    public async Task WhenBookingIsCreated(int customerId, string startDateText, string endDateText) {

        DateTime startDate = DateTime.ParseExact(startDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        DateTime endDate = DateTime.ParseExact(endDateText,  "yyyy-MM-dd", CultureInfo.InvariantCulture);
        
        booking = new Booking
        {
            CustomerId = customerId,
            StartDate = startDate,
            EndDate = endDate
        };
        
        try
        {
            result = await bookingManager.CreateBooking(booking);
        }
        catch (ArgumentException e)
        {
            caughtException = e;
        }
    }

    [Then(@"the booking should be created successfully")]
    public void ThenBookingShouldBeCreatedSuccessfully() {
        result.Should().BeTrue();
    } 

    [Then(@"an error should be thrown")]
    public void ThenAnErrorShouldBeThrown() {
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType(typeof(ArgumentException));
    }

    [Then(@"the booking should not be created")]
    public void ThenTheBookingShouldNotBeCreated() {
        result.Should().BeFalse();
    }
}
