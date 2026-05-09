namespace ServicoMonitorPasta.Testes;

public class OpcoesMonitorTestes
{
    [Fact]
    public void DeveTeProvalorerPadraoDeCaminhoPasta()
    {
        var opcoes = new OpcoesMonitor();

        Assert.Equal(@"C:\PastaMonitorada", opcoes.CaminhoPasta);
    }

    [Fact]
    public void DeveTerValorPadraoDeCaminhoLog()
    {
        var opcoes = new OpcoesMonitor();

        Assert.Equal(@"C:\Logs\monitor-arquivos.log", opcoes.CaminhoLog);
    }

    [Fact]
    public void DeveTerIncluirSubpastasComoFalsePorPadrao()
    {
        var opcoes = new OpcoesMonitor();

        Assert.False(opcoes.IncluirSubpastas);
    }

    [Fact]
    public void DevePermitirAlterarCaminhoPasta()
    {
        var opcoes = new OpcoesMonitor();

        opcoes.CaminhoPasta = @"C:\NovasPasta";

        Assert.Equal(@"C:\NovasPasta", opcoes.CaminhoPasta);
    }

    [Fact]
    public void DevePermitirAlterarCaminhoLog()
    {
        var opcoes = new OpcoesMonitor();

        opcoes.CaminhoLog = @"C:\NovoLog\app.log";

        Assert.Equal(@"C:\NovoLog\app.log", opcoes.CaminhoLog);
    }
}