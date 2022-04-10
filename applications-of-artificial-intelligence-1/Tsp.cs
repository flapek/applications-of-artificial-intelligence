using static System.Threading.Tasks.Parallel;

namespace applications_of_artificial_intelligence_1;

internal class Tsp
{
    private readonly int _distancesLenght;
    private readonly int[,] _distances;
    private readonly int _populationSize;
    private double _mutationProbability;
    private double _crucifixionProbability;
    private readonly int _kIndividuals;
    private readonly CrucifixionAlgorithmType _algorithm;
    private Generation[] _populations;
    private readonly Random _rnd;

    public Tsp(int[,] distances, int populationSize, double mutationProbability, double crucifixionProbability)
    {
        _distances = distances;
        _populationSize = populationSize;
        _mutationProbability = mutationProbability;
        _crucifixionProbability = crucifixionProbability;
        _distancesLenght = distances.GetLength(0);
        var kIndividuals = (int) Math.Round(0.05 * populationSize);
        _kIndividuals = kIndividuals == 0 ? 2 : kIndividuals;
        _algorithm = CrucifixionAlgorithmType.Pmx;
        _populations = new Generation[_populationSize];
        _rnd = new Random();
    }

    public async Task GeneratePopulation()
    {
        var indexes = Enumerable.Range(0, _distancesLenght).ToArray();

        await ForEachAsync(Enumerable.Range(0, _populationSize), async (idx, _) =>
        {
            var idxes = await Randomize(indexes);
            _populations[idx] = new Generation(idxes, await MarkPopulation(idxes));
        });
    }

    public Task<Generation> ChooseMin()
    {
        var min = _populations.Min(population => population.Mark);
        return Task.FromResult(_populations.FirstOrDefault(population => population.Mark.Equals(min)))!;
    }

    public async Task PopulationSelection()
    {
        var result = new Generation[_populationSize];
        for (var i = 0; i < _populationSize; i++)
        {
            var generations = await TakeRandom();
            var min = generations.Min(generation => generation.Mark);
            result[i] = generations.FirstOrDefault(generation => generation.Mark.Equals(min))!;
        }

        _populations = result;
    }

    public async Task UpdateMarks()
        => await ForEachAsync(_populations,
            async (gen, _) => gen.UpdateMark(await MarkPopulation(gen.Population)));

    private Task<Generation[]> TakeRandom()
    {
        var result = new Generation[_kIndividuals];
        for (var i = 0; i < _kIndividuals; i++)
            result[i] = _populations[_rnd.Next(0, _populations.Length)];
        return Task.FromResult(result);
    }

    public async Task Mutation()
    {
        foreach (var gen in _populations)
        {
            if (_rnd.NextDouble() > _mutationProbability) continue;
            var idx1 = _rnd.Next(0, _distancesLenght - 1);
            var idx2 = _rnd.Next(0, _distancesLenght - 1);
            (gen.Population[idx1], gen.Population[idx2]) = (gen.Population[idx2], gen.Population[idx1]);
        }
    }

    public async Task Crucifixion()
    {
        foreach (var step in SteppedIterator(0, _populationSize, 2))
        {
            if (_rnd.NextDouble() > _crucifixionProbability) continue;
            (_populations[step], _populations[step + 1]) =
                await CrucifixionPmx(_populations[step], _populations[step + 1]);
        }
    }

    public void UpdateMutationProbability() => _mutationProbability += 0.005;
    
    public void UpdateCrucifixionProbability() => _crucifixionProbability -= 0.005;

    private ValueTask<int[]> Randomize(IEnumerable<int> indexes) =>
        ValueTask.FromResult(indexes.OrderBy(_ => _rnd.Next()).ToArray());

    private ValueTask<int> MarkPopulation(IReadOnlyList<int> population)
    {
        var result = 0;
        for (var i = 0; i < _distancesLenght - 1; i++)
        {
            var j = i + 1;
            if (j >= _distancesLenght) j = 0;

            result += _distances[population[i], population[j]];
        }

        return ValueTask.FromResult(result);
    }

    private static IEnumerable<int> SteppedIterator(int startIndex, int endIndex, int stepSize)
    {
        for (var i = startIndex; i < endIndex; i += stepSize)
        {
            yield return i;
        }
    }

    private async Task<(Generation gen1, Generation gen2)> CrucifixionPmx(Generation gen1, Generation gen2)
    {
        Task<int[]> HelpMethod(int[] middle1, int[] middle2, int[] parent)
        {
            var result = new List<int>();
            foreach (var element in parent)
            {
                var x = element;
                while (middle1.Contains(x))
                {
                    var i = Array.IndexOf(middle1, x);
                    x = middle2[i];
                }

                result.Add(x);
            }

            return Task.FromResult(result.ToArray());
        }

        Task<int[]> Concat(int[] start, int[] mid, int[] end)
        {
            var result = new int[_distancesLenght];
            start.CopyTo(result, 0);
            mid.CopyTo(result, start.Length);
            end.CopyTo(result, mid.Length + start.Length);
            return Task.FromResult(result);
        }

        var idx1 = _rnd.Next(0, _distancesLenght - 1);
        var idx2 = _rnd.Next(0, _distancesLenght - 1);
        if (idx1 == idx2) return (gen1, gen2);
        if (idx1 > idx2) (idx1, idx2) = (idx2, idx1);

        var mid1 = gen1.Population[idx1..idx2];
        var mid2 = gen2.Population[idx1..idx2];
        
        return (
            new Generation(await Concat(
                await HelpMethod(mid1, mid2, gen2.Population[..idx1]), 
                mid1,
                await HelpMethod(mid1, mid2, gen2.Population[idx2..])), 0), 
            new Generation(await Concat(
                await HelpMethod(mid2, mid1, gen1.Population[..idx1]), 
                mid2, 
                await HelpMethod(mid2, mid1, gen1.Population[idx2..])), 0));
    }

    // private static Task CrucifixionOx(Generation gen1, Generation gen2)
    // {
    //     return Task.CompletedTask;
    // }
    //
    // private static Task CrucifixionCx(Generation gen1, Generation gen2)
    // {
    //     return Task.CompletedTask;
    // }
    
}