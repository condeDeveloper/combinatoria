namespace Conde.Combinatoria;

/// <summary>
/// Gerar os objetos um a um, sem guardar a lista inteira.
/// </summary>
/// <remarks>
/// Oito elementos dão 40.320 permutações, e doze dão 479 milhões. Devolver uma
/// lista pronta deixa de ser possível muito antes de o problema deixar de ser
/// interessante, então tudo aqui é preguiçoso e reutiliza o mesmo vetor.
/// </remarks>
public static class Geracao
{
    /// <summary>
    /// Avança para a próxima permutação na ordem lexicográfica, no lugar.
    /// </summary>
    /// <returns>Falso quando já estava na última, isto é, na ordem decrescente.</returns>
    /// <remarks>
    /// <para>
    /// O mesmo algoritmo do <c>next_permutation</c> do C++, e ele é mais esperto
    /// do que parece. Acha a última posição que ainda pode crescer, troca pelo
    /// menor valor maior que ela que esteja à direita, e inverte o rabo. A
    /// inversão é o passo que costuma ser esquecido, e sem ela a ordem fica
    /// quase certa.
    /// </para>
    /// <para>
    /// Ele funciona com valores repetidos, e aí visita cada arranjo distinto uma
    /// vez só em vez de <c>n!</c> vezes. É o que separa este algoritmo do que
    /// simplesmente troca posições.
    /// </para>
    /// </remarks>
    public static bool Proxima(int[] valores)
    {
        ArgumentNullException.ThrowIfNull(valores);

        var i = valores.Length - 2;

        while (i >= 0 && valores[i] >= valores[i + 1])
        {
            i--;
        }

        if (i < 0)
        {
            return false;
        }

        var j = valores.Length - 1;

        while (valores[j] <= valores[i])
        {
            j--;
        }

        (valores[i], valores[j]) = (valores[j], valores[i]);

        Array.Reverse(valores, i + 1, valores.Length - i - 1);

        return true;
    }

    /// <summary>Todas as permutações de 0 a n-1, em ordem lexicográfica.</summary>
    /// <remarks>
    /// O vetor devolvido é sempre o mesmo, reaproveitado. Quem precisa guardar
    /// copia; quem só lê não paga nada.
    /// </remarks>
    public static IEnumerable<int[]> Permutacoes(int n)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), n, "não é negativo");
        }

        var atual = Enumerable.Range(0, n).ToArray();

        do
        {
            yield return atual;
        }
        while (Proxima(atual));
    }

    /// <summary>
    /// Todas as permutações pelo algoritmo de Heap, em que cada passo é uma troca
    /// só.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A ordem não é a lexicográfica, e é esse o ponto: duas permutações
    /// consecutivas aqui diferem por <b>uma transposição</b>, o que faz dele o
    /// equivalente do código de Gray para permutações.
    /// </para>
    /// <para>
    /// Serve quando avaliar uma permutação custa caro e dá para atualizar a
    /// avaliação a partir de uma troca, em vez de recalcular do zero.
    /// </para>
    /// </remarks>
    public static IEnumerable<int[]> PermutacoesDeHeap(int n)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), n, "não é negativo");
        }

        var atual = Enumerable.Range(0, n).ToArray();

        yield return atual;

        if (n < 2)
        {
            yield break;
        }

        // A forma iterativa: o vetor de contadores faz o papel da pilha de
        // chamadas da forma recursiva.
        var contadores = new int[n];

        var nivel = 1;

        while (nivel < n)
        {
            if (contadores[nivel] < nivel)
            {
                var outro = nivel % 2 == 0 ? 0 : contadores[nivel];

                (atual[outro], atual[nivel]) = (atual[nivel], atual[outro]);

                yield return atual;

                contadores[nivel]++;

                nivel = 1;
            }
            else
            {
                contadores[nivel] = 0;

                nivel++;
            }
        }
    }

    /// <summary>Todas as combinações de k elementos entre n, em ordem lexicográfica.</summary>
    public static IEnumerable<int[]> Combinacoes(int n, int k)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), n, "não é negativo");
        }

        if (k < 0 || k > n)
        {
            yield break;
        }

        var atual = Enumerable.Range(0, k).ToArray();

        while (true)
        {
            yield return atual;

            // A última posição que ainda pode crescer sem empurrar as de depois
            // para fora.
            var i = k - 1;

            while (i >= 0 && atual[i] == n - k + i)
            {
                i--;
            }

            if (i < 0)
            {
                yield break;
            }

            atual[i]++;

            for (var j = i + 1; j < k; j++)
            {
                atual[j] = atual[j - 1] + 1;
            }
        }
    }

    /// <summary>
    /// Todos os subconjuntos de n elementos, na ordem do código de Gray.
    /// </summary>
    /// <remarks>
    /// Cada passo entra ou sai <b>um</b> elemento, e o valor devolvido é a máscara
    /// de bits. Percorrer os subconjuntos assim é o que permite manter uma soma,
    /// um produto ou um ou-exclusivo acumulado sem recalcular a cada passo.
    /// </remarks>
    public static IEnumerable<uint> SubconjuntosEmGray(int n)
    {
        if (n is < 0 or > 31)
        {
            throw new ArgumentOutOfRangeException(nameof(n), n, "está entre 0 e 31");
        }

        var quantos = 1u << n;

        for (var i = 0u; i < quantos; i++)
        {
            yield return Ordem.ParaGray(i);
        }
    }
}
