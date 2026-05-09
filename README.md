# ServicoMonitorPasta

Windows Service em .NET 10 que monitora uma pasta do sistema e registra em arquivo `.log` toda criação e deleção de arquivos.

---

## 📋 Índice

- [Visão Geral](#visão-geral)
- [Requisitos](#requisitos)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Configuração](#configuração)
- [Como Executar](#como-executar)
- [Deploy como Serviço Windows](#deploy-como-serviço-windows)
- [Internacionalização](#internacionalização)
- [Testes](#testes)
- [Arquitetura](#arquitetura)

---

## Visão Geral

O **ServicoMonitorPasta** é um Windows Service que utiliza `FileSystemWatcher` para monitorar em tempo real uma pasta configurável. Cada evento de criação ou deleção de arquivo é registrado em um arquivo `.log` com timestamp preciso.

**Exemplo de saída no log:**
```
[2026-05-09 14:32:01.123] [CRIADO  ] C:\PastaMonitorada\relatorio.pdf
[2026-05-09 14:33:45.456] [DELETADO] C:\PastaMonitorada\temp.txt
```

---

## Requisitos

- Windows 10 / Windows Server 2016 ou superior
- .NET 10 SDK
- Visual Studio 2022 (para desenvolvimento)

---

## Estrutura do Projeto

```
Desafio-Paschoalotto/
├── ServicoMonitorPasta/
│   ├── Program.cs                  # Ponto de entrada e configuração do host
│   ├── ServicoMonitoramento.cs     # Lógica principal do serviço
│   ├── OpcoesMonitor.cs            # Classe de configuração
│   ├── appsettings.json            # Configurações externalizadas
│   ├── Mensagens.resx              # Resources em Português (padrão)
│   ├── Mensagens.en.resx           # Resources em Inglês
│   └── Mensagens.es.resx           # Resources em Espanhol
│
└── ServicoMonitorPasta.Testes/
    ├── OpcoesMonitorTestes.cs       # Testes da classe de configuração
    ├── MensagensTestes.cs           # Testes de internacionalização
    └── ServicoMonitoramentoTestes.cs # Testes de comportamento do serviço
```

---

## Configuração

Edite o arquivo `appsettings.json` para ajustar os caminhos:

```json
{
  "FileMonitor": {
    "CaminhoPasta": "C:\\PastaMonitorada",
    "CaminhoLog": "C:\\Logs\\monitor-arquivos.log",
    "IncluirSubpastas": false
  }
}
```

| Propriedade | Descrição | Padrão |
|---|---|---|
| `CaminhoPasta` | Pasta a ser monitorada | `C:\PastaMonitorada` |
| `CaminhoLog` | Caminho completo do arquivo de log | `C:\Logs\monitor-arquivos.log` |
| `IncluirSubpastas` | Monitora subpastas recursivamente | `false` |

> As pastas são criadas automaticamente pelo serviço caso não existam.

---

## Como Executar

### Modo desenvolvimento (Visual Studio)

1. Abra a solução `Desafio-Paschoalotto.sln` no Visual Studio 2022
2. Defina `ServicoMonitorPasta` como projeto de inicialização
3. Pressione **F5**
4. Crie ou delete arquivos em `C:\PastaMonitorada`
5. Verifique o log em `C:\Logs\monitor-arquivos.log`

### Modo desenvolvimento (terminal)

```bash
cd ServicoMonitorPasta
dotnet run
```

---

## Deploy como Serviço Windows

**1. Publicar o executável:**
```bash
dotnet publish -c Release -r win-x64 --self-contained -o C:\Services\FileMonitor
```

**2. Instalar o serviço (como Administrador):**
```bash
sc create ServicoMonitorPasta binPath="C:\Services\FileMonitor\ServicoMonitorPasta.exe"
sc description ServicoMonitorPasta "Monitora pasta e registra eventos de arquivo em log"
sc config ServicoMonitorPasta start=auto
```

**3. Iniciar:**
```bash
sc start ServicoMonitorPasta
```

**4. Verificar status:**
```bash
sc query ServicoMonitorPasta
```

**5. Parar e remover:**
```bash
sc stop ServicoMonitorPasta
sc delete ServicoMonitorPasta
```

---

## Internacionalização

O serviço suporta três idiomas via `ResourceManager`. O idioma é selecionado automaticamente com base na cultura do sistema operacional.

| Cultura | Arquivo | Idioma |
|---|---|---|
| `pt` (padrão) | `Mensagens.resx` | Português |
| `en` | `Mensagens.en.resx` | Inglês |
| `es` | `Mensagens.es.resx` | Espanhol |

Para forçar um idioma específico em desenvolvimento, adicione temporariamente ao `Program.cs`:

```csharp
Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
```

---

## Testes

O projeto conta com **22 testes unitários** usando **xUnit** e **Moq**.

### Executar todos os testes

```bash
cd ServicoMonitorPasta.Testes
dotnet test
```

### Resultado esperado

```
total: 22 | falhou: 0 | bem-sucedido: 22 | ignorado: 0
```

### Cobertura dos testes

| Arquivo de Teste | Testes | O que cobre |
|---|---|---|
| `OpcoesMonitorTestes.cs` | 5 | Valores padrão e alteração de propriedades |
| `MensagensTestes.cs` | 12 | Resources nos 3 idiomas (PT, EN, ES) |
| `ServicoMonitoramentoTestes.cs` | 5 | Criação de pasta, log de eventos, ciclo de vida |

---

## Arquitetura

### Fluxo de eventos

```
FileSystemWatcher
      │
      ▼
OnArquivoCriado / OnArquivoDeletado
      │
      ▼
ConcurrentQueue<string>  ◄── thread-safe
      │
      ▼
SemaphoreSlim (sinal)
      │
      ▼
ExecuteAsync (loop principal)
      │
      ▼
GravarLogAsync (com retry automático)
      │
      ▼
monitor-arquivos.log
```

### Decisões técnicas

`ConcurrentQueue` + `SemaphoreSlim` — eventos do `FileSystemWatcher` chegam em threads do sistema operacional. A fila garante thread-safety sem uso de locks manuais, e o semáforo acorda o loop de gravação apenas quando há eventos, evitando polling.

**Escrita em batch** — o loop drena toda a fila de uma vez antes de abrir o arquivo, reduzindo operações de I/O em cenários de alto volume de eventos.

**Retry automático** — em caso de `IOException` (arquivo de log em uso), o serviço tenta novamente até 3 vezes com backoff crescente de 200ms.

**Configuração externalizada** — todos os parâmetros operacionais estão no `appsettings.json`, sem necessidade de recompilar para mudar caminhos.
