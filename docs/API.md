# API Reference (Phase 3)

Base URL (local): `http://localhost:5050`

## Endpoints

### `GET /api/trip/today`
Returns today's trip with Dijkstra-optimized stop order, GPS coordinates, statuses, and total route distance.

### `GET /api/trip/today/summary`
Trip report for Screen 3: per-supplier totals, trip duration, combined kg, and shortfall warnings.

### `POST /api/collections`
Submit a collection for the **current next stop only**.

```json
{
  "supplierId": "SUP003",
  "clearKg": 70,
  "colouredKg": 40,
  "condition": "Good",
  "collectedAt": "2026-06-22T10:30:00Z"
}
```

Returns `400` if `supplierId` does not match the expected next stop.

### `POST /api/collections/sync`
Bulk sync from the mobile app (Screen 3).

```json
{
  "collections": [
    {
      "supplierId": "SUP003",
      "clearKg": 70,
      "colouredKg": 40,
      "condition": "Good",
      "collectedAt": "2026-06-22T10:30:00Z"
    }
  ]
}
```

### `GET /api/suppliers`
Lists all suppliers (testing / barcode setup).

### `GET /health`
Health check.

## Route optimization

- **Haversine** (`GeoService`) — edge weights between GPS coordinates (km)
- **Dijkstra** (`RouteOptimizerService`) — from the current node, pick the nearest unvisited supplier; repeat until all stops are ordered
- Total route distance is the sum of legs from depot through each stop
