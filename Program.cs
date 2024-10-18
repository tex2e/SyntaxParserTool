
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using SyntaxParserTool;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddCommandLine(args); // コマンドライン引数を設定に追加する
builder.Services.AddHostedService<ParserWorker>();

IHost host = builder.Build();
host.Run();
