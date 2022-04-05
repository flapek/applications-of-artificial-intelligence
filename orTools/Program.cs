using CommandLine;
using Google.OrTools.ConstraintSolver;
using orTools;

await Parser.Default.ParseArguments<CommandLineOptions>(args)
    .MapResult(async args =>
    {
        var data = new DataModel
        {
            DistanceMatrix = await ReadFileAsync(args.Path)
        };

        var manager =
            new RoutingIndexManager(data.DistanceMatrix.GetLength(0), DataModel.VehicleNumber, DataModel.Depot);
        var routing = new RoutingModel(manager);

        var transitCallbackIndex = routing.RegisterTransitCallback((long fromIndex, long toIndex) =>
        {
            var fromNode = manager.IndexToNode(fromIndex);
            var toNode = manager.IndexToNode(toIndex);
            return data.DistanceMatrix[fromNode, toNode];
        });

        routing.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);

        var searchParameters =
            operations_research_constraint_solver.DefaultRoutingSearchParameters();
        searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;

        var solution = routing.SolveWithParameters(searchParameters);
        PrintSolution(routing, manager, solution);
    }, _ => Task.FromResult(-1));

static void PrintSolution(in RoutingModel routing, in RoutingIndexManager manager, in Assignment solution)
{
    Console.WriteLine("Objective: {0}", solution.ObjectiveValue());
    Console.WriteLine("Route:");
    long routeDistance = 0;
    var index = routing.Start(0);
    while (routing.IsEnd(index) == false)
    {
        Console.Write("{0}-", manager.IndexToNode((int) index));
        var previousIndex = index;
        index = solution.Value(routing.NextVar(index));
        routeDistance += routing.GetArcCostForVehicle(previousIndex, index, 0);
    }

    Console.WriteLine("{0}", manager.IndexToNode((int) index));
    Console.WriteLine("Route distance: {0}", routeDistance);
}

async ValueTask<long[,]> ReadFileAsync(string path)
{
    using StreamReader reader = new(path);
    if (!int.TryParse(await reader.ReadLineAsync(), out var size)) return new long[0, 0];

    var result = new long[size, size];

    for (var i = 0; i < size; i++)
    {
        var line = (await reader.ReadLineAsync() ?? "").Trim().Split(' ').Select(int.Parse).ToArray();
        for (var j = 0; j < line.Length; j++)
        {
            result[i, j] = line[j];
            result[j, i] = line[j];
        }
    }

    return result;
}


internal class DataModel
{
    public long[,] DistanceMatrix =
    {
        {0, 2451, 713, 1018, 1631, 1374, 2408, 213, 2571, 875, 1420, 2145, 1972},
        {2451, 0, 1745, 1524, 831, 1240, 959, 2596, 403, 1589, 1374, 357, 579},
        {713, 1745, 0, 355, 920, 803, 1737, 851, 1858, 262, 940, 1453, 1260},
        {1018, 1524, 355, 0, 700, 862, 1395, 1123, 1584, 466, 1056, 1280, 987},
        {1631, 831, 920, 700, 0, 663, 1021, 1769, 949, 796, 879, 586, 371},
        {1374, 1240, 803, 862, 663, 0, 1681, 1551, 1765, 547, 225, 887, 999},
        {2408, 959, 1737, 1395, 1021, 1681, 0, 2493, 678, 1724, 1891, 1114, 701},
        {213, 2596, 851, 1123, 1769, 1551, 2493, 0, 2699, 1038, 1605, 2300, 2099},
        {2571, 403, 1858, 1584, 949, 1765, 678, 2699, 0, 1744, 1645, 653, 600},
        {875, 1589, 262, 466, 796, 547, 1724, 1038, 1744, 0, 679, 1272, 1162},
        {1420, 1374, 940, 1056, 879, 225, 1891, 1605, 1645, 679, 0, 1017, 1200},
        {2145, 357, 1453, 1280, 586, 887, 1114, 2300, 653, 1272, 1017, 0, 504},
        {1972, 579, 1260, 987, 371, 999, 701, 2099, 600, 1162, 1200, 504, 0},
    };

    public const int VehicleNumber = 1;
    public const int Depot = 0;
}