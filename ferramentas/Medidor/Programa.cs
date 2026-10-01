using System.Diagnostics;
using System.Numerics;

using Conde.Combinatoria;

// A ferramenta de medida. Ela REGISTRA numeros; nenhuma afirmacao de tempo mora
// na suite de testes, porque uma afirmacao de relogio falha sem que nada tenha
// piorado, basta a maquina do CI estar ocupada.

var comando = args.Length > 0 ? args[0] : "ajuda";

switch (comando)
{
    case "estourar": Estourar(); break;
    case "crescer": Crescer(); break;
    case "numerar": Numerar(); break;
    case "gray": Gray(); break;
    default:
        Console.WriteLine("uso: medidor <estourar|crescer|numerar|gray>");
        break;
}

static void Estourar()
{
    Console.WriteLine();
    Console.WriteLine("  Onde cada caminho quebra. A combinatoria estoura os tipos de tamanho");
    Console.WriteLine("  fixo cedo demais para fingir que nao.");
    Console.WriteLine();

    static int OndeOFatorialEstoura(int bits)
    {
        BigInteger limite = (BigInteger.One << bits) - 1;

        BigInteger atual = 1;

        for (var n = 1; ; n++)
        {
            atual *= n;

            if (atual > limite)
            {
                return n;
            }
        }
    }

    Console.WriteLine("    o fatorial de que numero NAO cabe em:");
    Console.WriteLine($"      int    (31 bits)     {OndeOFatorialEstoura(31),4}!");
    Console.WriteLine($"      long   (63 bits)     {OndeOFatorialEstoura(63),4}!");
    Console.WriteLine($"      UInt128             {OndeOFatorialEstoura(128),4}!");
    Console.WriteLine();

    // O binomial pelo caminho dos fatoriais quebra muito antes do que o
    // resultado exigiria, porque ele constroi tres numeros gigantescos para
    // devolver um pequeno.
    Console.WriteLine("    C(n, n/2): quantos digitos tem o RESULTADO, e quantos tem n!");
    Console.WriteLine();
    Console.WriteLine("         n   digitos de C(n,n/2)   digitos de n!   cabe em long?");
    Console.WriteLine("  " + new string('-', 68));

    foreach (var n in (int[])[20, 40, 60, 62, 64, 100])
    {
        var binomial = Contagem.Binomial(n, n / 2);

        var fatorial = Contagem.Fatorial(n);

        var cabe = binomial <= long.MaxValue ? "sim" : "nao";

        Console.WriteLine($"  {n,8}   {binomial.ToString().Length,19}   {fatorial.ToString().Length,13}"
            + $"   {cabe,13}");
    }

    Console.WriteLine();
    Console.WriteLine("  C(62,31) ainda cabe num long com folga, e 62! tem 86 digitos. Quem");
    Console.WriteLine("  calcula o binomial dividindo fatoriais perde essa faixa inteira sem");
    Console.WriteLine("  precisar, e o erro nao aparece como numero errado: aparece como");
    Console.WriteLine("  excecao de estouro, ou pior, como numero negativo em aritmetica que");
    Console.WriteLine("  nao confere estouro.");
    Console.WriteLine();
}

static void Crescer()
{
    Console.WriteLine();
    Console.WriteLine("  Quantos digitos tem cada contagem. E o jeito honesto de comparar");
    Console.WriteLine("  numeros que nenhum tipo guarda.");
    Console.WriteLine();
    Console.WriteLine("         n        n!    C(2n,n)    Bell(n)       p(n)   desarranjos");
    Console.WriteLine("  " + new string('-', 70));

    foreach (var n in (int[])[5, 10, 25, 50, 100, 250, 500])
    {
        Console.WriteLine($"  {n,8}  {Contagem.Fatorial(n).ToString().Length,8}"
            + $"  {Contagem.Binomial(2 * n, n).ToString().Length,9}"
            + $"  {Contagem.Bell(n).ToString().Length,9}"
            + $"  {Contagem.Particoes(n).ToString().Length,9}"
            + $"  {Contagem.Desarranjos(n).ToString().Length,13}");
    }

    Console.WriteLine();
    Console.WriteLine("  O p(n) e o unico que cresce devagar, e e o unico sem formula fechada.");
    Console.WriteLine("  Os digitos dele crescem com a RAIZ de n, que e o teorema de Hardy e");
    Console.WriteLine("  Ramanujan de 1918.");
    Console.WriteLine();
    Console.WriteLine("  Eu tinha escrito aqui que o Bell ultrapassa o fatorial a partir de um");
    Console.WriteLine("  certo ponto. A tabela diz o contrario, e com folga: em n = 500 o");
    Console.WriteLine("  fatorial tem 1.135 digitos e o Bell tem 844. A conta explica por que:");
    Console.WriteLine("  o logaritmo do fatorial por n vale ln(n) - 1, e o do Bell vale");
    Console.WriteLine("  ln(n) - ln(ln(n)) - 1. O segundo e menor para todo n a partir de 16,");
    Console.WriteLine("  e a distancia so aumenta.");
    Console.WriteLine();

    Console.WriteLine($"    p(100)  = {Contagem.Particoes(100)}");
    Console.WriteLine($"    p(1000) = {Contagem.Particoes(1_000)}");
    Console.WriteLine();
}

