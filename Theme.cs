using System.Text;

namespace Void;

/// <summary>
/// Paleta e vocabulário visual do VØID.
/// Preto + cinza + um único destaque (verde ácido) — sem RGB gamer.
/// </summary>
public static class Theme
{
    // Verde ácido (#00FF9C) via ANSI truecolor — funciona em PowerShell moderno (Win10+)
    public const string Accent   = "\u001b[38;2;0;255;156m";
    public const string Info     = "\u001b[38;2;0;200;255m"; // ciano
    public const string Alert    = "\u001b[38;2;255;59;92m"; // vermelho
    public const string Dim      = "\u001b[38;2;112;112;112m"; // cinza secundário
    public const string Text     = "\u001b[38;2;214;214;214m"; // texto principal
    public const string Reset    = "\u001b[0m";
    public const string Bold     = "\u001b[1m";

    public static void Line(string text, string color = Text)
        => Console.WriteLine($"{color}{text}{Reset}");

    public static void Inline(string text, string color = Text)
        => Console.Write($"{color}{text}{Reset}");

    // [+] sucesso / conexão / entrada
    public static void Plus(string msg) => Line($"[+] {msg}", Accent);

    // [-] desconexão / saída
    public static void Minus(string msg) => Line($"[-] {msg}", Dim);

    // [!] erro / alerta
    public static void Bang(string msg) => Line($"[!] {msg}", Alert);

    // [i] informação neutra
    public static void Info_(string msg) => Line($"[i] {msg}", Info);

    // [MSG] transmissão recebida
    public static void IncomingTag() => Line("[MSG] INCOMING TRANSMISSION", Info);

    public static string Timestamp() => DateTime.Now.ToString("HH:mm:ss");

    /// <summary>Boot sequence com delay artificial — só roda uma vez, na abertura.</summary>
    public static async Task PlayBootSequenceAsync()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Clear();

        Line("");
        Line("                    V Ø I D", Bold + Accent);
        Line("");
        Line("              COMMUNICATION SYSTEM", Dim);
        Line("                  VERSION 1.0.0", Dim);
        Line("");
        Line(new string('─', 50), Dim);
        Line("");

        await BootStep("Initializing VOID Core");
        await BootStep("Loading network module");
        await BootStep("Loading communication module");
        await BootStep("Loading user environment");

        Line("");
        Line(new string('─', 50), Dim);
        Line("");
        Line("             ENTER THE VOID.", Accent);
        Line("");
    }

    private static async Task BootStep(string label)
    {
        Inline($"[BOOT] {label}", Text);
        Inline(new string('.', 40 - label.Length), Dim);
        await Task.Delay(120);
        Line(" OK", Accent);
    }

    public static void PrintPrompt(string context)
        => Inline($"{context}> ", Accent);
}