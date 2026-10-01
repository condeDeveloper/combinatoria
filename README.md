# combinatoria

Contar, numerar e gerar objetos combinatórios do zero em C# e .NET 8, com a
enumeração completa como juiz enquanto ela existir.

```
$ dotnet medidor.dll numerar

    enumerando as 3,628,800 permutacoes de 10 elementos:
      71 ms, ou 51,003,757 por segundo

    sorteando uma das 265252859812191058636308480000000 permutacoes de 30
    elementos, uniformemente, pelo numero: 7.7 microssegundos cada.

    chegar na ultima enumerando levaria 1.6e+17 anos.
```

## O juiz é a enumeração, enquanto ela existir

A combinatória tem um juiz embutido que quase nenhuma outra área tem: a fórmula
diz quantos são, e a enumeração conta quantos saíram. Enquanto o número for
pequeno, os dois caminhos existem ao mesmo tempo e não se parecem em nada.

Então é isso que os testes fazem. O binomial é conferido contra listar as
combinações uma a uma, em todas as 524.287 de até 18 elementos. O Catalan é
conferido contra contar as sequências de parênteses balanceados. O Stirling e o
Bell são conferidos contra enumerar as partições de conjunto pelas cadeias de
crescimento restrito. Os desarranjos, contra gerar as 46.233 permutações de até 8
elementos e contar as que não deixam ninguém no lugar. As partições de inteiro,
contra descer somando parcelas.

Quando o número deixa de ser pequeno, entram os catálogos. `p(100)` vale
190.569.292, e esse número é o começo da história de Hardy e Ramanujan: foi
MacMahon calculando partições à mão até 200 que deu a eles a tabela de
conferência.

São 16 testes.

## Numerar é uma bijeção, e é assim que se confere

Dar um número a cada permutação é fácil de fazer quase certo. O defeito típico
não é um número absurdo, é um buraco: duas permutações com o mesmo número, ou um
número que não corresponde a nada. Quem sorteia pelo número passa a sortear
torto, e nada quebra.

A única conferência que pega isso é a varredura. Para cada `n` até 8, o teste
gera todas as `n!` permutações, numera cada uma, e exige que os números sejam
exatamente 0 até `n! - 1`, sem repetição e na mesma ordem em que elas saem. São
46.234 objetos, e a mesma coisa é feita com as combinações, para todos os
`C(n,k)` com `n` até 16.

## A ordem colexicográfica, e por que não a intuitiva

O número de uma combinação sai pelo sistema de numeração combinatório:
`C(c0,1) + C(c1,2) + ... + C(ck,k)`. Isso ordena as combinações em ordem
colexicográfica, que é a lexicográfica lida de trás para frente, e não é a ordem
que ninguém esperaria.

A escolha não é por capricho. Nessa ordem o número de uma combinação **não
depende de n**: o conjunto `{0, 2, 5}` vale 11 num universo de 6 elementos e num
de 6 milhões. Repare que `n` não aparece em nenhuma das duas funções. Com a ordem
lexicográfica, que é a intuitiva, ele apareceria nas duas e crescer o universo
renumeraria tudo.

## Onde cada tipo quebra

```
$ dotnet medidor.dll estourar

    o fatorial de que numero NAO cabe em:
      int    (31 bits)       13!
      long   (63 bits)       21!
      UInt128               35!

         n   digitos de C(n,n/2)   digitos de n!   cabe em long?
  --------------------------------------------------------------------
        60                    18              82             sim
        62                    18              86             sim
        64                    19              90             sim
       100                    30             158             nao
```

`C(62,31)` cabe num `long` com vinte vezes de folga, e `62!` tem 86 dígitos.
Quem calcula o binomial dividindo fatoriais perde essa faixa inteira sem precisar.
E o erro não aparece como número errado: aparece como exceção de estouro, ou
pior, como número negativo em aritmética que não confere estouro.

Aqui o binomial é um produto parcial com divisão a cada passo, e a divisão é
sempre exata porque o produto de `i` inteiros consecutivos é divisível por `i!`.

## Duas previsões minhas, e as duas erradas

**A primeira.** Escrevi na ferramenta que o número de Bell ultrapassa o fatorial
a partir de certo ponto. A tabela diz o contrário, com folga:

```
$ dotnet medidor.dll crescer

         n        n!    C(2n,n)    Bell(n)       p(n)   desarranjos
  ----------------------------------------------------------------------
       100       158         59        116          9            158
       250       493        150        364         15            493
       500      1135        300        844         22           1134
```

A conta explica: o logaritmo do fatorial dividido por `n` vale `ln n - 1`, e o do
Bell vale `ln n - ln ln n - 1`. O segundo é menor para todo `n` a partir de 16, e
a distância só aumenta.

**A segunda.** O código de Gray permite percorrer todos os subconjuntos trocando
um elemento por vez, então manter uma soma acumulada custa uma adição por passo
em vez de somar os 22 elementos. Previ que o ganho seria 22 vezes.

```
$ dotnet medidor.dll gray

    recalculando cada um        220.6 ms
    em ordem de Gray             32.5 ms        6.8x
    idem, sem o enumerador        4.2 ms       52.6x
```

Errei para os dois lados. Com o enumerador preguiçoso dá 6,8 vezes, porque quase
todo o tempo restante é chamada de interface, uma por subconjunto, e ela custa
mais que a conta economizada. Sem ele dá 52 vezes, que é **mais** que os 22
elementos: cada elemento do laço de dentro não custa uma soma, custa um
deslocamento, uma máscara e um desvio que o processador não consegue prever,
porque o bit é essencialmente aleatório.

## O algoritmo de Heap, e a invariante que ele promete

A geração lexicográfica é o `next_permutation` do C++, e ele funciona com valores
repetidos: com `[0,0,1,1,2]` ele visita os 30 arranjos distintos e não os 120 que
existiriam se os iguais fossem distinguíveis. O teste confere isso contra o
coeficiente multinomial.

O algoritmo de Heap gera as mesmas permutações noutra ordem, e o que ele promete é
outra coisa: duas permutações consecutivas diferem por **uma transposição**. É o
equivalente do código de Gray para permutações, e serve quando avaliar uma
permutação custa caro e dá para atualizar a avaliação a partir de uma troca.

Essa promessa é uma invariante, então o teste a confere a cada passo: entre duas
saídas consecutivas, exatamente duas posições mudam. Mais a varredura, que exige
que as `n!` saiam e nenhuma repita.

## Rodar

.NET 8. Zero dependências fora do xUnit, e só nos testes.

```
dotnet test testes/Combinatoria.Testes/Combinatoria.Testes.csproj -c Release
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- numerar
```

## O que ele não faz

Não tem partições de inteiro com restrição, nem as de conjunto geradas uma a uma.
Conta quantas são; listar é outro problema.

Não tem numeração para partições, nem para árvores, nem para as outras famílias
que o Catalan conta. O sistema fatorial e o combinatório cobrem permutações e
combinações, e cada família nova pede o seu.

Não tem permutações com repetição numeradas, embora a geração lide com elas.

Não gera objetos aleatórios por rejeição nem por amostragem de Boltzmann. Sortear
pelo número é uniforme e é suficiente enquanto a contagem for calculável.

Não usa a fórmula de Hardy, Ramanujan e Rademacher para `p(n)`, que é exata e dá
a resposta direto, porque ela precisa de ponto flutuante de precisão arbitrária e
isso é um projeto inteiro à parte.

## Licença

MIT.
