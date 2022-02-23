// See https://aka.ms/new-console-template for more information
using applications_of_artificial_intelligence_1;
using CommandLine;

await Parser.Default.ParseArguments<CommandLineOptions>(args)
    .MapResult(async (CommandLineOptions args) =>
    {
        try
        {
            using var reader = new StreamReader(args.Path);

            var array = await ReadFileAsync(reader);

            Display(array);
            return 0;
        }
        catch (Exception)
        {
            return -3;
        }
    }, error => Task.FromResult(-1));

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