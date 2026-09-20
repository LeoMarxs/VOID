using System.Net;
using System.Net.Sockets;
using System.Text;
using Void;

await Theme.PlayBootSequenceAsync();

Theme.Line("TYPE 1 TO HOST, 2 TO CONNECT", Theme.Dim);
Theme.PrintPrompt("VOID");
var opcao = Console.ReadLine();

TcpClient client;
string peerLabel; // usado no VOID/LINK/<nome> depois de conectar

if (opcao == "1")
{
    Theme.Inline("PORT TO LISTEN (ex: 5000): ", Theme.Text);
    int porta = int.Parse(Console.ReadLine()!);

    var listener = new TcpListener(IPAddress.Any, porta);
    listener.Start();
    Theme.Info_($"LISTENING ON PORT {porta}");
    Theme.Line("      (find your local IP with: ipconfig)", Theme.Dim);
    Theme.Line("");
    Theme.Line("WAITING FOR INCOMING LINK...", Theme.Dim);

    client = await listener.AcceptTcpClientAsync();
    var remoteEp = client.Client.RemoteEndPoint as IPEndPoint;
    peerLabel = remoteEp?.Address.ToString() ?? "UNKNOWN";
    Theme.Plus($"NODE \"{peerLabel}\" CONNECTED");
}
else
{
    Theme.Inline("HOST IP (ex: 192.168.0.10): ", Theme.Text);
    string ip = Console.ReadLine()!;
    Theme.Inline("PORT: ", Theme.Text);
    int porta = int.Parse(Console.ReadLine()!);

    client = new TcpClient();
    Theme.Line("ESTABLISHING LINK...", Theme.Dim);
    try
    {
        await client.ConnectAsync(ip, porta);
    }
    catch (SocketException)
    {
        Theme.Bang("CONNECTION REFUSED");
        return;
    }
    peerLabel = ip;
    Theme.Plus("LINK ESTABLISHED");
}

Theme.Line("");
Theme.Line("TYPE MESSAGES AND PRESS ENTER. TYPE /EXIT TO TERMINATE LINK.", Theme.Dim);
Theme.Line("");

var stream = client.GetStream();
string promptContext = $"VOID/LINK/{peerLabel}";

// Task de leitura: fica escutando o que a outra máquina manda,
// roda em paralelo enquanto o loop principal escreve.
var tarefaLeitura = Task.Run(async () =>
{
    var buffer = new byte[4096];
    try
    {
        while (client.Connected)
        {
            int bytesLidos = await stream.ReadAsync(buffer);
            if (bytesLidos == 0) break; // outro lado fechou a conexão

            string mensagem = Encoding.UTF8.GetString(buffer, 0, bytesLidos);

            // limpa a linha do prompt atual antes de imprimir a mensagem recebida
            Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
            Theme.IncomingTag();
            Theme.Line($"[{Theme.Timestamp()}] <{peerLabel}>  {mensagem}", Theme.Text);
            Theme.PrintPrompt(promptContext);
        }
    }
    catch (IOException)
    {
        // conexão caiu
    }

    Theme.Line("");
    Theme.Minus("CONNECTION TERMINATED BY REMOTE NODE");
    Theme.Line("PRESS ENTER TO EXIT.", Theme.Dim);
});

// Loop principal: lê o que você digita e envia
while (true)
{
    Theme.PrintPrompt(promptContext);
    string? texto = Console.ReadLine();

    if (texto is null || texto == "/exit" || !client.Connected)
        break;

    if (string.IsNullOrWhiteSpace(texto))
        continue;

    byte[] dados = Encoding.UTF8.GetBytes(texto);
    try
    {
        await stream.WriteAsync(dados);
    }
    catch (IOException)
    {
        Theme.Bang("TRANSMISSION FAILED — LINK LOST");
        break;
    }
}

client.Close();
Theme.Line("");
Theme.Minus("LINK CLOSED");