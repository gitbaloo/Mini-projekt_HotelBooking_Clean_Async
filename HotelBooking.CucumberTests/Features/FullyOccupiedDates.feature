Feature: Fully Occupied Dates
  As a hotel manager
  I want to see which dates are fully occupied
  So that I can inform customers about availability

  Background:
    Given the hotel has 3 rooms
    And room 1 is booked from "today+10" to "today+20"
    And room 2 is booked from "today+10" to "today+20"
    And room 3 is booked from "today+10" to "today+20"

  Scenario: All dates in the occupied period are returned as fully occupied
    When I request fully occupied dates from "today+10" to "today+20"
    Then 11 fully occupied dates should be returned
    And all dates from "today+10" to "today+20" should be in the result

  Scenario: No fully occupied dates outside the booked period
    When I request fully occupied dates from "today+1" to "today+9"
    Then 0 fully occupied dates should be returned

  Scenario: Partially overlapping query returns only occupied dates
    When I request fully occupied dates from "today+8" to "today+12"
    Then 3 fully occupied dates should be returned
    And all dates from "today+10" to "today+12" should be in the result

  Scenario: Invalid date range throws exception
    When I request fully occupied dates from "today+20" to "today+10"
    Then an ArgumentException should be thrown for fully occupied dates

