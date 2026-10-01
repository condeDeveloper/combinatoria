using System.Numerics;

namespace Conde.Combinatoria;

/// <summary>
/// Contar sem enumerar: binomiais, Catalan, Stirling, Bell, desarranjos e
/// partições.
/// </summary>
/// <remarks>
/// <para>
/// Toda função aqui responde "quantos" sem construir um só objeto. É a diferença
/// entre saber que existem 100.891.344.545.564.193.334.812.497.256 maneiras de
/// escolher cinquenta entre cem e tentar listá-las.
/// </para>
/// <para>
/// Os resultados são <c>BigInteger</c> porque a combinatória estoura qualquer
/// tipo de tamanho fixo cedo demais para fingir que não. O fatorial de 21 já não
/// cabe num <c>long</c>, e um dos testes mede exatamente onde cada caminho
/// quebra.
/// </para>
/// </remarks>
public static class Contagem
{
    /// <summary>
    /// O binomial, calculado por multiplicações e divisões alternadas.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A forma com fatoriais, <c>n! / (k! · (n-k)!)</c>, é correta e inútil: ela
    /// constrói três números gigantescos para devolver um pequeno. <c>C(60,30)</c>
    /// cabe num <c>long</c>, mas <c>60!</c> tem 82 dígitos.
    /// </para>
    /// <para>
    /// Aqui o produto é parcial e a divisão acontece a cada passo. Ela é sempre
    /// exata porque o produto de <c>i</c> inteiros consecutivos é divisível por
    /// <c>i!</c>, e esse produto parcial é justamente um binomial.
    /// </para>
    /// </remarks>
    public static BigInteger Binomial(int n, int k)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), n, "não é negativo");
        }

        if (k < 0 || k > n)
        {
            return BigInteger.Zero;
        }

        k = Math.Min(k, n - k);

        BigInteger resultado = 1;

        for (var i = 1; i <= k; i++)
        {
            resultado = resultado * (n - k + i) / i;
        }

        return resultado;
    }

    /// <summary>O fatorial.</summary>
    public static BigInteger Fatorial(int n)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), n, "não é negativo");
        }

        BigInteger resultado = 1;

        for (var i = 2; i <= n; i++)
        {
            resultado *= i;
        }

        return resultado;
    }

    /// <summary>
    /// O número de Catalan, pela fórmula fechada.
    /// </summary>
    /// <remarks>
    /// Conta parênteses balanceados, árvores binárias com n nós, caminhos que não
    /// cruzam a diagonal, e mais umas duzentas coisas que acabam sendo a mesma.
    /// A fórmula é <c>C(2n,n) / (n+1)</c>, e a divisão é exata, o que não é óbvio
    /// olhando.
    /// </remarks>
    public static BigInteger Catalan(int n)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), n, "não é negativo");
        }

        return Binomial(2 * n, n) / (n + 1);
    }

    /// <summary>
    /// Os números de Stirling de segunda espécie: em quantas maneiras <c>n</c>
    /// objetos distintos se repartem em <c>k</c> grupos não vazios.
    /// </summary>
    /// <remarks>
    /// A recorrência é a pergunta "o último objeto fica sozinho ou acompanhado":
    /// se fica sozinho, sobram <c>k-1</c> grupos para os outros; se acompanha,
    /// há <c>k</c> grupos onde encaixá-lo.
    /// </remarks>
    public static BigInteger Stirling2(int n, int k)
    {
        if (n < 0 || k < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "não são negativos");
        }

        if (k > n)
        {
            return BigInteger.Zero;
        }

        var linha = new BigInteger[k + 1];

        // A linha comeca valendo S(0, .), que e 1 no zero e zero no resto.
        linha[0] = 1;

        for (var i = 1; i <= n; i++)
        {
            // De tras para frente: a posicao j usa a j-1 da linha ANTERIOR, e
            // escrever na ordem crescente estragaria essa leitura.
            for (var j = Math.Min(i, k); j >= 1; j--)
            {
                linha[j] = j * linha[j] + linha[j - 1];
            }

            linha[0] = 0;
        }

        return linha[k];
    }

    /// <summary>
    /// O número de Bell: em quantas maneiras <c>n</c> objetos se repartem em
    /// grupos, de qualquer quantidade.
    /// </summary>
    /// <remarks>
    /// É a soma dos Stirling de segunda espécie da linha, e aqui sai pelo
    /// triângulo de Peirce, que é outro caminho: cada linha começa com o último
    /// número da anterior e cada termo é o vizinho da esquerda mais o de cima.
    /// Os testes conferem que os dois caminhos dão o mesmo.
    /// </remarks>
    public static BigInteger Bell(int n)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), n, "não é negativo");
        }

        var linha = new BigInteger[] { 1 };

        for (var i = 1; i <= n; i++)
        {
            var proxima = new BigInteger[i + 1];

            proxima[0] = linha[^1];

            for (var j = 1; j <= i; j++)
            {
                proxima[j] = proxima[j - 1] + linha[j - 1];
            }

            linha = proxima;
        }

        return linha[0];
    }

    /// <summary>
    /// Os desarranjos: permutações em que nenhum elemento fica no lugar.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A recorrência <c>D(n) = n · D(n-1) + (-1)^n</c> é mais curta que a
    /// inclusão e exclusão e menos famosa. Ela sai da fórmula com fatoriais depois
    /// de um pouco de álgebra.
    /// </para>
    /// <para>
    /// A proporção de desarranjos tende a <c>1/e</c>, e tende rápido: já em
    /// <c>n = 10</c> a diferença está na sétima casa. É o que faz o problema do
    /// amigo oculto ter resposta praticamente constante.
    /// </para>
    /// </remarks>
    public static BigInteger Desarranjos(int n)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), n, "não é negativo");
        }

        BigInteger atual = 1;

        for (var i = 1; i <= n; i++)
        {
            atual = i * atual + (i % 2 == 0 ? 1 : -1);
        }

        return atual;
    }

    /// <summary>
    /// As partições de um inteiro: de quantas maneiras <c>n</c> é soma de inteiros
    /// positivos, sem contar a ordem.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Pelo caminho direto, que é uma soma sobre as parcelas possíveis e custa
    /// <c>O(n²)</c>. O outro caminho, o teorema dos números pentagonais de Euler,
    /// custa <c>O(n^1.5)</c> e está na função ao lado.
    /// </para>
    /// <para>
    /// <c>p(100)</c> vale 190.569.292, e esse número é o começo da história: foi
    /// MacMahon calculando partições à mão até 200 que deu a Hardy e Ramanujan a
    /// tabela contra a qual eles conferiram a fórmula assintótica deles.
    /// </para>
    /// </remarks>
    public static BigInteger Particoes(int n)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), n, "não é negativo");
        }

        var quantas = new BigInteger[n + 1];

        quantas[0] = 1;

        for (var parcela = 1; parcela <= n; parcela++)
        {
            for (var alvo = parcela; alvo <= n; alvo++)
            {
                quantas[alvo] += quantas[alvo - parcela];
            }
        }

        return quantas[n];
    }

    /// <summary>
    /// As partições pelo teorema dos números pentagonais de Euler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>p(n) = p(n-1) + p(n-2) - p(n-5) - p(n-7) + p(n-12) + …</c>, onde os
    /// índices são os números pentagonais generalizados e os sinais vêm em pares.
    /// Como esses números crescem quadraticamente, cada termo custa
    /// <c>O(raiz de n)</c> em vez de <c>O(n)</c>.
    /// </para>
    /// <para>
    /// Está aqui como segundo caminho, e é um bom segundo caminho justamente por
    /// não se parecer nem um pouco com o primeiro.
    /// </para>
    /// </remarks>
    public static BigInteger ParticoesPorEuler(int n)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), n, "não é negativo");
        }

        var p = new BigInteger[n + 1];

        p[0] = 1;

        for (var alvo = 1; alvo <= n; alvo++)
        {
            BigInteger soma = 0;

            for (var k = 1; ; k++)
            {
                var pentagonal = k * (3 * k - 1) / 2;

                if (pentagonal > alvo)
                {
                    break;
                }

                var sinal = k % 2 == 1 ? 1 : -1;

                soma += sinal * p[alvo - pentagonal];

                var outro = k * (3 * k + 1) / 2;

                if (outro <= alvo)
                {
                    soma += sinal * p[alvo - outro];
                }
            }

            p[alvo] = soma;
        }

        return p[n];
    }
}
