namespace ServicoMonitorPasta.Testes;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

public class ServicoMonitoramentoTestes : IDisposable
{
    private readonly string _pastaTeste;
    private readonly string _logTeste;
    private readonly Mock<ILogger<ServicoMonitoramento>> _loggerMock;
    private readonly OpcoesMonitor _opcoes;

    public ServicoMonitoramentoTestes()
    {
        _pastaTeste = Path.Combine(Path.GetTempPath(), $"TesteMonitor_{Guid.NewGuid()}");
        _logTeste = Path.Combine(Path.GetTempPath(), $"teste_{Guid.NewGuid()}.log");

        _loggerMock = new Mock<ILogger<ServicoMonitoramento>>();

        _opcoes = new OpcoesMonitor
        {
            CaminhoPasta = _pastaTeste,
            CaminhoLog = _logTeste,
            IncluirSubpastas = false
        };
    }

    public void Dispose()
    {
        if (Directory.Exists(_pastaTeste))
            Directory.Delete(_pastaTeste, recursive: true);

        if (File.Exists(_logTeste))
            File.Delete(_logTeste);
    }

    private ServicoMonitoramento CriarServico()
    {
        var opcoesWrapper = Options.Create(_opcoes);
        return new ServicoMonitoramento(_loggerMock.Object, opcoesWrapper);
    }

    [Fact]
    public async Task StartAsync_DeveCriarPastaSeNaoExistir()
    {
        var servico = CriarServico();

        await servico.StartAsync(CancellationToken.None);

        Assert.True(Directory.Exists(_pastaTeste));

        await servico.StopAsync(CancellationToken.None);
        servico.Dispose();
    }

    [Fact]
    public async Task StartAsync_NaoDeveCriarPastaSeJaExistir()
    {
        Directory.CreateDirectory(_pastaTeste);
        var servico = CriarServico();

        await servico.StartAsync(CancellationToken.None);

        Assert.True(Directory.Exists(_pastaTeste));

        await servico.StopAsync(CancellationToken.None);
        servico.Dispose();
    }

    [Fact]
    public async Task StopAsync_DeveEncerrarSemErros()
    {
        var servico = CriarServico();
        await servico.StartAsync(CancellationToken.None);

        var excecao = await Record.ExceptionAsync(() =>
            servico.StopAsync(CancellationToken.None));

        Assert.Null(excecao);
        servico.Dispose();
    }

    [Fact]
    public async Task ArquivoCriado_DeveGerarEntradaNoLog()
    {
        var servico = CriarServico();
        await servico.StartAsync(CancellationToken.None);

        var arquivoTeste = Path.Combine(_pastaTeste, "teste.txt");
        await File.WriteAllTextAsync(arquivoTeste, "conteúdo teste");

        await Task.Delay(500);

        Assert.True(File.Exists(_logTeste), "Arquivo de log não foi criado.");
        var conteudoLog = await File.ReadAllTextAsync(_logTeste);
        Assert.Contains("Criado", conteudoLog);
        Assert.Contains("teste.txt", conteudoLog);

        await servico.StopAsync(CancellationToken.None);
        servico.Dispose();
    }

    [Fact]
    public async Task ArquivoDeletado_DeveGerarEntradaNoLog()
    {
        var servico = CriarServico();
        await servico.StartAsync(CancellationToken.None);

        var arquivoTeste = Path.Combine(_pastaTeste, "deletar.txt");
        await File.WriteAllTextAsync(arquivoTeste, "conteúdo");
        await Task.Delay(300);

        // Act
        File.Delete(arquivoTeste);
        await Task.Delay(500);

        Assert.True(File.Exists(_logTeste), "Arquivo de log não foi criado.");
        var conteudoLog = await File.ReadAllTextAsync(_logTeste);
        Assert.Contains("Deletado", conteudoLog);
        Assert.Contains("deletar.txt", conteudoLog);

        await servico.StopAsync(CancellationToken.None);
        servico.Dispose();
    }
}