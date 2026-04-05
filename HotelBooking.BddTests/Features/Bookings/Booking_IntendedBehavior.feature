@ui @intended_behavior
Feature: Create booking
  In order to make a booking request
  As a user
  I want valid bookings to be created and invalid bookings to be rejected clearly

  Background:
    Given the booking form is open

  Rule: Both dates are required

    Scenario: Reject request when the start date is missing
      When the user submits a booking without a start date and with end date "2026-04-02"
      Then the booking is not created
      And the user sees a validation error for the start date

    Scenario: Reject request when the end date is missing
      When the user submits a booking with start date "2026-04-01" and without an end date
      Then the booking is not created
      And the user sees a validation error for the end date

  Rule: The start date must be after today

    Scenario: Reject request when the start date is before today
      When the user submits a booking from a past date to a future date
      Then the booking is not created
      And the user sees a validation error explaining that the start date is invalid

    Scenario: Reject request when the booking starts today
      When the user submits a booking that starts today and ends today
      Then the booking is not created
      And the user sees a validation error explaining that bookings cannot start today

  Rule: The start date must not be after the end date

    Scenario: Reject request when the start date is after the end date
      When the user submits a booking with the start date after the end date
      Then the booking is not created
      And the user sees a validation error explaining that the date range is invalid

  Rule: A valid booking is created when a room is available

    Scenario: Accept a future one-day booking when a room is available
      When the user submits a one-day booking outside the fully occupied period for customer "John Smith"
      Then the user is redirected to the bookings page
      And the new booking appears in the bookings table
      And the booking is active

    Scenario: Accept a future multi-day booking when a room is available
      When the user submits a multi-day booking outside the fully occupied period for customer "Jane Doe"
      Then the user is redirected to the bookings page
      And the new booking appears in the bookings table
      And the booking is active

  Rule: A valid booking is rejected when no room is available for the full requested period

    Scenario: Reject request when dates are valid but no room is available
      When the user submits a booking inside the fully occupied period for customer "John Smith"
      Then the booking is not created
      And the user sees a message that no room is available for the selected period

  Rule: A booking cannot start on the same day another booking ends

    Scenario: Reject request when a booking starts on the day an existing booking ends
      Given room "A" has an active booking that ends tomorrow
      And all other rooms are unavailable for tomorrow and the next day
      When the user submits a booking for customer "Jane Doe" starting tomorrow and ending the next day
      Then the booking is not created
      And the user sees a message that no room is available for the selected period