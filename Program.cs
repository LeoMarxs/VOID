using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("=== Chat P2P (TCP) ===");
Console.WriteLine("1 - Hospedar (aguardar conexão)");
Console.WriteLine("2 - Conectar em um host");
Console.Write("Escolha: ");
var opcao = Console.ReadLine();

TcpClient client;

if (opcao == "1")
{
    Console.Write("Porta para escutar (ex: 5000): ");
    int porta = int.Parse(Console.ReadLine()!);

    var listener = new TcpListener(IPAddress.Any, porta);
    listener.Start();
    Console.WriteLine($"Aguardando conexão na porta {porta}...");
    Console.WriteLine("(descubra seu IP local com: ipconfig)");

    client = await listener.AcceptTcpClientAsync();
    Console.WriteLine("Cliente conectado!");
}
else
{
    Console.Write("IP do host (ex: 192.168.0.10): ");
    string ip = Console.ReadLine()!;
    Console.Write("Porta: ");
    int porta = int.Parse(Console.ReadLine()!);

    client = new TcpClient();
    await client.ConnectAsync(ip, porta);
    Console.WriteLine("Conectado ao host!");
}

Console.WriteLine("Digite mensagens e pressione Enter. Digite /sair para encerrar.\n");

var stream = client.GetStream();

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
            Console.WriteLine($"\r> {mensagem}");
            Console.Write("Você: ");
        }
    }
    catch (IOException)
    {
        // conexão caiu
    }
    Console.WriteLine("\nConexão encerrada pelo outro lado. Pressione Enter para sair.");
});

// Loop principal: lê o que você digita e envia
while (true)
{
    Console.Write("Você: ");
    string? texto = Console.ReadLine();

    if (texto is null || texto == "/sair" || !client.Connected)
        break;

    byte[] dados = Encoding.UTF8.GetBytes(texto);
    try
    {
        await stream.WriteAsync(dados);
    }
    catch (IOException)
    {
        Console.WriteLine("Falha ao enviar. Conexão perdida.");
        break;
    }
}

client.Close();
Console.WriteLine("Chat encerrado.");
