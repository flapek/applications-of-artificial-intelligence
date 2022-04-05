using CommandLine;

namespace orTools
{
    internal sealed class CommandLineOptions
    {
        [Option('d', "data", Required = true, HelpText = "Path to file with data")]
        public string Path { get; set; }
    }
}