static void Numerar()
{
    Console.WriteLine();
    Console.WriteLine("  Numerar contra enumerar. As duas respondem a mesma pergunta, e so uma");
    Console.WriteLine("  delas termina.");
    Console.WriteLine();

    // Quantas permutacoes por segundo a enumeracao visita?
    var relogio = Stopwatch.StartNew();

    var visitadas = 0L;

    foreach (var _ in Geracao.Permutacoes(10))
    {
        visitadas++;
    }

    var segundos = relogio.Elapsed.TotalSeconds;

    var porSegundo = visitadas / segundos;

    Console.WriteLine($"    enumerando as {visitadas:n0} permutacoes de 10 elementos:");
    Console.WriteLine($"      {segundos * 1000:n0} ms, ou {porSegundo:n0} por segundo");
    Console.WriteLine();

    // E a numeracao?
    var sorteio = new Random(29);

    const int N = 30;

    var quantas = Contagem.Fatorial(N);

    relogio.Restart();

    for (var i = 0; i < 100_000; i++)
    {
        var bytes = new byte[16];

        sorteio.NextBytes(bytes);

        bytes[^1] &= 0x7F;

        Ordem.DesranquearPermutacao(N, new BigInteger(bytes) % quantas);
    }

    var porCada = relogio.Elapsed.TotalMilliseconds / 100_000;

    Console.WriteLine($"    sorteando uma das {quantas} permutacoes de {N} elementos,");
    Console.WriteLine($"    uniformemente, pelo numero: {porCada * 1_000:n1} microssegundos cada.");
    Console.WriteLine();

    var anos = (double)quantas / porSegundo / 60 / 60 / 24 / 365;

    Console.WriteLine($"    chegar na ultima enumerando levaria {anos:0.0e+0} anos.");
    Console.WriteLine();
    Console.WriteLine("  Nao e uma diferenca de constante nem de algoritmo: e a diferenca entre");
    Console.WriteLine("  um problema que tem resposta e um que nao tem.");
    Console.WriteLine();
}

static void Gray()
{
    const int Elementos = 22;

    Console.WriteLine();
    Console.WriteLine($"  Somar os pesos de todos os {1L << Elementos:n0} subconjuntos de {Elementos} elementos.");
    Console.WriteLine();

    var pesos = new long[Elementos];

    var sorteio = new Random(29);

    for (var i = 0; i < Elementos; i++)
    {
        pesos[i] = sorteio.Next(1, 1_000_000);
    }

    // Do jeito direto: para cada subconjunto, soma os elementos dele.
    var relogio = Stopwatch.StartNew();

    long total = 0;

    for (var mascara = 0u; mascara < 1u << Elementos; mascara++)
    {
        long soma = 0;

        for (var i = 0; i < Elementos; i++)
        {
            if ((mascara >> i & 1) != 0)
            {
                soma += pesos[i];
            }
        }

        total += soma;
    }

    var direto = relogio.Elapsed.TotalMilliseconds;

    // Na ordem do codigo de Gray: cada passo troca UM elemento, entao a soma se
    // atualiza com uma adicao ou uma subtracao.
    relogio.Restart();

    long acumulado = 0;

    long outroTotal = 0;

    uint anterior = 0;

    foreach (var mascara in Geracao.SubconjuntosEmGray(Elementos))
    {
        var mudou = mascara ^ anterior;

        if (mudou != 0)
        {
            var qual = System.Numerics.BitOperations.TrailingZeroCount(mudou);

            acumulado += (mascara >> qual & 1) != 0 ? pesos[qual] : -pesos[qual];
        }

        outroTotal += acumulado;

        anterior = mascara;
    }

    var emGray = relogio.Elapsed.TotalMilliseconds;

    // A mesma coisa sem o enumerador preguicoso, para separar o ganho do truque
    // do custo da chamada de interface a cada passo.
    relogio.Restart();

    long semIterador = 0;

    long acumuladoDireto = 0;

    uint anteriorDireto = 0;

    for (var i = 0u; i < 1u << Elementos; i++)
    {
        var mascara = Ordem.ParaGray(i);

        var mudou = mascara ^ anteriorDireto;

        if (mudou != 0)
        {
            var qual = System.Numerics.BitOperations.TrailingZeroCount(mudou);

            acumuladoDireto += (mascara >> qual & 1) != 0 ? pesos[qual] : -pesos[qual];
        }

        semIterador += acumuladoDireto;

        anteriorDireto = mascara;
    }

    var direitoAoPonto = relogio.Elapsed.TotalMilliseconds;

    Console.WriteLine($"    recalculando cada um    {direto,9:0.0} ms");
    Console.WriteLine($"    em ordem de Gray        {emGray,9:0.0} ms      {direto / emGray,5:0.0}x");
    Console.WriteLine($"    idem, sem o enumerador  {direitoAoPonto,9:0.0} ms      {direto / direitoAoPonto,5:0.0}x");
    Console.WriteLine();
    Console.WriteLine($"    os totais batem?        {(total == outroTotal && total == semIterador ? "sim" : "NAO")}");
    Console.WriteLine();
    Console.WriteLine("  Eu previ 22x, que e o numero de elementos, porque e o laco de dentro");
    Console.WriteLine("  que some. Errei para os dois lados, e as duas ultimas linhas dizem");
    Console.WriteLine("  por que.");
    Console.WriteLine();
    Console.WriteLine("  Com o enumerador preguicoso da 6,8x: quase todo o tempo que sobra e");
    Console.WriteLine("  chamada de interface, uma por subconjunto, e ela custa mais do que a");
    Console.WriteLine("  conta que o truque economizou.");
    Console.WriteLine();
    Console.WriteLine("  Sem ele da 52x, que e MAIS que os 22 elementos. Cada elemento do laco");
    Console.WriteLine("  de dentro nao custa uma soma: custa um deslocamento, uma mascara e um");
    Console.WriteLine("  desvio que o processador nao consegue prever, porque o bit e");
    Console.WriteLine("  essencialmente aleatorio. Trocar isso por uma soma e uma subtracao vale");
    Console.WriteLine("  mais do que o numero de elementos sugere.");
    Console.WriteLine();
}
