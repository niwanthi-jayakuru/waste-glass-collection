# Database Schema

## Entity Relationship

```
Supplier (1) ──< (N) TripStop >── (1) Trip
```

## Tables

### Suppliers

| Column | Type | Notes |
|--------|------|-------|
| Id | string (PK) | Barcode value, e.g. `SUP001` |
| Name | string | Display name |
| Address | string | Optional address line |
| Latitude | double | GPS |
| Longitude | double | GPS |
| ExpectedClearKg | decimal | Expected clear glass |
| ExpectedColouredKg | decimal | Expected coloured glass |

### Trips

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | Auto-increment |
| Date | date | Trip date (UTC date) |
| StartLatitude | double | Collector depot / start |
| StartLongitude | double | Collector depot / start |
| TotalDistanceKm | decimal | Set after route calculation |
| StartedAt | datetime | Trip start |
| CompletedAt | datetime? | Null until finished |

### TripStops

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | Auto-increment |
| TripId | int (FK) | |
| SupplierId | string (FK) | |
| SequenceOrder | int | 1-based visit order |
| Status | enum | `Pending`, `Next`, `Collected` |
| CollectedClearKg | decimal? | Filled on collection |
| CollectedColouredKg | decimal? | Filled on collection |
| Condition | string? | e.g. Good, Damaged |
| CollectedAt | datetime? | When collected |

Unique index: `(TripId, SupplierId)`, `(TripId, SequenceOrder)`.

## Seed Suppliers (London area)

| ID | Name | Lat | Lng | Expected Clear | Expected Coloured |
|----|------|-----|-----|----------------|-------------------|
| SUP001 | Green Valley Cafe | 51.5074 | -0.1278 | 50 | 30 |
| SUP002 | Riverside Pub | 51.5155 | -0.0923 | 40 | 25 |
| SUP003 | Sunny Supermarket | 51.5033 | -0.1195 | 80 | 45 |
| SUP004 | Oak Street Bistro | 51.5120 | -0.1440 | 35 | 20 |
| SUP005 | City Glass Depot | 51.4980 | -0.1050 | 60 | 35 |

Collector start (trip): **51.5050, -0.1200** (central London).

## Barcode Testing

1. Open https://barcode.tec-it.com
2. Select **Code 128**
3. Enter `SUP001` (then repeat for each ID)
4. Download/display on a second device for scanning

The app decodes the barcode to `supplierId` and must match the current **Next** stop before unlocking the form.
