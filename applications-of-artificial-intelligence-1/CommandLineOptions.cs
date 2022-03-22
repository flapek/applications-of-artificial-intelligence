using CommandLine;

namespace applications_of_artificial_intelligence_1
{
    internal sealed class CommandLineOptions
    {
        [Option('d', "data", Required = true, HelpText = "Path to file with data")]
        public string Path { get; set; }
        
        [Option('p', "population", Required = true, HelpText = "Population size")]
        public int Population { get; set; }
        
        [Option('k', "kIndividuals", Required = true, HelpText = "K-individuals size")]
        public int KIndividuals { get; set; }

        [Option('c', Required = false, HelpText = "Crucifixion algorithm type")]
        public CrucifixionAlgorithmType CrucifixionAlgorithm { get; set; }
    }

    internal enum CrucifixionAlgorithmType
    {
        PMX,
        OX,
        CX
    }
}
