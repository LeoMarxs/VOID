```
██╗   ██╗ ██████╗ ██╗██████╗
██║   ██║██╔═══██╗██║██╔══██╗
██║   ██║██║   ██║██║██║  ██║
╚██╗ ██╔╝██║   ██║██║██║  ██║
 ╚████╔╝ ╚██████╔╝██║██████╔╝
  ╚═══╝   ╚═════╝ ╚═╝╚═════╝
```

# VØID — COMMUNICATION SYSTEM

> **ENTER THE VOID.**

Sistema de comunicação ponto-a-ponto via terminal, escrito em C# do zero — sem frameworks de mensageria, sem servidor de terceiros, sem interface gráfica. Duas máquinas, uma conexão direta, um canal.

---

## O que é o VØID

VØID não é "só um chat". É um sistema de comunicação distribuído via terminal: cada máquina roda o mesmo binário, e qualquer uma pode assumir o papel de **host** (quem abre e escuta um canal) ou de **node conector** (quem estabelece o link até o host). Não existe backend, nuvem ou conta de usuário — a conversa existe apenas enquanto a conexão TCP entre as duas máquinas estiver viva.

A proposta é dupla: funcionar como uma ferramenta real de comunicação em rede local, e servir como base de estudo de sockets, concorrência assíncrona e protocolos de rede em C# — sem a abstração de bibliotecas prontas escondendo o que está de fato acontecendo por baixo.

---

## Como funciona

TCP é uma conexão ponto-a-ponto: um lado precisa escutar uma porta (`TcpListener`), o outro precisa discar para ela (`TcpClient.ConnectAsync`). Depois que o link é aceito, os dois lados compartilham um `NetworkStream` — um fluxo de bytes bidirecional.

O desafio de todo chat em tempo real é que ler e escrever precisam acontecer **ao mesmo tempo**. Por isso o VØID roda duas rotinas em paralelo assim que o link é estabelecido:

```
[LINK ESTABELECIDO]
        │
        ├──► Task de leitura (assíncrona, contínua)
        │        └─ escuta o stream, imprime toda transmissão recebida
        │
        └──► Loop principal (thread de entrada do usuário)
                 └─ captura o que você digita e envia pelo stream
```

A task de leitura nunca bloqueia a digitação, e vice-versa — as duas correm de forma independente sobre o mesmo canal até que o link seja encerrado (por comando, queda de rede, ou o node remoto desconectar).

---

## Tecnologias utilizadas

| Camada | Tecnologia |
|---|---|
| Linguagem | C# / .NET 8 |
| Rede | `System.Net.Sockets` — `TcpListener` e `TcpClient` puros |
| Concorrência | `async`/`await` + `Task.Run` para leitura e escrita paralelas |
| Interface | Console (CLI), sem GUI |
| Estética visual | Sequências de escape ANSI (truecolor RGB) para cores no terminal |
| Runtime alvo | PowerShell 7+ / Windows Terminal (suporte a ANSI truecolor) |

Nenhuma dependência externa — só a biblioteca padrão do .NET. É rede de baixo nível de propósito, sem SignalR, WebSockets de framework ou qualquer camada de abstração sobre o socket.

---

## Identidade

| Conceito | Termo no VØID |
|---|---|
| Servidor / quem escuta | **Host** |
| Máquina remota conectada | **Node** |
| Conexão ativa | **Link** |
| Mensagem enviada | **Transmission** |
| Prompt sem conexão | `VOID>` |
| Prompt com link ativo | `VOID/LINK/<node>>` |

Paleta: preto + cinza + verde ácido (`#00FF9C`) como destaque, ciano para informação, vermelho para alerta — sem elementos "hacker genérico" (sem Matrix, sem caveira). A referência é terminal Linux + cyberpunk minimalista.

---

## Funcionalidades

- **Boot sequence** com banner `V Ø I D` e simulação de carregamento de módulos ao iniciar
- **Dois modos de operação**: hospedar (`1`) ou conectar (`2`)
- **Comunicação bidirecional em tempo real** — leitura e escrita simultâneas, sem travar o console
- **Mensagens com timestamp**: `[HH:mm:ss] <node>  mensagem`
- **Mensagens de sistema padronizadas**:
  - `[+]` sucesso / link estabelecido / node conectado
  - `[-]` desconexão / link encerrado
  - `[!]` erro / conexão recusada / falha de transmissão
  - `[i]` informação neutra
  - `[MSG]` transmissão recebida
- **Encerramento controlado** via `/exit`
- **Detecção de queda de link** — se a rede cair ou o outro lado fechar, o sistema avisa em vez de travar

---

## Manual de uso

### 1. Pré-requisito

.NET 8 SDK instalado nas duas máquinas. Confirme:

```powershell
dotnet --version
```

### 2. Preparar os arquivos

Coloque `Program.cs`, `Theme.cs` e `ChatP2P.csproj` na mesma pasta, **nas duas máquinas** que vão se comunicar.

### 3. Descobrir o IP de quem vai hospedar

Na máquina que vai abrir o canal (host), no PowerShell:

```powershell
ipconfig
```

Anote o **Endereço IPv4** da rede ativa (ex: `192.168.0.12`). A outra máquina vai precisar desse número.

> As duas máquinas precisam estar na mesma rede local (mesmo Wi-Fi/roteador).

### 4. Iniciar o sistema

Em cada máquina, dentro da pasta do projeto:

```powershell
dotnet run
```

### 5. Escolher o papel

Após o boot, o sistema pergunta:

```
TYPE 1 TO HOST, 2 TO CONNECT
VOID>
```

**Máquina que hospeda:** digite `1`, informe uma porta livre (ex: `5000`), e aguarde:

```
WAITING FOR INCOMING LINK...
```

**Máquina que conecta:** digite `2`, informe o IP anotado no passo 3 e a mesma porta.

### 6. Conversar

Assim que o link é aceito, o prompt muda para `VOID/LINK/<node>>`. A partir daí, basta digitar e pressionar Enter — mensagens podem ser enviadas dos dois lados a qualquer momento, sem esperar turno.

### 7. Encerrar

Digite `/exit` em qualquer um dos lados para encerrar o link.

> **Importante:** o host precisa estar ativo e aguardando *antes* de a outra máquina tentar conectar. Tentar conectar antes disso resulta em `[!] CONNECTION REFUSED`.

---

## Limitações conhecidas / próximos passos

- Conexão restrita à mesma rede local (sem NAT traversal ou relay)
- Sem framing de mensagens — mensagens enviadas em sequência muito rápida podem ser recebidas concatenadas no mesmo pacote
- Sem persistência de histórico — a conversa existe apenas durante o link ativo
- Sem autenticação — qualquer node que saiba IP e porta pode se conectar
- Suporte a múltiplos nodes simultâneos (mais de uma conexão por host) ainda não implementado

---

*VOID is a terminal-based communication system designed for private network communication.*
*VERSION 1.0.0 — BUILD 2026 — CORE: C#*
