using System.IO;
using Fclp;

namespace SNTP_server
{
    internal class ServerArguments
    {
        public int SecondsDelay { get; set; }

        public bool NegativeDelay { get; set; }

        public static bool TryParse(string[] args, out ServerArguments serverArguments, TextWriter logWriter)
        {
            var argumentsParser = new FluentCommandLineParser<ServerArguments>();

            argumentsParser.Setup(a => a.SecondsDelay)
                .As('d', "delay")
                .SetDefault(0)
                .WithDescription("In seconds");

            argumentsParser.Setup(a => a.NegativeDelay)
                .As('n', "negative")
                .SetDefault(false)
                .WithDescription("Use for negatove delay");

            argumentsParser.SetupHelp("?", "h", "help")
                .Callback(text => logWriter.WriteLine(text));

            var parsedResult = argumentsParser.Parse(args);

            if (parsedResult.HasErrors)
            {
                argumentsParser.HelpOption.ShowHelp(argumentsParser.Options);
                serverArguments = null;
                return false;
            }

            if (argumentsParser.Object.NegativeDelay)
                argumentsParser.Object.SecondsDelay *= -1;

            serverArguments = argumentsParser.Object;
            return !parsedResult.HelpCalled;
        }
    }
}