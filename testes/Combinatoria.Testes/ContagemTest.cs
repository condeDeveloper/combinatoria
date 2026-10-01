using System.Numerics;

using Xunit;

namespace Conde.Combinatoria.Testes;

/// <summary>
/// Contar sem enumerar, conferido contra enumerar.
/// </summary>
/// <remarks>
/// <para>
/// A combinatória tem o juiz perfeito embutido: a fórmula diz quantos são, e a
/// enumeração conta quantos saíram. Enquanto o número for pequeno, os dois
/// caminhos existem, e eles não se parecem em nada.
/// </para>
/// <para>
/// Quando o número deixa de ser pequeno, entram os catálogos: p(100), os Bell,
/// os Catalan e os Stirling estão tabelados há mais de um século, e alguns deles
/// foram calculados à mão antes de existir computador.
/// </para>
/// </remarks>
public class ContagemTest
{
    [Fact(DisplayName = "o binomial contra contar as combinacoes uma a uma")]
    public void BinomialContraAEnumeracao()
    {
        // A definicao, aplicada ate onde ela cabe nesta vida: C(18,9) sao 48.620
        // combinacoes, e a soma de todas as linhas ate 18 sao 524.287.
        var geradas = 0L;

        for (var n = 0; n <= 18; n++)
        {
            for (var k = 0; k <= n; k++)
            {
                var quantas = 0;

                foreach (var combinacao in Geracao.Combinacoes(n, k))
                {
                    Assert.Equal(k, combinacao.Length);

                    quantas++;
                }

                Assert.True(Contagem.Binomial(n, k) == quantas,
                    $"C({n},{k}): a formula deu {Contagem.Binomial(n, k)} e sairam {quantas}");

                geradas += quantas;
            }
        }

        // A soma de cada linha e 2^n, entao a soma das linhas de 0 a 18 e
        // 2^19 - 1.
        Assert.Equal((1L << 19) - 1, geradas);

        // Fora do intervalo, zero.
        Assert.Equal(BigInteger.Zero, Contagem.Binomial(5, 6));
        Assert.Equal(BigInteger.Zero, Contagem.Binomial(5, -1));
        Assert.Equal(BigInteger.One, Contagem.Binomial(0, 0));

        Assert.Throws<ArgumentOutOfRangeException>(() => Contagem.Binomial(-1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Contagem.Fatorial(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Contagem.Catalan(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Contagem.Bell(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Contagem.Desarranjos(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Contagem.Particoes(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Contagem.ParticoesPorEuler(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Contagem.Stirling2(-1, 0));
    }

    [Fact(DisplayName = "a identidade de Pascal, em todo par ate 150")]
    public void APascal()
    {
        // C(n,k) = C(n-1,k-1) + C(n-1,k). Um juiz do tipo deducao: nao compara com
        // implementacao nenhuma, so exige que a aritmetica feche. Sao 11.475
        // pares, e cada um amarra tres valores.
        for (var n = 1; n <= 150; n++)
        {
            for (var k = 0; k <= n; k++)
            {
                Assert.Equal(
                    Contagem.Binomial(n - 1, k - 1) + Contagem.Binomial(n - 1, k),
                    Contagem.Binomial(n, k));
            }

            // E a soma da linha e 2^n.
            BigInteger soma = 0;

            for (var k = 0; k <= n; k++)
            {
                soma += Contagem.Binomial(n, k);
            }

            Assert.Equal(BigInteger.One << n, soma);
        }

        // C(100,50) tem trinta digitos e e o exemplo que mostra por que o
        // caminho dos fatoriais nao serve: ele passaria por 100!, que tem 158.
        Assert.Equal(
            BigInteger.Parse("100891344545564193334812497256"),
            Contagem.Binomial(100, 50));

        Assert.Equal(158, Contagem.Fatorial(100).ToString().Length);
    }

    [Fact(DisplayName = "Catalan pela formula contra Catalan pela recorrencia")]
    public void OsCatalan()
    {
        // Dois caminhos: a formula fechada C(2n,n)/(n+1) e a recorrencia de
        // convolucao, que e como o numero aparece de verdade nos problemas.
        var pelaRecorrencia = new BigInteger[201];

        pelaRecorrencia[0] = 1;

        for (var n = 1; n <= 200; n++)
        {
            for (var i = 0; i < n; i++)
            {
                pelaRecorrencia[n] += pelaRecorrencia[i] * pelaRecorrencia[n - 1 - i];
            }

            Assert.Equal(pelaRecorrencia[n], Contagem.Catalan(n));
        }

        // Catalogo.
        Assert.Equal(
            new BigInteger[] { 1, 1, 2, 5, 14, 42, 132, 429, 1_430, 4_862, 16_796 },
            Enumerable.Range(0, 11).Select(Contagem.Catalan).ToArray());

        // E a conferencia que importa: Catalan conta os parenteses balanceados.
        for (var n = 0; n <= 10; n++)
        {
            var balanceados = 0;

            // Cada sequencia de 2n bits: 1 abre, 0 fecha.
            for (var mascara = 0; mascara < 1 << (2 * n); mascara++)
            {
                var altura = 0;

                var valida = true;

                for (var i = 0; i < 2 * n; i++)
                {
                    altura += (mascara >> i & 1) == 1 ? 1 : -1;

                    if (altura < 0)
                    {
                        valida = false;

                        break;
                    }
                }

                if (valida && altura == 0)
                {
                    balanceados++;
                }
            }

            Assert.True(Contagem.Catalan(n) == balanceados,
                $"Catalan({n}) deu {Contagem.Catalan(n)} e ha {balanceados} sequencias balanceadas");
        }
    }

    [Fact(DisplayName = "Stirling e Bell contra a enumeracao das particoes de conjunto")]
    public void StirlingEBell()
    {
        // O juiz: as cadeias de crescimento restrito. Toda particao de um conjunto
        // de n elementos corresponde a exatamente uma cadeia em que o primeiro
        // termo e 0 e cada termo e no maximo um a mais que o maior anterior.
        // Enumerar as cadeias e enumerar as particoes, sem repetir nenhuma.
        for (var n = 0; n <= 8; n++)
        {
            var porQuantidade = new int[n + 1];

            var total = 0;

            void Descer(int posicao, int maior, int[] cadeia)
            {
                if (posicao == n)
                {
                    porQuantidade[maior + 1]++;

                    total++;

                    return;
                }

                for (var valor = 0; valor <= maior + 1; valor++)
                {
                    cadeia[posicao] = valor;

                    Descer(posicao + 1, Math.Max(maior, valor), cadeia);
                }
            }

            if (n == 0)
            {
                // O conjunto vazio tem exatamente uma particao, a vazia, e ela
                // tem zero blocos. Bell(0) = 1 e S(0,0) = 1.
                total = 1;

                porQuantidade[0] = 1;
            }
            else
            {
                Descer(0, -1, new int[n]);
            }

            Assert.True(Contagem.Bell(n) == total,
                $"Bell({n}) deu {Contagem.Bell(n)} e ha {total} particoes");

            for (var k = 0; k <= n; k++)
            {
                Assert.True(Contagem.Stirling2(n, k) == porQuantidade[k],
                    $"S({n},{k}) deu {Contagem.Stirling2(n, k)} e ha {porQuantidade[k]}");
            }
        }

        // E a identidade: Bell e a soma da linha de Stirling. Conferida bem alem
        // de onde a enumeracao chega.
        for (var n = 0; n <= 60; n++)
        {
            BigInteger soma = 0;

            for (var k = 0; k <= n; k++)
            {
                soma += Contagem.Stirling2(n, k);
            }

            Assert.Equal(Contagem.Bell(n), soma);
        }

        // Catalogo.
        Assert.Equal(
            new BigInteger[] { 1, 1, 2, 5, 15, 52, 203, 877, 4_140, 21_147, 115_975 },
            Enumerable.Range(0, 11).Select(Contagem.Bell).ToArray());

        Assert.Equal(42_525, (int)Contagem.Stirling2(10, 5));
        Assert.Equal(BigInteger.Zero, Contagem.Stirling2(5, 6));
    }

    [Fact(DisplayName = "os desarranjos contra contar as permutacoes sem ponto fixo")]
    public void OsDesarranjos()
    {
        // O juiz exaustivo: para cada n ate 8, gera as n! permutacoes e conta as
        // que nao deixam ninguem no lugar.
        for (var n = 0; n <= 8; n++)
        {
            var semPontoFixo = 0;

            foreach (var permutacao in Geracao.Permutacoes(n))
            {
                var fixo = false;

                for (var i = 0; i < n; i++)
                {
                    if (permutacao[i] == i)
                    {
                        fixo = true;

                        break;
                    }
                }

                if (!fixo)
                {
                    semPontoFixo++;
                }
            }

            Assert.True(Contagem.Desarranjos(n) == semPontoFixo,
                $"D({n}) deu {Contagem.Desarranjos(n)} e ha {semPontoFixo}");
        }

        // Catalogo: 1, 0, 1, 2, 9, 44, 265, 1854, 14833, 133496, 1334961.
        Assert.Equal(
            new BigInteger[] { 1, 0, 1, 2, 9, 44, 265, 1_854, 14_833, 133_496, 1_334_961 },
            Enumerable.Range(0, 11).Select(Contagem.Desarranjos).ToArray());

        // A proporcao tende a 1/e, e tende rapido. Em n = 10 a diferenca ja esta
        // na setima casa: e por isso que a chance de um amigo oculto de dez
        // pessoas sair sem ninguem se tirando e praticamente a mesma de um de mil.
        var proporcao = (double)Contagem.Desarranjos(10) / (double)Contagem.Fatorial(10);

        Assert.True(Math.Abs(proporcao - 1 / Math.E) < 1e-7,
            $"a proporcao deu {proporcao} e 1/e e {1 / Math.E}");
    }

    [Fact(DisplayName = "as particoes por dois caminhos, e p(100) contra o catalogo")]
    public void AsParticoes()
    {
        // O caminho direto custa O(n^2) e o de Euler custa O(n^1.5), e os dois nao
        // se parecem: um soma sobre as parcelas possiveis, o outro alterna sinais
        // sobre os numeros pentagonais.
        for (var n = 0; n <= 200; n++)
        {
            Assert.True(Contagem.Particoes(n) == Contagem.ParticoesPorEuler(n),
                $"discordaram em p({n})");
        }

        // Catalogo: 1, 1, 2, 3, 5, 7, 11, 15, 22, 30, 42.
        Assert.Equal(
            new BigInteger[] { 1, 1, 2, 3, 5, 7, 11, 15, 22, 30, 42 },
            Enumerable.Range(0, 11).Select(Contagem.Particoes).ToArray());

        // p(100) e o numero que comeca a historia de Hardy e Ramanujan: foi
        // MacMahon calculando a mao ate 200 que deu a eles a tabela de conferencia.
        Assert.Equal(190_569_292, (int)Contagem.Particoes(100));

        Assert.Equal(
            BigInteger.Parse("3972999029388"),
            Contagem.Particoes(200));

        // E a enumeracao, enquanto ela cabe: as particoes de n ate 25, contadas
        // uma a uma pela definicao.
        for (var n = 0; n <= 25; n++)
        {
            var quantas = 0;

            void Descer(int sobra, int maiorParcela)
            {
                if (sobra == 0)
                {
                    quantas++;

                    return;
                }

                for (var parcela = Math.Min(sobra, maiorParcela); parcela >= 1; parcela--)
                {
                    Descer(sobra - parcela, parcela);
                }
            }

            Descer(n, n == 0 ? 0 : n);

            Assert.True(Contagem.Particoes(n) == quantas,
                $"p({n}) deu {Contagem.Particoes(n)} e sairam {quantas}");
        }
    }
}
