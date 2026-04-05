using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBooking.BddTests.Context;

public class BookingTestState
{
    public string? LastCreatedCustomerName { get; set; }
    public DateTime? LastStartDate { get; set; }
    public DateTime? LastEndDate { get; set; }

    public bool BookingCreated { get; set; }
    public string? LastErrorMessage { get; set; }
}
