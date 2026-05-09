namespace ServicoMonitorPasta.Testes;

using System.Globalization;
using System.Resources;

public class MensagensTestes
{
    private readonly ResourceManager _rm =
        new ResourceManager("ServicoMonitorPasta.Mensagens",
                            typeof(ServicoMonitorPasta.ServicoMonitoramento).Assembly);

    [Theory]
    [InlineData("pt", "Monitoramento encerrado.")]
    [InlineData("en", "Monitoring stopped.")]
    [InlineData("es", "Monitoreo detenido.")]
    public void MonitoramentoEncerradoDeveRetornarMensagemCorreta(
        string cultura, string esperado)
    {
        var resultado = _rm.GetString("MonitoramentoEncerrado",
                            new CultureInfo(cultura));

        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData("pt", "Monitoramento iniciado em: {0}")]
    [InlineData("en", "Monitoring started at: {0}")]
    [InlineData("es", "Monitoreo iniciado en: {0}")]
    public void MonitoramentoIniciadoDeveRetornarMensagemCorreta(
        string cultura, string esperado)
    {
        var resultado = _rm.GetString("MonitoramentoIniciado",
                            new CultureInfo(cultura));

        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData("pt", "Pasta criada: {0}")]
    [InlineData("en", "Folder created: {0}")]
    [InlineData("es", "Carpeta creada: {0}")]
    public void PastaCriadaDeveRetornarMensagemCorreta(
        string cultura, string esperado)
    {
        var resultado = _rm.GetString("PastaCriada",
                            new CultureInfo(cultura));

        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData("pt", "Erro no FileSystemWatcher.")]
    [InlineData("en", "FileSystemWatcher error.")]
    [InlineData("es", "Error en FileSystemWatcher.")]
    public void ErroPastaLogDeveRetornarMensagemCorreta(
        string cultura, string esperado)
    {
        var resultado = _rm.GetString("ErroPastaLog",
                            new CultureInfo(cultura));

        Assert.Equal(esperado, resultado);
    }
}