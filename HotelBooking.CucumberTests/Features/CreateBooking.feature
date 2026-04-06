Feature: Create Booking
  As a hotel manager
  I want to create bookings for hotel rooms
  So that customers can reserve rooms for specific periods

  Background:
    Given the hotel has 3 rooms
    And room 1 is booked from "today+10" to "today+20"
    And room 2 is booked from "today+10" to "today+20"
    And room 3 is booked from "today+10" to "today+20"

  # --- Equivalence Partition: Valid dates, room available (EP1) ---

  Scenario: Successfully create a booking when a room is available
    When I create a booking from "today+2" to "today+5" for customer 1
    Then the booking should be created successfully
    And the booking should be marked as active
    And the booking should be assigned to an available room

  Scenario: Successfully create a single-day booking
    When I create a booking from "today+5" to "today+5" for customer 1
    Then the booking should be created successfully

  # --- Equivalence Partition: Valid dates, no room available (EP2) ---

  Scenario: Fail to create booking when all rooms are occupied
    When I create a booking from "today+12" to "today+18" for customer 1
    Then the booking should not be created

  # --- Equivalence Partition: StartDate in the past (EP3) ---

  Scenario: Fail to create booking with start date in the past
    When I try to create a booking from "today-5" to "today+5" for customer 1
    Then an ArgumentException should be thrown

  # --- Equivalence Partition: StartDate is today (EP4) ---

  Scenario: Fail to create booking with start date today
    When I try to create a booking from "today" to "today+5" for customer 1
    Then an ArgumentException should be thrown

  # --- Equivalence Partition: StartDate after EndDate (EP5) ---

  Scenario: Fail to create booking with start date after end date
    When I try to create a booking from "today+10" to "today+5" for customer 1
    Then an ArgumentException should be thrown

  # --- Boundary Value Analysis ---

  Scenario: Book the day before the fully occupied period starts
    When I create a booking from "today+1" to "today+9" for customer 1
    Then the booking should be created successfully

  Scenario: Book exactly when the fully occupied period starts
    When I create a booking from "today+10" to "today+10" for customer 1
    Then the booking should not be created

  Scenario: Book exactly when the fully occupied period ends
    When I create a booking from "today+20" to "today+20" for customer 1
    Then the booking should not be created

  Scenario: Book the day after the fully occupied period ends
    When I create a booking from "today+21" to "today+21" for customer 1
    Then the booking should be created successfully

  Scenario: Booking overlaps the start of the occupied period
    When I create a booking from "today+9" to "today+11" for customer 1
    Then the booking should not be created

  Scenario: Booking overlaps the end of the occupied period
    When I create a booking from "today+19" to "today+21" for customer 1
    Then the booking should not be created

  Scenario: Booking encompasses the entire occupied period
    When I create a booking from "today+8" to "today+25" for customer 1
    Then the booking should not be created

  Scenario: StartDate is tomorrow (minimum valid future date)
    When I create a booking from "today+1" to "today+1" for customer 1
    Then the booking should be created successfully

