@ui @current_behavior
Feature: Create booking in the current MVC application
  In order to document the delivered MVC behavior
  As a tester
  I want to capture what the application does today, including defects

  Background:
    Given the booking form is open

  Rule: Missing required dates are validated on the page

    @bb01
    Scenario: BB-01 Reject request when the start date is missing
      When the user submits a booking without a start date and with end date "2026-04-02"
      Then the booking is not created
      And the user sees "The StartDate field is required." below the start date field

    @bb02
    Scenario: BB-02 Reject request when the end date is missing
      When the user submits a booking with start date "2026-04-01" and without an end date
      Then the booking is not created
      And the user sees "The EndDate field is required." below the end date field

  Rule: Invalid date input currently causes an unhandled exception

    @known_bug @bb03
    Scenario: BB-03 Reject request when the start date is before today
      When the user submits a booking from a past date to a future date
      Then the application crashes with an ArgumentException
      And the exception message is "The start date cannot be in the past or later than the end date."

    @known_bug @bb04
    Scenario: BB-04 Reject request when the booking starts today
      When the user submits a booking that starts today and ends today
      Then the application crashes with an ArgumentException
      And the exception message is "The start date cannot be in the past or later than the end date."

    @known_bug @bb06
    Scenario: BB-06 Reject request when the start date is after the end date
      When the user submits a booking with the start date after the end date
      Then the application crashes with an ArgumentException
      And the exception message is "The start date cannot be in the past or later than the end date."

  Rule: Valid bookings are created when a room is available

    @bb05
    Scenario: BB-05 Accept a future one-day booking when a room is available
      When the user submits a one-day booking outside the fully occupied period for customer "John Smith"
      Then the user is redirected from "/Bookings/Create" to "/Bookings"
      And the new booking appears in the bookings table

    @bb07
    Scenario: BB-07 Accept a future multi-day booking when a room is available
      When the user submits a multi-day booking outside the fully occupied period for customer "Jane Doe"
      Then the user is redirected from "/Bookings/Create" to "/Bookings"
      And the new booking appears in the bookings table

  Rule: Valid dates with no availability show an on-page status message

    @bb08
    Scenario: BB-08 Reject request when dates are valid but no room is available
      When the user submits a booking inside the fully occupied period for customer "John Smith"
      Then the booking is not created
      And the user stays on "/Bookings/Create"
      And the user sees "The booking could not be created. There were no available room."