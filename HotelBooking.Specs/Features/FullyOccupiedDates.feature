Feature: Fully Occupied Dates
  As a hotel manager
  I want to know which dates are fully occupied
  So that I can inform customers about availability

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
  # Core correctness: fully occupied period is identified correctly
  # -----------------------------------------------------------------------

  Scenario: Fully occupied dates are identified when query range spans the occupied block
    Given I query occupied dates from "Today+9" to "Today+25"
    When I request the fully occupied dates
    Then the result should contain exactly 11 dates
    And all dates from "Today+10" to "Today+20" should be in the result

  Scenario: Exact boundary dates of the occupied block are included
    Given I query occupied dates from "Today+10" to "Today+20"
    When I request the fully occupied dates
    Then the result should contain exactly 11 dates
    And all dates from "Today+10" to "Today+20" should be in the result

  Scenario: Only dates where ALL rooms are booked appear in the result
    Given I query occupied dates from "Today+1" to "Today+3"
    When I request the fully occupied dates
    Then the result should contain exactly 0 dates

  Scenario: Single date inside fully occupied period is identified correctly
    Given I query occupied dates from "Today+15" to "Today+15"
    When I request the fully occupied dates
    Then the result should contain exactly 1 dates

  Scenario: Single date outside fully occupied period is not flagged
    Given I query occupied dates from "Today+5" to "Today+5"
    When I request the fully occupied dates
    Then the result should contain exactly 0 dates

  Scenario: Query range entirely before the occupied block returns no dates
    Given I query occupied dates from "Today+1" to "Today+9"
    When I request the fully occupied dates
    Then the result should contain exactly 0 dates

  Scenario: Query range entirely after the occupied block returns no dates
    Given I query occupied dates from "Today+21" to "Today+30"
    When I request the fully occupied dates
    Then the result should contain exactly 0 dates

  # -----------------------------------------------------------------------
  # Edge case: start date equals end date (single day)
  # -----------------------------------------------------------------------

  Scenario: Same-day range on fully occupied date returns that date
    Given I query occupied dates from "Today+10" to "Today+10"
    When I request the fully occupied dates
    Then the result should contain exactly 1 dates

  Scenario: Same-day range on non-occupied date returns empty
    Given I query occupied dates from "Today+1" to "Today+1"
    When I request the fully occupied dates
    Then the result should contain exactly 0 dates

  # -----------------------------------------------------------------------
  # Edge case: no bookings at all
  # -----------------------------------------------------------------------

  Scenario: No bookings means no fully occupied dates
    Given there are no bookings in the system
    And I query occupied dates from "Today+1" to "Today+30"
    When I request the fully occupied dates
    Then the result should contain exactly 0 dates

  # -----------------------------------------------------------------------
  # Invalid input: start after end
  # -----------------------------------------------------------------------

  Scenario: Querying with start date after end date throws an exception
    Given I query occupied dates from "Today+5" to "Today+2"
    When I request the fully occupied dates
    Then an ArgumentException should be thrown
