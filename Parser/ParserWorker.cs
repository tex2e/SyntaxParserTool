
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Sprache;
using System.Text;

namespace SyntaxParserTool.Parser;

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


            string inputfile = config["input"] ?? throw new ArgumentNullException("config.input");
            string parsertype = config["type"] ?? throw new ArgumentNullException("config.type");
            string inputencoding = config["encode"] ?? "";

            string input;
            switch (inputencoding)
            {
                case "sjis":
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    input = File.ReadAllText(inputfile, Encoding.GetEncoding("Shift_JIS"));
                    break;
                default:
                    input = File.ReadAllText(inputfile);
                    break;
            }

            logger.LogInformation("inputfile: {}", inputfile);
            logger.LogInformation("inputencoding: {}", inputencoding);

            switch (parsertype)
            {
                case "expr":
                    var parsed = SyntaxParserTool.Parser.Expr.ExpressionParser.ParseExpression(input);
                    logger.LogInformation("parsed: {}", parsed);
                    logger.LogInformation("value: {}", parsed.Compile()());
                    break;

                case "xml":
                    var parsedXml = SyntaxParserTool.Parser.Xml.XmlParser.Document.Parse(input);
                    logger.LogInformation("parsed: {}", parsedXml);
                    break;

                case "csv":
                    var parsedCsv = SyntaxParserTool.Parser.Csv.CsvParser.Csv.Parse(input);
                    var parsedCsvString = new StringBuilder();
                    foreach (var line in parsedCsv)
                    {
                        parsedCsvString.AppendJoin(", ", line.ToArray()).Append(Environment.NewLine);
                    }
                    logger.LogInformation("{}", parsedCsvString);
                    break;

                case "mydsl":
                    var parsedMyDSL = SyntaxParserTool.Parser.MyDSL.MyDSLParser.Questionnaire.Parse(input);
                    logger.LogInformation("parsed: {}", parsedMyDSL);
                    break;

                case "bat":
                    var parsedWindowsBat = SyntaxParserTool.Parser.WindowsBatch.WindowsBatchParser.BatchFile.Parse(input);
                    Console.WriteLine(parsedWindowsBat);
                    break;

                default:
                    throw new NotImplementedException("選択したファイル種別は構文解析できません！");
            }
            

        }
        finally
        {
            appLifetime.StopApplication();
        }
    }

}

// Usage:
//   dotnet run --input InputFiles/TestExpr.txt --type expr
//   dotnet run --input InputFiles/TestFile.xml --type xml
