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
            var min = markedPopulation.FirstOrDefault(population => population.Value == markedPopulation.Min(x => x.Value));
            while (generation < 10)
            {
                generation++;
                var temporaryPopulation = PopulationSelection(markedPopulation, args.KIndividuals);
                var childGeneration = Crucifixion(temporaryPopulation, args.CrucifixionAlgorithm);
                childGeneration = Mutation(childGeneration);
                markedPopulation = MarkPopulations(childGeneration, matrix);
                min = markedPopulation.FirstOrDefault(population => population.Value == markedPopulation.Min(x => x.Value));
            }
            
            Console.WriteLine($"{min.Key.Aggregate("", (current, key) => current + $"{key}, ")} {min.Value}");
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
    if (args.Population < args.KIndividuals) throw new Exception("K-individuals cannot be higher than population!!");
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
        var (p, mark) = populations.OrderBy(_ => Guid.NewGuid())
            .Take(kIndividuals)
            .OrderBy(x => x.Value)
            .FirstOrDefault();
        result.Add(new(p, mark));
    }

    return result;
}

IEnumerable<int[]> Crucifixion(IEnumerable<Generation> populations, CrucifixionAlgorithmType algorithm) =>
    algorithm switch
    {
        CrucifixionAlgorithmType.PMX => CrucifixionPmx(populations),
        CrucifixionAlgorithmType.OX => CrucifixionOx(populations),
        CrucifixionAlgorithmType.CX => CrucifixionCx(populations),
        _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
    };

IEnumerable<int[]> CrucifixionPmx(IEnumerable<Generation> populations)
{
    var generations = populations as Generation[] ?? populations.ToArray();
    var arrayLenght = generations[0].Population.Length;
    var result = new List<int[]>();
    for (var i = 0; i < generations.Length - 1; i += 2)
    {
        var (population1, _) = generations[i];
        var (population2, _) = generations[i + 1];

        var split1 = 0;
        var split2 = 0;

        while (split1 == split2)
        {
            split1 = Random.Shared.Next(1, arrayLenght - 1);
            split2 = Random.Shared.Next(1, arrayLenght - 1);

            if (split1 <= split2) continue;
            var t = split1;
            split1 = split2;
            split2 = split1;
        }

        var newp1 = new int[arrayLenght];
        var newp2 = new int[arrayLenght];

        for (var j = split1; j < split2; j++)
        {
            newp1[j] = population2[j];
            newp2[j] = population1[j];
        }

        for (var j = split1; j < split2; j++)
        {
            for (var k = 0; k < arrayLenght; k++)
            {
                if (j == k) continue;
                newp1[k] = population1[k] != newp1[j] ? population1[k] : population1[j];
                newp2[k] = population2[k] != newp2[j] ? population2[k] : population2[j];
            }
        }

        result.Add(newp1);
        result.Add(newp2);
    }

    if (generations.Length % 2 == 1) result.Add(generations.TakeLast(1).FirstOrDefault()?.Population ?? throw new InvalidOperationException());

    return result;
}

IEnumerable<int[]> CrucifixionOx(IEnumerable<Generation> populations)
{
    var generations = populations as Generation[] ?? populations.ToArray();
    var arrayLenght = generations[0].Population.Length;
    var result = new List<int[]>();
    for (var i = 0; i < generations.Length - 1; i += 2)
    {
        var (population1, _) = generations[i];
        var (population2, _) = generations[i + 1];

        var split1 = 0;
        var split2 = 0;

        while (split1 == split2)
        {
            split1 = Random.Shared.Next(1, arrayLenght - 1);
            split2 = Random.Shared.Next(1, arrayLenght - 1);

            if (split1 <= split2) continue;
            var t = split1;
            split1 = split2;
            split2 = split1;
        }

        var newp1 = new int[arrayLenght];
        var newp2 = new int[arrayLenght];

        for (var j = split1; j < split2; j++)
        {
            newp1[j] = population1[j];
            newp2[j] = population2[j];
        }

        for (var j = split2; j < arrayLenght + split2; j++)
        {
            var temporary = j % arrayLenght;
            
            newp1[temporary] = population2[j] != newp1[j] ? population1[j] : population1[j];
            newp2[temporary] = population1[j] != newp2[j] ? population2[j] : population2[j];
        }

        result.Add(newp1);
        result.Add(newp2);
    }

    if (generations.Length % 2 == 1) result.Add(generations.TakeLast(1).FirstOrDefault().Population);

    return result;
}

IEnumerable<int[]> CrucifixionCx(IEnumerable<Generation> populations)
{
    throw new NotImplementedException();
}

IEnumerable<int[]> Mutation(IEnumerable<int[]> populations)
{
    var result = new List<int[]>();

    foreach (var population in populations)
    {
        var idx = Random.Shared.Next(0, population.Length - 1);
        var nextIdx = idx * 2 % population.Length;
        var a = population[idx];
        var b = population[nextIdx];

        population[idx] = b;
        population[nextIdx] = a;
        
        result.Add(population);
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

IEnumerable<int[]> GeneratePopulation(IReadOnlyCollection<int[]> matrix, int populationSize)
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