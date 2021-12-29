using System.Collections.Generic;
using CommandLine;

namespace PvBackUps.Configs {
    public class CommandLineOptions {
        [Value(index: 0, Required = true, HelpText = "Path to search files")]
        public string SourcePath { get; set; }
        
        [Value(index:1, Required = true, HelpText = "Path to copy files")]
        public string DestinationPath { get; set; }

        [Option(shortName: 'e', longName: "extensions", Required = false, HelpText = "Valid file's extensions, separated with ','", Default = new []{"dt"}, Separator = ',')]
        public IEnumerable<string> Extensions { get; set; }
    }
}