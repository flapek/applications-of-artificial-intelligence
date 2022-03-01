using applications_of_artificial_intelligence_1;
using CommandLine;
using System.Collections.Concurrent;
using System.Diagnostics;

await Parser.Default.ParseArguments<CommandLineOptions>(args)
    .MapResult(async (CommandLineOptions args) =>
    {
        try
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            using var reader = new StreamReader(args.Path);

            var array = await ReadFileAsync(reader);
            var population = GeneratePopulation(array);

            ConcurrentDictionary<int[], int> markedPopulation = MarkPopulations(population, array);

            stopwatch.Stop();
            Console.WriteLine("Elapsed time in miliseconds: {0}", stopwatch.ElapsedMilliseconds);
            return 0;
        }
        catch (Exception)
        {
            return -3;
        }
    }, error => Task.FromResult(-1));

ConcurrentDictionary<int[], int> MarkPopulations(List<int[]> populations, int[][] distances)
{
    ConcurrentDictionary<int[], int> result = new();
    foreach (var population in populations)
        result.TryAdd(population, MarkPopulation(population, distances));

    return result;
}

int MarkPopulation(int[] population, int[][] distances)
{
    int result = 0;
    for (int i = 0; i < population.Length; i++)
    {
        var j = i + 1;
        if (j >= population.Length)
            j = 0;

        result += distances[population[i]][population[j]];    
    }

    return result;
}

List<int[]> GeneratePopulation(int[][] array)
{
    List<int[]> population = new();
    var indexes = new int[array.Length];
    for (int i = 0; i < array.Length; i++)
        indexes[i] = i;

    for (int i = 0; i < array.Length; i++)
    {
        population.Add(Randomize(indexes));
    }

    return population;
}

int[] Randomize(int[] indexes)
    => indexes.OrderBy(_ => Random.Shared.Next()).ToArray();

async ValueTask<int[][]> ReadFileAsync(StreamReader reader)
{
    if (!int.TryParse(await reader.ReadLineAsync(), out int size))
        return Array.Empty<int[]>();
    var result = new int[size][];

    for (int i = 0; i < size; i++)
    {
        result[i] = new int[size];
        var line = (await reader.ReadLineAsync() ?? "").Trim().Split(' ').Select(s => int.Parse(s)).ToArray();
        for (int j = 0; j < line.Length; j++)
        {
            result[i][j] = line[j];
            result[j][i] = line[j];
        }
    }

    return result;
}

void Display(int[][] array)
{
    for (int i = 0; i < array.Length; i++)
    {
        for (int j = 0; j < array[i].Length; j++)
        {
            Console.Write(array[i][j] + " ");
        }
        Console.WriteLine();
    }
}

