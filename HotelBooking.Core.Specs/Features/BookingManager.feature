Feature: Create a booking

Create a booking for a hotel room.

Scenario Outline: Create a valid booking for a hotel room
    Given there is an available room
    When I create a booking as a customer with id <customerid>, start date "<startdate>" and end date "<enddate>"
    Then the booking should be created successfully

    Examples:
        | customerid | startdate   | enddate     |
        | 1          | 2027-07-01  | 2027-07-05  |
        | 2          | 2027-08-10  | 2027-08-15  |
        | 3          | 2027-09-20  | 2027-09-25  |

Scenario Outline: Create a booking with invalid start date for a hotel room
    Given there is an available room
    When I create a booking as a customer with id <customerid>, start date "<startdate>" and end date "<enddate>"
    Then an error should be thrown

    Examples:
        | customerid | startdate   | enddate     |
        | 1          | 2027-07-05  | 2027-07-01  |
        | 2          | 2027-08-15  | 2027-08-10  |
        | 3          | 2027-09-25  | 2027-09-20  |

Scenario Outline: Create a booking with invalid end date for a hotel room
    Given there is an available room
    When I create a booking as a customer with id <customerid>, start date "<startdate>" and end date "<enddate>"
    Then an error should be thrown

    Examples:
        | customerid | startdate   | enddate     |
        | 1          | 2027-07-01  | 2027-06-30  |
        | 2          | 2027-08-10  | 2027-08-09  |
        | 3          | 2027-09-20  | 2027-09-19  |

Scenario: Try to create a booking when there are no available rooms
    Given there are no available rooms
    When I create a booking as a customer with id 1, start date "2024-07-01" and end date "2024-07-05"
    Then the booking should not be created