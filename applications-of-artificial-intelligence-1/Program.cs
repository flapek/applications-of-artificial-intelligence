using applications_of_artificial_intelligence_1;
using CommandLine;
using System.Diagnostics;

Tsp tsp;
var generation = 0;
var stagnation = 0;
Stopwatch stopwatch;

await Parser.Default.ParseArguments<CommandLineOptions>(args)
    .MapResult(async args =>
    {
        stopwatch = Stopwatch.StartNew();
        tsp = new Tsp(await ReadFileAsync(args.Path), args.Population, args.MutationPropability,
            args.CrucifixionPropability);

        await tsp.GeneratePopulation();
        var min = await tsp.ChooseMin();
        while (generation < args.Generations)
        {
            generation++;
            await tsp.PopulationSelection();
            await tsp.Crucifixion();
            await tsp.Mutation();
            await tsp.UpdateMarks();
            var newMin = await tsp.ChooseMin();
            if (min.Mark >= newMin.Mark)
            {
                min = new Generation(newMin.Population.ToArray(), newMin.Mark);
                stagnation = 0;
            }
            else
            {
                tsp.SetDefault();
                stagnation++;
            }

            if (stagnation % 600 == 0) tsp.UpdateMutationProbability();
            if (stagnation % 600 == 0) tsp.UpdateCrucifixionProbability();
            if (generation % 1000 > 800 && stagnation > 300) tsp.ChangeAlgorithm();
            if (generation % 1000 > 800 && stagnation % 100 > 90) await tsp.MutationBomb();
            if (stagnation > 15000) break;
            
            if (generation % 500 != 0) continue;
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


async ValueTask<int[,]> ReadFileAsync(string path)
{
    using StreamReader reader = new(path);
    if (!int.TryParse(await reader.ReadLineAsync(), out var size)) return new int[,] { };
    var result = new int[size, size];

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

void Display(Generation gen) =>
    Console.WriteLine("{0} {1}", string.Join('-', gen.Population), gen.Mark);