# HotelBooking Cucumber (SpecFlow) Tests

## Overview

This project (`HotelBooking.Specs`) adds **Cucumber-style BDD tests** using
[SpecFlow](https://specflow.org/) — the standard Cucumber implementation for .NET.
Tests are discovered and executed by xUnit.

---

## Prerequisites

| Tool | Version |
|---|---|
| .NET SDK | 8.0 or later |
| NuGet access | Required to restore packages |

---

## Running the tests

```bash
# From the solution root
cd Mini-projekt_HotelBooking_Clean_Async-main

dotnet restore HotelBooking.Specs/HotelBooking.Specs.csproj
dotnet test   HotelBooking.Specs/HotelBooking.Specs.csproj
```

Or run all projects at once if you have a solution file:

```bash
dotnet test
```

---

## Project layout

```
HotelBooking.Specs/
├── Features/
│   ├── CreateBooking.feature          # All CreateBooking scenarios
│   └── FullyOccupiedDates.feature     # All GetFullyOccupiedDates scenarios
├── StepDefinitions/
│   ├── SharedSteps.cs                 # Background steps (rooms, bookings tables)
│   ├── CreateBookingSteps.cs          # Steps for CreateBooking feature
│   └── FullyOccupiedDatesSteps.cs     # Steps for FullyOccupiedDates feature
├── Support/
│   ├── BookingTestContext.cs          # Shared state injected via SpecFlow DI
│   ├── GlobalUsings.cs                # Global 'using' for xunit Assert
│   └── AssemblyConfig.cs             # Disables parallel test execution
└── specflow.json                      # SpecFlow configuration
```

---

## Feature files

### `CreateBooking.feature` — 14 scenarios

| Scenario | Expected outcome |
|---|---|
| Room available (dates 2–8) | Returns `true`, `IsActive = true`, valid room assigned |
| Single available day | Returns `true` |
| Just before fully occupied period | Returns `true` |
| Just after fully occupied period | Returns `true` |
| All rooms occupied (day 10–11) | Returns `false`, `IsActive = false`, no room assigned |
| Start date inside occupied period | Returns `false` |
| Range encompasses occupied period | Returns `false` |
| Range entirely inside occupied period | Returns `false` |
| Start date in past or today (3 inline examples) | Throws `ArgumentException` |
| Start date after end date (2 inline examples) | Throws `ArgumentException` |
| End date before start date (2 inline examples) | Throws `ArgumentException` |
| Hotel has no rooms | Returns `false` |

### `FullyOccupiedDates.feature` — 12 scenarios

| Scenario | Expected outcome |
|---|---|
| Query spans occupied block | 11 dates (Today+10 to Today+20) |
| Exact boundary range | 11 dates |
| Range with only partially booked day (Today+1) | 0 dates |
| Single date inside occupied period | 1 date |
| Single date outside occupied period | 0 dates |
| Range entirely before occupied block | 0 dates |
| Range entirely after occupied block | 0 dates |
| Same-day range on occupied date | 1 date |
| Same-day range on non-occupied date | 0 dates |
| No bookings in system | 0 dates |
| Start date after end date | Throws `ArgumentException` |

### Readability of .feature files

If you are using VS code it is highly recommended using the extension called 'Cucumber (Gherkin) Full Support' by Alexander Krechik.

---

## Data model used in Background tables

The `Background` section in each feature file populates the test data using
SpecFlow tables.  Dates are written as **offset tokens**:

| Token | Meaning |
|---|---|
| `Today+N` | `DateTime.Today.AddDays(N)` |
| `Today-N` | `DateTime.Today.AddDays(-N)` |
| `Today` | `DateTime.Today` |

The parser lives in `BookingTestContext.ParseDate()`.

The shared fixture mirrors the unit-test setup already in
`HotelBooking.UnitTests/BookingManagerTests.cs`:

- **3 rooms** (Room A / B / C, IDs 1–3)  
- **Room 1** booked on `Today+1` (single day)  
- **All 3 rooms** booked from `Today+10` to `Today+20` (fully occupied block)
