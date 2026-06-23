namespace GlassCollectionApi.Services;

public record RouteNode(string Id, double Latitude, double Longitude, bool IsDepot = false);

public record OptimizedRoute(IReadOnlyList<string> SupplierOrder, double TotalDistanceKm);

public class RouteOptimizerService
{
    /// <summary>
    /// Dijkstra's algorithm — shortest distances from source to all reachable nodes.
    /// </summary>
    public static double[] Dijkstra(double[,] graph, int source, int nodeCount)
    {
        var distances = Enumerable.Repeat(double.PositiveInfinity, nodeCount).ToArray();
        var visited = new bool[nodeCount];
        distances[source] = 0;

        for (var i = 0; i < nodeCount; i++)
        {
            var u = -1;
            var minDist = double.PositiveInfinity;
            for (var v = 0; v < nodeCount; v++)
            {
                if (!visited[v] && distances[v] < minDist)
                {
                    minDist = distances[v];
                    u = v;
                }
            }

            if (u == -1)
                break;

            visited[u] = true;

            for (var v = 0; v < nodeCount; v++)
            {
                if (!visited[v] && graph[u, v] < double.PositiveInfinity)
                {
                    var alt = distances[u] + graph[u, v];
                    if (alt < distances[v])
                        distances[v] = alt;
                }
            }
        }

        return distances;
    }

    /// <summary>
    /// Builds a visit order from the depot using repeated Dijkstra runs to pick the nearest unvisited supplier.
    /// </summary>
    public OptimizedRoute OptimizeRoute(double startLat, double startLon, IReadOnlyList<RouteNode> suppliers)
    {
        if (suppliers.Count == 0)
            return new OptimizedRoute(Array.Empty<string>(), 0);

        var nodes = new List<RouteNode> { new("DEPOT", startLat, startLon, true) };
        nodes.AddRange(suppliers);

        var nodeCount = nodes.Count;
        var graph = BuildDistanceGraph(nodes);

        var unvisited = Enumerable.Range(1, suppliers.Count).ToHashSet();
        var order = new List<string>();
        var totalDistance = 0.0;
        var current = 0;

        while (unvisited.Count > 0)
        {
            var distances = Dijkstra(graph, current, nodeCount);
            var next = unvisited.MinBy(n => distances[n])!;

            totalDistance += distances[next];
            order.Add(nodes[next].Id);
            unvisited.Remove(next);
            current = next;
        }

        return new OptimizedRoute(order, Math.Round(totalDistance, 2));
    }

    private static double[,] BuildDistanceGraph(IReadOnlyList<RouteNode> nodes)
    {
        var count = nodes.Count;
        var graph = new double[count, count];

        for (var i = 0; i < count; i++)
        {
            for (var j = 0; j < count; j++)
            {
                if (i == j)
                {
                    graph[i, j] = 0;
                    continue;
                }

                graph[i, j] = GeoService.HaversineKm(
                    nodes[i].Latitude, nodes[i].Longitude,
                    nodes[j].Latitude, nodes[j].Longitude);
            }
        }

        return graph;
    }
}
