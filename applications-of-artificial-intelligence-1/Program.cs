using applications_of_artificial_intelligence_1;
using CommandLine;
using System.Collections.Concurrent;
using System.Diagnostics;

await Parser.Default.ParseArguments<CommandLineOptions>(args)
    .MapResult(async args =>
    {
        try
        {
            ValidateArgs(args);
            
            var stopwatch = Stopwatch.StartNew();
            using StreamReader reader = new(args.Path);

            var matrix = await ReadFileAsync(reader);
            var population = GeneratePopulation(matrix, args.Population);
            var markedPopulation = MarkPopulations(population, matrix);
            var generation = 0;

            while (generation < 10)
            {
                generation++;
                var temporaryPopulation = PopulationSelection(markedPopulation, args.KIndividuals);

            }

            stopwatch.Stop();
            Console.WriteLine("Elapsed time in milliseconds: {0}", stopwatch.ElapsedMilliseconds.ToString());
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return -3;
        }
    }, _ => Task.FromResult(-1));

void ValidateArgs(CommandLineOptions args)
{
    if (args.Population < args.KIndividuals)
        throw new Exception("K-individuals cannot be higher than population!!");
}

ConcurrentDictionary<int[], int> MarkPopulations(IEnumerable<int[]> populations, IReadOnlyList<int[]> distances)
{
    ConcurrentDictionary<int[], int> result = new();
    foreach (var population in populations) result.TryAdd(population, MarkPopulation(population, distances));

    return result;
}

IEnumerable<Generation> PopulationSelection(ConcurrentDictionary<int[], int> populations, int kIndividuals)
{
    List<Generation> result = new();
    foreach (var _ in populations)
    {
        var (p, mark) = populations
            .OrderBy(_ => Guid.NewGuid()).Take(kIndividuals)
            .OrderBy(x => x.Value).FirstOrDefault();
        result.Add(new (p, mark));
    }
    return result;
}

int MarkPopulation(IReadOnlyList<int> population, IReadOnlyList<int[]> distances)
{
    var result = 0;
    for (var i = 0; i < population.Count; i++)
    {
        var j = i + 1;
        if (j >= population.Count) j = 0;

        result += distances[population[i]][population[j]];
    }

    return result;
}

List<int[]> GeneratePopulation(IReadOnlyCollection<int[]> matrix, int populationSize)
{
    List<int[]> population = new();
    var indexes = Enumerable.Range(0, matrix.Count).ToArray();

    for (var i = 0; i < populationSize; i++) population.Add(Randomize(indexes));

    return population;
}

int[] Randomize(IEnumerable<int> indexes) => indexes.OrderBy(_ => Random.Shared.Next()).ToArray();

async ValueTask<int[][]> ReadFileAsync(StreamReader reader)
{
    if (!int.TryParse(await reader.ReadLineAsync(), out var size)) return Array.Empty<int[]>();
    var result = new int[size][];

    for (var i = 0; i < size; i++)
    {
        result[i] = new int[size];
        var line = (await reader.ReadLineAsync() ?? "").Trim().Split(' ').Select(int.Parse).ToArray();
        for (var j = 0; j < line.Length; j++)
        {
            result[i][j] = line[j];
            result[j][i] = line[j];
        }
    }

    return result;
}

void Display(IEnumerable<int[]> array)
{
    foreach (var t in array)
    {
        foreach (var t1 in t) Console.Write($"{t1} ");
        Console.WriteLine();
    }
}


record Generation(int[] Population, int Mark);