using ServicoMonitorPasta;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "ServicoMonitorPasta";
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<OpcoesMonitor>(
            context.Configuration.GetSection("FileMonitor"));

        services.AddHostedService<ServicoMonitoramento>();
    })
    .Build();

await host.RunAsync();