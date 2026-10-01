using System.Numerics;

using Xunit;

namespace Conde.Combinatoria.Testes;

/// <summary>
/// A numeração tem de ser uma bijeção, e isso se confere varrendo o conjunto.
/// </summary>
/// <remarks>
/// <para>
/// Numerar objetos é fácil de fazer quase certo. O defeito típico não é um número
/// absurdo, é um buraco: dois objetos com o mesmo número, ou um número que não
/// corresponde a objeto nenhum. Quem sorteia uma permutação pelo número passa a
/// sortear torto, e nada quebra.
/// </para>
/// <para>
/// A única conferência que pega isso é a varredura: gerar os 40.320 objetos,
/// numerar cada um, e exigir que os números sejam exatamente 0 até 40.319, sem
/// repetir.
/// </para>
/// </remarks>
public class OrdemTest
{
    [Fact(DisplayName = "numerar permutacoes e uma bijecao, em todo n ate 8")]
    public void ABijecaoDasPermutacoes()
    {
        var total = 0;

        for (var n = 0; n <= 8; n++)
        {
            var quantas = (int)Contagem.Fatorial(n);

            var vistos = new bool[quantas];

            var esperado = 0;

            foreach (var permutacao in Geracao.Permutacoes(n))
            {
                var ranque = (int)Ordem.RanquearPermutacao(permutacao);

                Assert.InRange(ranque, 0, quantas - 1);

                Assert.False(vistos[ranque], $"o numero {ranque} saiu duas vezes com n = {n}");

                vistos[ranque] = true;

                // A geracao sai em ordem lexicografica, entao o numero tem de
                // sair em ordem tambem: 0, 1, 2, ...
                Assert.Equal(esperado, ranque);

                esperado++;

                // E a volta devolve a mesma permutacao.
                Assert.Equal(permutacao, Ordem.DesranquearPermutacao(n, ranque));

                total++;
            }

            Assert.Equal(quantas, esperado);
        }

        // 1 + 1 + 2 + 6 + 24 + 120 + 720 + 5.040 + 40.320.
        Assert.Equal(46_234, total);

        Assert.Throws<ArgumentNullException>(() => Ordem.RanquearPermutacao(null!));
        Assert.Throws<ArgumentException>(() => Ordem.RanquearPermutacao([0, 0]));
        Assert.Throws<ArgumentException>(() => Ordem.RanquearPermutacao([0, 2]));
        Assert.Throws<ArgumentException>(() => Ordem.RanquearPermutacao([-1, 0]));
        Assert.Throws<ArgumentOutOfRangeException>(() => Ordem.DesranquearPermutacao(3, 6));
        Assert.Throws<ArgumentOutOfRangeException>(() => Ordem.DesranquearPermutacao(3, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Ordem.DesranquearPermutacao(-1, 0));
    }

    [Fact(DisplayName = "numerar permutacoes grandes, onde listar deixou de ser possivel")]
    public void AsPermutacoesGrandes()
    {
        // O ponto da numeracao: ela funciona onde a enumeracao nao chega. Com 30
        // elementos ha 2,65 x 10^32 permutacoes, e o teste confere a ida e a volta
        // em numeros sorteados dessa faixa.
        var sorteio = new Random(29);

        const int N = 30;

        var quantas = Contagem.Fatorial(N);

        Assert.Equal(33, quantas.ToString().Length);

        for (var caso = 0; caso < 2_000; caso++)
        {
            // Um numero uniforme na faixa, montado a partir de bytes sorteados.
            var bytes = new byte[16];

            sorteio.NextBytes(bytes);

            bytes[^1] &= 0x7F;

            var ranque = new BigInteger(bytes) % quantas;

            var permutacao = Ordem.DesranquearPermutacao(N, ranque);

            Assert.Equal(N, permutacao.Distinct().Count());

            Assert.Equal(ranque, Ordem.RanquearPermutacao(permutacao));
        }

        // As duas pontas.
        Assert.Equal(Enumerable.Range(0, N), Ordem.DesranquearPermutacao(N, 0));

        Assert.Equal(
            Enumerable.Range(0, N).Reverse(),
            Ordem.DesranquearPermutacao(N, quantas - 1));
    }

    [Fact(DisplayName = "numerar combinacoes e uma bijecao, e o numero nao depende de n")]
    public void ABijecaoDasCombinacoes()
    {
        for (var n = 0; n <= 16; n++)
        {
            for (var k = 0; k <= n; k++)
            {
                var quantas = (int)Contagem.Binomial(n, k);

                var vistos = new bool[quantas];

                foreach (var combinacao in Geracao.Combinacoes(n, k))
                {
                    var ranque = (int)Ordem.RanquearCombinacao(combinacao);

                    Assert.InRange(ranque, 0, quantas - 1);

                    Assert.False(vistos[ranque], $"o numero {ranque} repetiu em C({n},{k})");

                    vistos[ranque] = true;

                    Assert.Equal(combinacao, Ordem.DesranquearCombinacao(k, ranque));
                }

                Assert.DoesNotContain(false, vistos);
            }
        }

        // A propriedade que fez a ordem colexicografica ser a escolhida: o numero
        // de uma combinacao NAO DEPENDE do tamanho do universo. {0,2,5} vale 11
        // num universo de 6 e num de 6 milhoes, e por isso crescer o universo nao
        // renumera nada.
        int[] fixa = [0, 2, 5];

        // C(0,1) + C(2,2) + C(5,3) = 0 + 1 + 10 = 11.
        Assert.Equal(11, (int)Ordem.RanquearCombinacao(fixa));

        Assert.Equal(fixa, Ordem.DesranquearCombinacao(3, 11));

        // Repare que n nao aparece em nenhuma das duas chamadas. Na ordem
        // lexicografica, que e a intuitiva, ele apareceria nas duas e o numero
        // mudaria junto com ele.
        Assert.Equal([0, 1, 2], Ordem.DesranquearCombinacao(3, 0));
        Assert.Empty(Ordem.DesranquearCombinacao(0, 0));

        Assert.Throws<ArgumentNullException>(() => Ordem.RanquearCombinacao(null!));
        Assert.Throws<ArgumentException>(() => Ordem.RanquearCombinacao([2, 1]));
        Assert.Throws<ArgumentException>(() => Ordem.RanquearCombinacao([1, 1]));
        Assert.Throws<ArgumentException>(() => Ordem.RanquearCombinacao([-1]));
        Assert.Throws<ArgumentOutOfRangeException>(() => Ordem.DesranquearCombinacao(-1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Ordem.DesranquearCombinacao(2, -1));
    }

    [Fact(DisplayName = "a ordem colexicografica e mesmo a ordem dos numeros")]
    public void AOrdemColexicografica()
    {
        // Colexicografica e a lexicografica lida de tras para frente: compara os
        // ultimos elementos primeiro. Aqui isso e conferido contra a ordenacao
        // explicita, em todas as 8.008 combinacoes de 6 entre 16.
        var combinacoes = Geracao.Combinacoes(16, 6).Select(c => c.ToArray()).ToList();

        Assert.Equal(8_008, combinacoes.Count);

        var porNumero = combinacoes
            .OrderBy(c => Ordem.RanquearCombinacao(c))
            .ToList();

        var porColex = combinacoes
            .OrderBy(c => c, Comparer<int[]>.Create((um, outro) =>
            {
                for (var i = um.Length - 1; i >= 0; i--)
                {
                    if (um[i] != outro[i])
                    {
                        return um[i].CompareTo(outro[i]);
                    }
                }

                return 0;
            }))
            .ToList();

        for (var i = 0; i < combinacoes.Count; i++)
        {
            Assert.Equal(porColex[i], porNumero[i]);

            Assert.Equal(i, (int)Ordem.RanquearCombinacao(porNumero[i]));
        }
    }

    [Fact(DisplayName = "o codigo de Gray troca exatamente um bit, em todos os 16 milhoes")]
    public void OCodigoDeGray()
    {
        // A varredura: dezesseis milhoes de valores consecutivos, conferindo as
        // duas propriedades que fazem o codigo existir.
        const uint Ate = 1 << 24;

        var anterior = Ordem.ParaGray(0);

        Assert.Equal(0u, anterior);

        for (var n = 1u; n < Ate; n++)
        {
            var cinza = Ordem.ParaGray(n);

            // Um bit, nem zero nem dois. A mensagem e montada so quando falha:
            // interpolar string dentro de um laco de dezesseis milhoes de voltas
            // custa mais que o teste inteiro.
            if (System.Numerics.BitOperations.PopCount(cinza ^ anterior) != 1)
            {
                Assert.Fail($"de {n - 1} para {n} mudaram "
                    + $"{System.Numerics.BitOperations.PopCount(cinza ^ anterior)} bits");
            }

            // E a volta.
            if (Ordem.DeGray(cinza) != n)
            {
                Assert.Fail($"a volta de {n} deu {Ordem.DeGray(cinza)}");
            }

            anterior = cinza;
        }

        // E nos extremos do tipo, onde o deslocamento de 16 casas importa.
        foreach (var n in (uint[])[0, 1, uint.MaxValue, uint.MaxValue - 1, 1u << 31])
        {
            Assert.Equal(n, Ordem.DeGray(Ordem.ParaGray(n)));
        }

        Assert.Equal(1u, Ordem.ParaGray(1));
        Assert.Equal(3u, Ordem.ParaGray(2));
        Assert.Equal(2u, Ordem.ParaGray(3));
        Assert.Equal(6u, Ordem.ParaGray(4));
    }
}
