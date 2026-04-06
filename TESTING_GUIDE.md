# Create Booking Feature - Testing Documentation

## Implementeret i `feature/part2-John`

Dette dokument beskriver implementeringen af **Create Booking** feature-testen med tre testing-tilgange: black-box test cases, Cucumber (BDD) tests, og Postman API tests.

---

## 1. Test Cases Dokumentation

**Fil:** `docs/TestCases_CreateBooking.md`

Indeholder:
- **Equivalence Partitioning** (5 partitioner)
- **Boundary Value Analysis** (11 test cases)
- **Decision Table** (5 test combinations)
- **Additional Test Cases** (4 edge cases)

### Hovedfund:
- StartDate SKAL være i fremtiden (> Today)
- StartDate SKAL være ≤ EndDate
- BookingManager tjekker alle aktive bookinger for overlaps
- Hvis alle rum er optaget i perioden → **false** (booking mislykkedes)
- Hvis der er et ledigt rum → **true** + RoomId bliver sat

---

## 2. Cucumber Testing (Reqnroll/BDD)

**Projektmappe:** `HotelBooking.CucumberTests/`

### Setup:
```bash
cd HotelBooking.CucumberTests
dotnet restore
dotnet build
dotnet test
```

### Struktur:
```
HotelBooking.CucumberTests/
├── Features/
│   ├── CreateBooking.feature          (14 scenarier)
│   └── FullyOccupiedDates.feature     (4 scenarier - optional)
├── StepDefinitions/
│   ├── CreateBookingSteps.cs          (When/Then for Create Booking)
│   ├── FullyOccupiedDatesSteps.cs     (When/Then for Fully Occupied)
│   └── SharedSteps.cs                 (Given steps - fælles)
└── Support/
    └── BookingContext.cs              (Delt state via DI)
```

