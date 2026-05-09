namespace ServicoMonitorPasta;

using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Globalization;
using System.Resources;

public sealed class ServicoMonitoramento : BackgroundService
{
    private readonly ILogger<ServicoMonitoramento> _logger;
    private readonly OpcoesMonitor _opcoes;
    private readonly ConcurrentQueue<string> _filaEventos = new();
    private readonly SemaphoreSlim _sinal = new(0);
    private FileSystemWatcher? _watcher;
    private static readonly ResourceManager _rm =
    new ResourceManager("ServicoMonitorPasta.Mensagens",
                        typeof(ServicoMonitoramento).Assembly);

    public ServicoMonitoramento(
        ILogger<ServicoMonitoramento> logger,
        IOptions<OpcoesMonitor> opcoes)
    {
        _logger = logger;
        _opcoes = opcoes.Value;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_opcoes.CaminhoPasta))
        {
            Directory.CreateDirectory(_opcoes.CaminhoPasta);
            _logger.LogInformation(Msg("PastaCriada", _opcoes.CaminhoPasta));
        }

        var dirLog = Path.GetDirectoryName(_opcoes.CaminhoLog);
        if (!string.IsNullOrEmpty(dirLog) && !Directory.Exists(dirLog))
            Directory.CreateDirectory(dirLog);

        _watcher = new FileSystemWatcher(_opcoes.CaminhoPasta)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
            Filter = "*.*",
            IncludeSubdirectories = _opcoes.IncluirSubpastas,
            EnableRaisingEvents = true
        };

        _watcher.Created += OnArquivoCriado;
        _watcher.Deleted += OnArquivoDeletado;
        _watcher.Error += OnErro;

        _logger.LogInformation(Msg("MonitoramentoIniciado", _opcoes.CaminhoPasta));

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _sinal.WaitAsync(TimeSpan.FromSeconds(30), stoppingToken)
                        .ConfigureAwait(false);

            var linhas = new List<string>();
            while (_filaEventos.TryDequeue(out var linha))
                linhas.Add(linha);

            if (linhas.Count > 0)
                await GravarLogAsync(linhas, stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _watcher?.Dispose();
        _logger.LogInformation(Msg("MonitoramentoEncerrado"));
        return base.StopAsync(cancellationToken);
    }

    private void OnArquivoCriado(object sender, FileSystemEventArgs e)
    {
        var msg = Msg("ArquivoCriado", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), e.FullPath);
        _filaEventos.Enqueue(msg);
        _sinal.Release();
        _logger.LogInformation(msg);
    }

    private void OnArquivoDeletado(object sender, FileSystemEventArgs e)
    {
        var msg = Msg("ArquivoDeletado", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), e.FullPath);
        _filaEventos.Enqueue(msg);
        _sinal.Release();
        _logger.LogInformation(msg);
    }

    private void OnErro(object sender, ErrorEventArgs e)
    {
        _logger.LogError(e.GetException(), Msg("ErroPastaLog"));
        if (_watcher is not null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.EnableRaisingEvents = true;
        }
    }

    private async Task GravarLogAsync(IEnumerable<string> linhas, CancellationToken ct)
    {
        const int maxTentativas = 3;
        for (int tentativa = 1; tentativa <= maxTentativas; tentativa++)
        {
            try
            {
                await using var sw = new StreamWriter(
                    _opcoes.CaminhoLog,
                    append: true,
                    encoding: System.Text.Encoding.UTF8);

                foreach (var linha in linhas)
                    await sw.WriteLineAsync(linha);

                return;
            }
            catch (IOException ex) when (tentativa < maxTentativas)
            {
                _logger.LogWarning(ex, Msg("LogBloqueado", tentativa, maxTentativas));
                await Task.Delay(200 * tentativa, ct);
            }
        }
    }

    private static string Msg(string chave, params object[] args)
    {
        var texto = _rm.GetString(chave, CultureInfo.CurrentUICulture)
                    ?? chave;
        return args.Length > 0 ? string.Format(texto, args) : texto;
    }
}