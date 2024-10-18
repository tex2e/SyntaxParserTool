namespace SyntaxParserTool;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

public sealed class ParserWorker(
    ILogger<ParserWorker> logger, 
    IHostApplicationLifetime appLifetime,
    IConfiguration config)
        : IHostedService
{

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        // logger.LogInformation("[+] Start!");
        appLifetime.ApplicationStarted.Register(OnStarted);
        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        // logger.LogInformation("[+] Stop!");
        return Task.CompletedTask;
    }

    // ---

    private void OnStarted()
    {
        try
        {
            // logger.LogInformation($"inputfile: {config["input"]}");

            string inputfile = config["input"] ?? "";
            if (string.IsNullOrEmpty(inputfile))
                throw new ArgumentNullException(nameof(inputfile));
            string input = File.ReadAllText(inputfile);
            // logger.LogInformation("input: {input}", input);

            string parsertype = config["type"] ?? "";
            if (string.IsNullOrEmpty(parsertype))
                throw new ArgumentNullException(nameof(parsertype));

            switch (parsertype)
            {
                case "expr":
                    var parsed = ExpressionParser.ParseExpression(input);
                    logger.LogInformation("parsed: {}", parsed);
                    logger.LogInformation("value: {}", parsed.Compile()());
                    break;
            }
            

        }
        finally
        {
            appLifetime.StopApplication();
        }
    }

}
