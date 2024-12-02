class BeeAlgorithm
{
    const int N = 300;
    const int K_F = 40;
    const int K_R = 4;
    const int generation_probability = 3;
    const int MaxIterations = 1000;
    const long Infinity = long.MaxValue;

    static Random random = new Random();
    static long[,] graph = new long[N, N];

    static void InitializeGraph()
    {
        for (int i = 0; i < N; i++)
        {
            int edges = 0;
            for (int j = i + 1; j < N; j++)
            {
                if (edges < 10 && random.Next(0, 10) < generation_probability)
                {
                    graph[i, j] = random.Next(5, 151);
                    graph[j, i] = graph[i, j];
                    edges++;
                }
                else
                {
                    graph[i, j] = Infinity;
                }
            }
            if (edges == 0)
            {
                int neighbor = random.Next(0, N);
                while (neighbor == i)
                {
                    neighbor = random.Next(0, N);
                }

                graph[i, neighbor] = random.Next(5, 151);
                graph[neighbor, i] = graph[i, neighbor];
            }
        }
    }

    static (List<int> Path, long Distance) BeeOptimization(int start, int end)
    {
        List<int> bestPath = null;
        long bestDistance = Infinity;

        for (int iteration = 0; iteration < MaxIterations; iteration++)
        {
            List<(List<int>, long)> scoutRoutes = GenerateRandomRoutes(start, end, K_R);
            List<(List<int>, long)> forageRoutes = ExploitRoutes(scoutRoutes, K_F);

            foreach (var route in forageRoutes)
            {
                if (route.Item2 < bestDistance)
                {
                    bestDistance = route.Item2;
                    bestPath = route.Item1;
                }
            }
        }

        return (bestPath, bestDistance);
    }

    static List<(List<int>, long)> GenerateRandomRoutes(int start, int end, int count)
    {
        var routes = new List<(List<int>, long)>();

        for (int i = 0; i < count; i++)
        {
            List<int> route = new List<int> { start };
            int current = start;

            while (current != end)
            {
                var neighbors = Enumerable.Range(0, N)
                                          .Where(v => graph[current, v] != Infinity && !route.Contains(v))
                                          .ToList();
                if (neighbors.Count == 0) break;
                current = neighbors[random.Next(neighbors.Count)];
                route.Add(current);
            }

            if (current == end)
            {
                long distance = CalculateDistance(route);
                routes.Add((route, distance));
            }
        }

        return routes;
    }

    static List<(List<int>, long)> ExploitRoutes(List<(List<int>, long)> scoutRoutes, int count)
    {
        var forageRoutes = new List<(List<int>, long)>();

        foreach (var (route, distance) in scoutRoutes)
        {
            List<int> bestLocalRoute = new List<int>(route);
            long bestLocalDistance = distance;

            for (int i = 0; i < count; i++)
            {
                var newRoute = MutateRoute(route);
                long newDistance = CalculateDistance(newRoute);

                if (newDistance < bestLocalDistance)
                {
                    bestLocalRoute = newRoute;
                    bestLocalDistance = newDistance;
                }
            }

            forageRoutes.Add((bestLocalRoute, bestLocalDistance));
        }

        return forageRoutes;
    }

    static List<int> MutateRoute(List<int> route)
    {
        var mutatedRoute = new List<int>(route);
        int index = random.Next(1, route.Count - 1);
        var neighbors = Enumerable.Range(0, N)
                                  .Where(v => graph[route[index - 1], v] != Infinity && !route.Contains(v))
                                  .ToList();

        if (neighbors.Count > 0)
        {
            int bestNeighbor = neighbors.OrderBy(v => graph[route[index - 1], v]).First();

            if (graph[bestNeighbor, route[index + 1]] != Infinity)
            {
                mutatedRoute[index] = bestNeighbor;

                long oldDistance = CalculateDistance(route);
                long newDistance = CalculateDistance(mutatedRoute);

                if (newDistance < oldDistance && newDistance != Infinity)
                {
                    return mutatedRoute;
                }
                else
                {
                    mutatedRoute[index] = route[index];
                }
            }
        }
        return mutatedRoute;
    }

    static long CalculateDistance(List<int> route)
    {
        long distance = 0;
        for (int i = 0; i < route.Count - 1; i++)
        {
            if (graph[route[i], route[i + 1]] == Infinity)
                return Infinity;
            distance += graph[route[i], route[i + 1]];
            if (distance < 0)
                return Infinity;
        }
        return distance;
    }

    static void Main()
    {
        InitializeGraph();
        var bestPath = BeeOptimization(0, N - 1);
        Console.WriteLine("Найкоротший шлях:");
        Console.WriteLine(string.Join(" -> ", bestPath.Path));
        Console.WriteLine($"Довжина шляху: {bestPath.Distance}");
    }
}
