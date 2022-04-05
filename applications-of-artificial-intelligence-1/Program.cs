using applications_of_artificial_intelligence_1;
using CommandLine;
using System.Diagnostics;

int[][] distances;
int distancesLenght;
int populationSize;

await Parser.Default.ParseArguments<CommandLineOptions>(args)
    .MapResult(async args =>
    {
        populationSize = args.Population;
        var kIndividuals = (int) Math.Round(0.05 * populationSize);
        kIndividuals = kIndividuals == 0 ? 2 : kIndividuals;
        var generation = 0;

        var stopwatch = Stopwatch.StartNew();
        distances = await ReadFileAsync(args.Path);
        distancesLenght = distances.Length;

        var populations = await GeneratePopulation();
        var min = populations.OrderBy(p => p.Mark).FirstOrDefault();

        while (generation < args.Generations)
        {
            generation++;
            populations = await PopulationSelection(populations, kIndividuals);
            populations = await Crucifixion(populations, CrucifixionAlgorithmType.Pmx, args.CrucifixionPropability);
            await Mutation(populations, args.MutationPropability);
            await UpdateMarks(populations);

            min = populations.OrderBy(p => p.Mark).FirstOrDefault();

            if (generation % 100 != 0) continue;
            Console.WriteLine(generation);
            Display(min);
        }

        stopwatch.Stop();

        Console.WriteLine("The best:");
        Display(min);
        Console.WriteLine("Elapsed time: {0}:{1}:{2}", stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds,
            stopwatch.Elapsed.Milliseconds);
        return 0;
    }, _ => Task.FromResult(-1));


async ValueTask<int[][]> ReadFileAsync(string path)
{
    using StreamReader reader = new(path);
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

async ValueTask<Generation[]> GeneratePopulation()
{
    var population = new Generation[populationSize];
    var indexes = Enumerable.Range(0, distancesLenght).ToArray();

    await Parallel.ForEachAsync(Enumerable.Range(0, populationSize), async (idx, _) =>
    {
        indexes = await Randomize(indexes);
        population[idx] = (new Generation(indexes, await MarkPopulation(indexes)));
    });

    return population.ToArray();
}

ValueTask<int[]> Randomize(IEnumerable<int> indexes) =>
    ValueTask.FromResult(indexes.OrderBy(_ => Random.Shared.Next()).ToArray());

ValueTask<int> MarkPopulation(IReadOnlyList<int> population)
{
    var result = 0;
    for (var i = 0; i < population.Count; i++)
    {
        var j = i + 1;
        if (j >= population.Count) j = 0;

        result += distances[population[i]][population[j]];
    }

    return new ValueTask<int>(result);
}

ValueTask<Generation[]> PopulationSelection(Generation[] populations, int kIndividuals)
{
    var result = new Generation[populationSize];

    // TODO: to many allocated memory
    for (var i = 0; i < populationSize; i++)
        result[i] = populations.OrderBy(_ => Random.Shared.NextDouble()).Take(kIndividuals).OrderBy(x => x.Mark)
            .FirstOrDefault();

    return new ValueTask<Generation[]>(result);
}

async ValueTask<Generation[]> Crucifixion(Generation[] populations, CrucifixionAlgorithmType algorithm,
    double crucifixionProbability)
{
    var result = new Generation[populationSize];

    await Parallel.ForEachAsync(SteppedIterator(0, populationSize, 2), (i, _) =>
    {
        if (Random.Shared.NextDouble() < crucifixionProbability)
        {
            (result[i], result[i + 1]) = algorithm switch
            {
                CrucifixionAlgorithmType.Pmx => CrucifixionPmx(populations.ElementAt(i), populations.ElementAt(i + 1)),
                CrucifixionAlgorithmType.Ox => CrucifixionOx(populations.ElementAt(i), populations.ElementAt(i + 1)),
                CrucifixionAlgorithmType.Cx => CrucifixionCx(populations.ElementAt(i), populations.ElementAt(i + 1)),
                _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
            };
        }
        else
            (result[i], result[i + 1]) = (populations[i], populations[i + 1]);
        
        return ValueTask.CompletedTask;
    });
    
    return result;
}

IEnumerable<int> SteppedIterator(int startIndex, int endIndex, int stepSize)
{
    for (var i = startIndex; i < endIndex; i += stepSize)
    {
        yield return i;
    }
}

(Generation gen1, Generation gen2) CrucifixionPmx(Generation gen1, Generation gen2)
{
    var genLenght = gen1.Population.Length - 1;
    var (idx1, idx2) = (Random.Shared.Next(0, genLenght), Random.Shared.Next(0, genLenght));
    
    return (gen1, gen2);
}

(Generation gen1, Generation gen2) CrucifixionOx(Generation gen1, Generation gen2)
{
    return (gen1, gen2);
}

(Generation gen1, Generation gen2) CrucifixionCx(Generation gen1, Generation gen2)
{
    return (gen1, gen2);
}

async Task Mutation(IEnumerable<Generation> populations, double mutationProbability)
{
    await Parallel.ForEachAsync(populations, (gen, _) =>
    {
        if (Random.Shared.NextDouble() > mutationProbability) return ValueTask.CompletedTask;
        var idx1 = Random.Shared.Next(0, gen.Population.Length - 1);
        var idx2 = Random.Shared.Next(0, gen.Population.Length - 1);
        (gen.Population[idx1], gen.Population[idx2]) = (gen.Population[idx2], gen.Population[idx1]);
        return ValueTask.CompletedTask;
    });
}

async Task UpdateMarks(IEnumerable<Generation> populations)
    => await Parallel.ForEachAsync(populations,
        async (gen, _) => await gen.UpdateMark(await MarkPopulation(gen.Population)));

void Display(Generation generation) =>
    Console.WriteLine("{0} {1}", string.Join('-', generation.Population), generation.Mark);

internal struct Generation
{
    public int[] Population { get; }
    public int Mark { get; private set; }

    public Generation(int[] population, int mark)
    {
        Population = population;
        Mark = mark;
    }

    public ValueTask UpdateMark(int mark)
    {
        Mark = mark;
        return new ValueTask();
    }
};