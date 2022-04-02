using CommandLine;

namespace applications_of_artificial_intelligence_1
{
    internal sealed class CommandLineOptions
    {
        [Option('d', "data", Required = true, HelpText = "Path to file with data")]
        public string Path { get; set; }

        [Option('p', "population", Required = true, HelpText = "Population size")]
        public int Population { get; set; }
        
        [Option('c', Required = false, HelpText = "Crucifixion propability")]
        public double CrucifixionPropability { get; set; } = 0.95;

        [Option('m', Required = false, HelpText = "Mutation propability")]
        public double MutationPropability { get; set; } = 0.005;

        [Option('g', Required = true, HelpText = "Number of minimum generation")]
        public int Generations { get; set; }
    }
}