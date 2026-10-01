using System.Numerics;

namespace Conde.Combinatoria;

/// <summary>
/// Numerar objetos combinatórios: dar a cada um um número, e devolver o objeto a
/// partir do número.
/// </summary>
/// <remarks>
/// <para>
/// É o que permite sortear uma permutação uniformemente sem listar as outras,
/// guardar uma combinação em um inteiro, ou dividir um trabalho entre máquinas
/// dando a cada uma uma faixa de números. Em todos esses casos, enumerar não é
/// opção: a quantidade é grande demais.
/// </para>
/// <para>
/// A exigência é dura e fácil de conferir: a numeração tem de ser uma
/// <b>bijeção</b> com os números de 0 até a quantidade menos um. Nenhum buraco,
/// nenhuma repetição. Os testes conferem isso varrendo o conjunto inteiro.
/// </para>
/// </remarks>
public static class Ordem
{
    /// <summary>
    /// O número de uma permutação na ordem lexicográfica.
    /// </summary>
    /// <remarks>
    /// <para>
    /// É o sistema de numeração fatorial: a primeira casa vale <c>(n-1)!</c>, a
    /// segunda <c>(n-2)!</c>, e o dígito de cada casa é quantos elementos ainda
    /// não usados são menores que o atual. Esse dígito é o código de Lehmer.
    /// </para>
    /// <para>
    /// A entrada é uma permutação de <c>0</c> a <c>n-1</c>. Qualquer outra coisa é
    /// recusada, porque aceitar valores repetidos daria um número que não
    /// corresponde a nada.
    /// </para>
    /// </remarks>
    public static BigInteger RanquearPermutacao(int[] permutacao)
    {
        ArgumentNullException.ThrowIfNull(permutacao);

        var n = permutacao.Length;

        var usado = new bool[n];

        foreach (var valor in permutacao)
        {
            if (valor < 0 || valor >= n || usado[valor])
            {
                throw new ArgumentException(
                    $"não é uma permutação de 0 a {n - 1}", nameof(permutacao));
            }

            usado[valor] = true;
        }

        Array.Clear(usado);

        BigInteger ranque = 0;

        for (var i = 0; i < n; i++)
        {
            var menores = 0;

            for (var valor = 0; valor < permutacao[i]; valor++)
            {
                if (!usado[valor])
                {
                    menores++;
                }
            }

            ranque += menores * Contagem.Fatorial(n - 1 - i);

            usado[permutacao[i]] = true;
        }

        return ranque;
    }

    /// <summary>A permutação de um número, na ordem lexicográfica.</summary>
    public static int[] DesranquearPermutacao(int n, BigInteger ranque)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), n, "não é negativo");
        }

        if (ranque < 0 || ranque >= Contagem.Fatorial(n))
        {
            throw new ArgumentOutOfRangeException(
                nameof(ranque), ranque, $"está fora de 0 até {n}! - 1");
        }

        var sobrando = Enumerable.Range(0, n).ToList();

        var permutacao = new int[n];

        for (var i = 0; i < n; i++)
        {
            var bloco = Contagem.Fatorial(n - 1 - i);

            var qual = (int)(ranque / bloco);

            permutacao[i] = sobrando[qual];

            sobrando.RemoveAt(qual);

            ranque %= bloco;
        }

        return permutacao;
    }

    /// <summary>
    /// O número de uma combinação, pelo sistema de numeração combinatório.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A soma é <c>C(c0,1) + C(c1,2) + … + C(ck,k)</c>, com os elementos em ordem
    /// crescente. Isso ordena as combinações em ordem <b>colexicográfica</b>, que
    /// é a lexicográfica lida de trás para frente.
    /// </para>
    /// <para>
    /// A escolha não é por capricho. Nessa ordem o número de uma combinação
    /// <b>não depende de n</b>: a combinação {0,2,5} tem o mesmo número num
    /// universo de 6 elementos e num de 6 milhões. Com a ordem lexicográfica, que
    /// é a intuitiva, crescer o universo renumera tudo.
    /// </para>
    /// </remarks>
    public static BigInteger RanquearCombinacao(int[] escolhidos)
    {
        ArgumentNullException.ThrowIfNull(escolhidos);

        BigInteger ranque = 0;

        for (var i = 0; i < escolhidos.Length; i++)
        {
            if (escolhidos[i] < 0)
            {
                throw new ArgumentException("os elementos não são negativos", nameof(escolhidos));
            }

            if (i > 0 && escolhidos[i] <= escolhidos[i - 1])
            {
                throw new ArgumentException("os elementos vêm em ordem crescente", nameof(escolhidos));
            }

            ranque += Contagem.Binomial(escolhidos[i], i + 1);
        }

        return ranque;
    }

    /// <summary>A combinação de <c>k</c> elementos que tem o número dado.</summary>
    /// <remarks>
    /// A volta é gulosa e desce de <c>k</c> até 1: para cada casa, o maior
    /// elemento cujo binomial ainda cabe. Que isso sempre funcione é o teorema que
    /// faz o sistema ser um sistema.
    /// </remarks>
    public static int[] DesranquearCombinacao(int k, BigInteger ranque)
    {
        if (k < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(k), k, "não é negativo");
        }

        if (ranque < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ranque), ranque, "não é negativo");
        }

        var escolhidos = new int[k];

        for (var casa = k; casa >= 1; casa--)
        {
            var maior = casa - 1;

            while (Contagem.Binomial(maior + 1, casa) <= ranque)
            {
                maior++;
            }

            escolhidos[casa - 1] = maior;

            ranque -= Contagem.Binomial(maior, casa);
        }

        return escolhidos;
    }

    /// <summary>
    /// O código de Gray de um número.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uma linha, e ela esconde a propriedade inteira: números consecutivos têm
    /// códigos que diferem em <b>exatamente um bit</b>. Foi inventado para
    /// codificadores de posição, onde ler um valor no meio da transição entre dois
    /// adjacentes daria um número qualquer se vários bits mudassem de uma vez.
    /// </para>
    /// <para>
    /// Em combinatória ele serve para percorrer todos os subconjuntos trocando um
    /// elemento por vez, o que faz cada passo custar <c>O(1)</c> em vez de
    /// <c>O(n)</c>.
    /// </para>
    /// </remarks>
    public static uint ParaGray(uint n) => n ^ (n >> 1);

    /// <summary>A volta do código de Gray.</summary>
    /// <remarks>
    /// A ida é uma linha e a volta não é: cada bit do resultado depende de todos
    /// os bits mais altos, então é preciso propagar. Em cinco passos de deslocamento
    /// e ou-exclusivo, os 32 bits ficam prontos.
    /// </remarks>
    public static uint DeGray(uint cinza)
    {
        cinza ^= cinza >> 16;
        cinza ^= cinza >> 8;
        cinza ^= cinza >> 4;
        cinza ^= cinza >> 2;
        cinza ^= cinza >> 1;

        return cinza;
    }
}