### Teknologi:
- **Reqnroll 3.0** (SpecFlow's officielle efterfølger)
- **xUnit** test framework
- **Moq** for mocking af repositories
- Cucumber Expressions (`{string}`, `{int}`)

### Test Resultater:
```
Test Run Successful.
Total tests: 18
     Passed: 18
```

### Kør testene:
```bash
# I projektmappe
dotnet test

# Med output
dotnet test --verbosity normal
```

---

## 3. Postman API Testing

**Fil:** `Postman/HotelBooking_CreateBooking_Tests.postman_collection.json`

### Forudsætninger:
1. Docker skal være installeret
2. WebAPI skal køre: `docker-compose up`

### Kør med Postman GUI:
1. Åbn Postman
2. Import collection: `Postman/HotelBooking_CreateBooking_Tests.postman_collection.json`
3. Sørg for `baseUrl` = `http://localhost:5001`
4. Kør hele collection'en

### Kør med Newman CLI:
```bash
# Installer newman (kræver Node.js/npm)
npm install -g newman

# Kør tests
newman run Postman/HotelBooking_CreateBooking_Tests.postman_collection.json

# Med HTML-rapport
newman run Postman/HotelBooking_CreateBooking_Tests.postman_collection.json \
  --reporters cli,html \
  --reporter-html-export postman-results.html
```

### Test Cases i Postman:

| # | Test Case | Forventet | Endpoint |
|---|-----------|-----------|----------|
| Setup | Verify Rooms & Customers | 200 | GET /rooms, GET /customers |
| TC1 | Valid booking (30 days ahead) | 201 Created | POST /Bookings |
| TC2 | Book remaining rooms (same period) | 201 or 409 | POST /Bookings |
| TC2b | Book remaining rooms (again) | 201 or 409 | POST /Bookings |
| TC3 | No rooms available (all booked) | 409 Conflict | POST /Bookings |
| TC4 | Start date in the past | 400/500 | POST /Bookings |
| TC5 | Start date = today | 400/500 | POST /Bookings |
| TC6 | Start date > end date | 400/500 | POST /Bookings |
| TC7 | Empty body | 400 | POST /Bookings |
| TC8 | Single-day booking (60 days ahead) | 201 Created | POST /Bookings |
| TC9 | Verify bookings persisted | 200 + array | GET /Bookings |

### Features:
- ✅ Dynamiske datoer via Pre-request Scripts
- ✅ Assertions på status codes
- ✅ JSON response validering
- ✅ Error message checks
- ✅ Test flow sekvens (Setup → Tests → Verify)

---

## 4. Eksisterende Tests (Uskadt)

```
HotelBooking.UnitTests/
└── BookingManagerTests.cs          (24 tests - alle passerer ✅)

HotelBooking.IntegrationTests/
└── BookingManagerTests.cs          (integration tests)
```

**Kør unit tests:**
```bash
dotnet test HotelBooking.UnitTests
```

---

## 5. Docker Setup

**docker-compose.yml** starter WebAPI og MVC:

```bash
# Start services
docker-compose up -d

# Check logs
docker-compose logs -f webapi

# Stop services
docker-compose down
```

**API URL:** `http://localhost:5001`  
**MVC URL:** `http://localhost:5000`

---

## 6. Branch Information

**Branch:** `feature/part2-John`

### Filer tilføjet/ændret:
```
HotelBooking.CucumberTests/              (nyt projekt)
├── Features/
├── StepDefinitions/
├── Support/
└── HotelBooking.CucumberTests.csproj

Postman/
└── HotelBooking_CreateBooking_Tests.postman_collection.json  (ny)

docs/
└── TestCases_CreateBooking.md           (ny)

HotelBooking.sln                          (updated)
```

---

## 7. Workflow til Manuel Testing

### Step 1: Start Docker
```bash
docker-compose up -d
# Vent 10-15 sekunder for at services starter
```

### Step 2: Kør Cucumber Tests
```bash
dotnet test HotelBooking.CucumberTests/HotelBooking.CucumberTests.csproj
```

### Step 3: Kør Unit Tests
```bash
dotnet test HotelBooking.UnitTests/HotelBooking.UnitTests.csproj
```

### Step 4: Kør Postman Tests (med Postman GUI)
1. Åbn Postman
2. Import collection fra `Postman/HotelBooking_CreateBooking_Tests.postman_collection.json`
3. Sørg for baseUrl = `http://localhost:5001`
4. Klik "Run" for hele collection'en

### Step 5: Stop Docker
```bash
docker-compose down
```

---

## 8. Vigtige Konfigurationer

### BookingManager Rules (i `HotelBooking.Core/Services/BookingManager.cs`):
```csharp
// FindAvailableRoom validering:
if (startDate <= DateTime.Today || startDate > endDate)
    throw new ArgumentException("...");

// Room selection:
// Room er disponibel hvis der INGEN aktive bookings overlapper
// Hvis alle rum har overlaps → return -1
// CreateBooking checker: if (roomId >= 0) → booking lykkes
```

### DbInitializer Data:
```csharp
// 3 rooms (A, B, C)
// Alle 3 rooms booked Today+4 til Today+18 (periode: Today+4 til Today+14)
// = ALLE rum er fuldt optaget fra Today+10 til Today+20 (ish)
```

---

## 9. Test Coverage

### Black-Box Coverage:
- ✅ Equivalence Partitions: 5/5
- ✅ Boundary Values: 11/11
- ✅ Decision Table: 5/5
- ✅ Additional Cases: 4/4

### Cucumber Coverage:
- ✅ Valid booking scenarios: 4
- ✅ Fully occupied scenarios: 7
- ✅ Invalid input scenarios: 4
- ✅ Error handling: 3
- **Total: 18 scenarios, 18 passing**

### Postman Coverage:
- ✅ Positive path: 1-2, 8
- ✅ Boundary path: 2b, 3
- ✅ Negative path: 4-7
- ✅ Verification: 9
- **Total: 9 requests**

---

## 10. Troubleshooting

### "Newman not found"
```bash
# Node.js/npm ikke installeret?
# Brug i stedet Postman GUI eller dotnet test
```

### "Port 5001 in use"
```bash
# Anden process bruger port 5001
docker-compose ps
docker-compose down
# Eller find proces: netstat -ano | findstr :5001
```

### "Test failures in Postman"
```bash
# Bekræft Docker kører
docker-compose ps

# Bekræft baseUrl = http://localhost:5001
# (ikke https!)

# Check WebAPI logs
docker-compose logs webapi
```

### "Cucumber tests fail"
```bash
# Ensure Reqnroll is installed
dotnet restore HotelBooking.CucumberTests

# Rebuild
dotnet build HotelBooking.CucumberTests

# Run with verbose output
dotnet test HotelBooking.CucumberTests --verbosity normal
```

---

## Resumé

✅ **Implementeret:**
1. Black-box test cases (Equivalence Partitioning + Boundary Analysis)
2. Cucumber/BDD tests med Reqnroll (18 scenarios)
3. Postman API tests (9 requests med assertions)
4. Docker-based API testing setup
5. Test dokumentation

✅ **Kvalitet:**
- 18/18 Cucumber tests passing
- 24/24 Unit tests passing
- Hele solution bygger uden fejl
- Alle eksisterende tests uskadt

✅ **Branches:**
- Alt implementeret i `feature/part2-John`

