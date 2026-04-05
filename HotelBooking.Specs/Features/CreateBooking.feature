Feature: Create Booking
  As a hotel receptionist
  I want to create bookings for customers
  So that rooms are reserved for their stay

  Background:
    Given the hotel has the following rooms:
      | Id | Description |
      | 1  | Room A      |
      | 2  | Room B      |
      | 3  | Room C      |
    And the following active bookings exist:
      | Id | StartDate   | EndDate     | CustomerId | RoomId |
      | 1  | Today+1     | Today+1     | 1          | 1      |
      | 2  | Today+10    | Today+20    | 1          | 1      |
      | 3  | Today+10    | Today+20    | 2          | 2      |
      | 4  | Today+10    | Today+20    | 2          | 3      |

  # -----------------------------------------------------------------------
  # Happy path: booking created successfully
  # -----------------------------------------------------------------------

  # BB-05
  Scenario: Booking is created on a single available day
    Given a customer wants to book from "Today+5" to "Today+5"
    When the booking is submitted
    Then the booking should be created successfully
    And the booking should be marked as active

  # BB-07
  Scenario: Booking is created when a room is available
    Given a customer wants to book from "Today+2" to "Today+8"
    When the booking is submitted
    Then the booking should be created successfully
    And the booking should be marked as active
    And the booking should be assigned a valid room

  # BB-09
  Scenario: Booking is created just before the fully occupied period
    Given a customer wants to book from "Today+2" to "Today+9"
    When the booking is submitted
    Then the booking should be created successfully
    And the booking should be marked as active

  # BB-10
  Scenario: Booking is created just after the fully occupied period
    Given a customer wants to book from "Today+21" to "Today+25"
    When the booking is submitted
    Then the booking should be created successfully
    And the booking should be marked as active

  # -----------------------------------------------------------------------
  # No-room path: booking rejected
  # -----------------------------------------------------------------------

  # BB-04
   Scenario: Booking is rejected when start date is today
    Given a customer wants to book from "Today" to "Today+1"
    When the booking is submitted
    Then the booking should not be created
    And the booking should not be marked as active
    And no room should be assigned to the booking

  # BB-08
  Scenario: Booking is rejected when all rooms are occupied
    Given a customer wants to book from "Today+10" to "Today+11"
    When the booking is submitted
    Then the booking should not be created
    And the booking should not be marked as active
    And no room should be assigned to the booking

  # BB-11
  Scenario: Booking is rejected when start date is inside the fully occupied period
    Given a customer wants to book from "Today+15" to "Today+25"
    When the booking is submitted
    Then the booking should not be created

  # BB-12
  Scenario: Booking is rejected when the range encompasses the fully occupied period
    Given a customer wants to book from "Today+8" to "Today+25"
    When the booking is submitted
    Then the booking should not be created

  # -----------------------------------------------------------------------
  # Input validation: invalid dates throw an exception
  # -----------------------------------------------------------------------
  
  # BB-01
  Scenario: Booking throws exception when start date is missing
    Given a customer has accidentally entered "NULL" by leaving out start date
    When the booking is submitted
    Then an ArgumentException is thrown

  # BB-02
  Scenario: Booking throws exception when end date is missing
    Given a customer has accidentally entered "NULL" by leaving out end date
    When the booking is submitted
    Then an ArgumentException is thrown
    
  # BB-03
  Scenario Outline: Booking throws exception when start date is not in the future
    Given a customer wants to book from "<StartDate>" to "Today+2"
    When the booking is submitted
    Then an ArgumentException should be thrown

    Examples:
      | StartDate |
      | Today-10  |
      | Today-1   |
      | Today     |

  # BB-06
  Scenario Outline: Booking throws exception when start date is after end date
    Given a customer wants to book from "<StartDate>" to "<EndDate>"
    When the booking is submitted
    Then an ArgumentException should be thrown

    Examples:
      | StartDate | EndDate  |
      | Today+10  | Today+1  |
      | Today+5   | Today+4  |

  # -----------------------------------------------------------------------
  # No rooms in the hotel
  # -----------------------------------------------------------------------

  # BB-13
  Scenario: Booking is rejected when hotel has no rooms
    Given the hotel has no rooms
    And a customer wants to book from "Today+1" to "Today+2"
    When the booking is submitted
    Then the booking should not be created
