using Xunit;

namespace Conde.Combinatoria.Testes;

/// <summary>
/// Os geradores contra a contagem, e contra as invariantes que cada um promete.
/// </summary>
/// <remarks>
/// Um gerador combinatório erra de três jeitos: pula objetos, repete objetos, ou
/// sai fora da ordem prometida. A contagem pega os dois primeiros, e só a
/// comparação explícita pega o terceiro.
/// </remarks>
public class GeracaoTest
{
    [Fact(DisplayName = "a proxima permutacao visita todas as n!, em ordem, para n ate 8")]
    public void TodasEmOrdem()
    {
        for (var n = 0; n <= 8; n++)
        {
            var quantas = (int)Contagem.Fatorial(n);

            int[]? anterior = null;

            var sairam = 0;

            foreach (var permutacao in Geracao.Permutacoes(n))
            {
                if (anterior is not null)
                {
                    // Estritamente crescente na ordem lexicografica: nem igual,
                    // nem fora de ordem.
                    Assert.True(Lexicografica(anterior, permutacao) < 0,
                        $"[{string.Join(",", anterior)}] nao vem antes de "
                        + $"[{string.Join(",", permutacao)}]");
                }

                anterior = permutacao.ToArray();

                sairam++;
            }

            Assert.Equal(quantas, sairam);

            // A ultima e a decrescente, e dali a Proxima devolve falso.
            if (n > 0)
            {
                Assert.Equal(Enumerable.Range(0, n).Reverse(), anterior);
            }
        }

        Assert.Throws<ArgumentNullException>(() => Geracao.Proxima(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => Geracao.Permutacoes(-1).ToList());
        Assert.Throws<ArgumentOutOfRangeException>(() => Geracao.PermutacoesDeHeap(-1).ToList());
        Assert.Throws<ArgumentOutOfRangeException>(() => Geracao.Combinacoes(-1, 0).ToList());
        Assert.Throws<ArgumentOutOfRangeException>(() => Geracao.SubconjuntosEmGray(32).ToList());
    }

    private static int Lexicografica(int[] um, int[] outro)
    {
        for (var i = 0; i < um.Length; i++)
        {
            if (um[i] != outro[i])
            {
                return um[i].CompareTo(outro[i]);
            }
        }

        return 0;
    }

    [Fact(DisplayName = "com valores repetidos ela visita cada arranjo UMA vez, e nao n! vezes")]
    public void ComValoresRepetidos()
    {
        // A propriedade que separa este algoritmo de qualquer troca de posicoes:
        // com [0,0,1,1,2] ele visita os 30 arranjos distintos, e nao os 120 que
        // existiriam se os iguais fossem distinguiveis.
        //
        // A conta e o coeficiente multinomial: 5! / (2! . 2! . 1!) = 30.
        int[] valores = [0, 0, 1, 1, 2];

        var vistos = new HashSet<string>();

        var sairam = 0;

        do
        {
            Assert.True(vistos.Add(string.Join(",", valores)),
                $"[{string.Join(",", valores)}] saiu duas vezes");

            sairam++;
        }
        while (Geracao.Proxima(valores));

        Assert.Equal(30, sairam);

        var esperado = Contagem.Fatorial(5)
            / (Contagem.Fatorial(2) * Contagem.Fatorial(2) * Contagem.Fatorial(1));

        Assert.True(esperado == sairam, $"o multinomial deu {esperado} e sairam {sairam}");

        // Todos iguais: um arranjo so.
        int[] iguais = [7, 7, 7, 7];

        Assert.False(Geracao.Proxima(iguais));

        // E um vetor vazio ou de um elemento nao tem proxima.
        Assert.False(Geracao.Proxima([]));
        Assert.False(Geracao.Proxima([1]));
    }

    [Fact(DisplayName = "o algoritmo de Heap troca UMA dupla por passo, e visita todas")]
    public void OAlgoritmoDeHeap()
    {
        // O equivalente do codigo de Gray para permutacoes. A ordem nao e a
        // lexicografica, e o que ele promete e outra coisa: duas permutacoes
        // consecutivas diferem por exatamente uma transposicao.
        for (var n = 0; n <= 8; n++)
        {
            var quantas = (int)Contagem.Fatorial(n);

            var vistos = new bool[quantas];

            int[]? anterior = null;

            var sairam = 0;

            foreach (var permutacao in Geracao.PermutacoesDeHeap(n))
            {
                var ranque = (int)Ordem.RanquearPermutacao(permutacao);

                Assert.False(vistos[ranque], $"a permutacao de numero {ranque} repetiu");

                vistos[ranque] = true;

                if (anterior is not null)
                {
                    var diferentes = 0;

                    for (var i = 0; i < n; i++)
                    {
                        if (anterior[i] != permutacao[i])
                        {
                            diferentes++;
                        }
                    }

                    Assert.True(diferentes == 2,
                        $"entre duas permutacoes consecutivas mudaram {diferentes} posicoes");
                }

                anterior = permutacao.ToArray();

                sairam++;
            }

            Assert.Equal(quantas, sairam);

            // Nenhum buraco: todas as n! sairam.
            Assert.DoesNotContain(false, vistos);
        }
    }

    [Fact(DisplayName = "as combinacoes saem ordenadas por dentro e em ordem por fora")]
    public void AsCombinacoes()
    {
        for (var n = 0; n <= 14; n++)
        {
            for (var k = 0; k <= n; k++)
            {
                int[]? anterior = null;

                var sairam = 0;

                foreach (var combinacao in Geracao.Combinacoes(n, k))
                {
                    // Ordenada por dentro, e dentro do universo.
                    for (var i = 0; i < k; i++)
                    {
                        Assert.InRange(combinacao[i], 0, n - 1);

                        if (i > 0)
                        {
                            Assert.True(combinacao[i] > combinacao[i - 1]);
                        }
                    }

                    // E em ordem lexicografica por fora.
                    if (anterior is not null)
                    {
                        Assert.True(Lexicografica(anterior, combinacao) < 0);
                    }

                    anterior = combinacao.ToArray();

                    sairam++;
                }

                Assert.True(Contagem.Binomial(n, k) == sairam,
                    $"C({n},{k}) devia dar {Contagem.Binomial(n, k)} e deu {sairam}");
            }
        }

        // k maior que n nao devolve nada, e k negativo tambem nao.
        Assert.Empty(Geracao.Combinacoes(3, 4));
        Assert.Empty(Geracao.Combinacoes(3, -1));

        // k = 0 devolve UMA combinacao, a vazia. Nao zero: ha exatamente uma
        // maneira de escolher nada.
        Assert.Single(Geracao.Combinacoes(5, 0));
    }

    [Fact(DisplayName = "os subconjuntos em Gray: todos, uma vez, trocando um elemento por passo")]
    public void OsSubconjuntosEmGray()
    {
        for (var n = 0; n <= 18; n++)
        {
            var vistos = new bool[1 << n];

            uint? anterior = null;

            var sairam = 0;

            foreach (var mascara in Geracao.SubconjuntosEmGray(n))
            {
                Assert.InRange(mascara, 0u, (uint)((1 << n) - 1));

                Assert.False(vistos[mascara], $"o subconjunto {mascara} repetiu com n = {n}");

                vistos[mascara] = true;

                if (anterior is { } antes)
                {
                    // Um elemento entra ou sai. Nunca dois, nunca nenhum.
                    Assert.Equal(
                        1, System.Numerics.BitOperations.PopCount(mascara ^ antes));
                }

                anterior = mascara;

                sairam++;
            }

            Assert.Equal(1 << n, sairam);

            Assert.DoesNotContain(false, vistos);
        }

        // O primeiro e o conjunto vazio.
        Assert.Equal(0u, Geracao.SubconjuntosEmGray(5).First());

        // E o ultimo difere do primeiro por um bit so, o que faz a sequencia ser
        // um ciclo. E a propriedade que o codificador de posicao usa de verdade:
        // a volta inteira do disco tem de fechar.
        var todos = Geracao.SubconjuntosEmGray(8).ToArray();

        Assert.Equal(1, System.Numerics.BitOperations.PopCount(todos[^1] ^ todos[0]));
    }
}
