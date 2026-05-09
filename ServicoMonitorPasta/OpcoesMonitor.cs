namespace ServicoMonitorPasta;

public class OpcoesMonitor
{
    public string CaminhoPasta { get; set; } = @"C:\PastaMonitorada";
    public string CaminhoLog { get; set; } = @"C:\Logs\monitor-arquivos.log";
    public bool IncluirSubpastas { get; set; } = false;
}