using CommandLine;

namespace applications_of_artificial_intelligence_1
{
    internal sealed class CommandLineOptions
    {
        [Option('p', "path", Required = true, HelpText = "Path to file with data")]
        public string Path { get; set; }
    }
}
