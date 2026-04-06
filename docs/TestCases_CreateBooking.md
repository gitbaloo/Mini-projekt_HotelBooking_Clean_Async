# Test Cases: Create Booking

## Black-Box Test Techniques Applied

### Equivalence Partitioning & Boundary Value Analysis

The `CreateBooking` feature relies on `FindAvailableRoom(startDate, endDate)` which has these rules:
- **StartDate** must be in the future (strictly after today)
- **StartDate** must be ≤ **EndDate**
- A room is available if no active booking overlaps the requested period

---

## Equivalence Partitions

| Partition | Description | Expected Result |
|-----------|-------------|-----------------|
| **EP1** – Valid dates, room available | StartDate in future, EndDate ≥ StartDate, at least one room free | Booking created (`true`) |
| **EP2** – Valid dates, no room available | StartDate in future, EndDate ≥ StartDate, all rooms booked in period | Booking NOT created (`false`) |
| **EP3** – StartDate in the past | StartDate < Today | `ArgumentException` |
| **EP4** – StartDate is today | StartDate == Today | `ArgumentException` |
| **EP5** – StartDate after EndDate | StartDate > EndDate | `ArgumentException` |

---

## Boundary Value Analysis

Using the seeded data where all 3 rooms are booked from `Today+10` to `Today+20`:

| # | Test Case | StartDate | EndDate | Expected |
|---|-----------|-----------|---------|----------|
| **BVA1** | Day before occupied period starts | Today+1 | Today+9 | `true` (room available) |
| **BVA2** | Exact start of occupied period | Today+10 | Today+10 | `false` (no room) |
| **BVA3** | Exact end of occupied period | Today+20 | Today+20 | `false` (no room) |
| **BVA4** | Day after occupied period ends | Today+21 | Today+21 | `true` (room available) |
| **BVA5** | Overlap start boundary | Today+9 | Today+11 | `false` (no room) |
| **BVA6** | Overlap end boundary | Today+19 | Today+21 | `false` (no room) |
| **BVA7** | Entire occupied period | Today+10 | Today+20 | `false` (no room) |
| **BVA8** | Encompasses occupied period | Today+8 | Today+25 | `false` (no room) |
| **BVA9** | StartDate = Today (boundary) | Today | Today+5 | `ArgumentException` |
| **BVA10** | StartDate = Tomorrow (boundary) | Today+1 | Today+1 | `true` (room available) |
| **BVA11** | StartDate = EndDate (single day) | Today+5 | Today+5 | `true` (room available) |

---

## Additional Test Cases

| # | Test Case | Description | Expected |
|---|-----------|-------------|----------|
| **AC1** | Partial room availability | Only some rooms booked, not all | `true` (assigns free room) |
| **AC2** | No rooms exist | 0 rooms in system | `false` |
| **AC3** | Inactive bookings ignored | Room has inactive booking in period | `true` (inactive doesn't count) |
| **AC4** | Multiple consecutive bookings | Book same period twice when 2+ rooms available | Both succeed |

---

## Decision Table (Create Booking)

| Condition | TC1 | TC2 | TC3 | TC4 | TC5 |
|-----------|-----|-----|-----|-----|-----|
| StartDate > Today | Y | Y | N | Y | Y |
| StartDate ≤ EndDate | Y | Y | Y | N | Y |
| Room available | Y | N | - | - | Y |
| **Action** | **Created** | **Not created** | **Exception** | **Exception** | **Created** |

