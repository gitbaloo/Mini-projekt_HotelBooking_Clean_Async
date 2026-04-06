using System;
using System.Collections.Generic;
using HotelBooking.Core;
using Moq;

namespace HotelBooking.CucumberTests.Support
{
    /// <summary>
    /// Shared state injected by Reqnroll into all step definition classes for a scenario.
    /// </summary>
    public class BookingContext
    {
        public Mock<IRepository<Booking>> MockBookingRepo { get; } = new();
        public Mock<IRepository<Room>> MockRoomRepo { get; } = new();
        public IBookingManager BookingManager { get; set; } = null!;

        public List<Room> Rooms { get; set; } = new();
        public List<Booking> Bookings { get; set; } = new();

        public bool BookingResult { get; set; }
        public Booking? LastBooking { get; set; }
        public Exception? CaughtException { get; set; }
        public List<DateTime>? FullyOccupiedResult { get; set; }

        public static DateTime ParseRelativeDate(string input)
        {
            input = input.Trim().ToLower();
            if (input == "today")
                return DateTime.Today;

            if (input.StartsWith("today+"))
                return DateTime.Today.AddDays(int.Parse(input.Replace("today+", "")));

            if (input.StartsWith("today-"))
                return DateTime.Today.AddDays(-int.Parse(input.Replace("today-", "")));

            throw new ArgumentException($"Cannot parse relative date: {input}");
        }
    }
}

