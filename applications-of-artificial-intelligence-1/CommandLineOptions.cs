using CommandLine;

namespace applications_of_artificial_intelligence_1
{
    internal sealed class CommandLineOptions
    {
        [Option('d', "data", Required = true, HelpText = "Path to file with data")]
        public string Path { get; set; }
        
        [Option('p', "population", Required = true, HelpText = "Path to file with data")]
        public int Population { get; set; }
    }
}
