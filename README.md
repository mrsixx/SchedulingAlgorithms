# Scheduling Algorithms

Este repositório contém a implementação de algoritmos de escalonamento desenvolvidos para o Trabalho de Conclusão de Curso em Ciência da Computação pela Universidade Federal do ABC, intitulado:

**"O Escalonador dos Papéis: Abordagens heurísticas com colônias de formigas para o escalonamento flexível de trabalhos no contexto da indústria gráfica"**

[Leia o trabalho completo aqui](TCC.pdf)

## Descrição

O projeto implementa diversas variações de algoritmos baseados em Colônia de Formigas (Ant Colony Optimization - ACO) para resolver problemas de escalonamento flexível de trabalhos (Flexible Job Shop Scheduling Problem - FJSSP), com aplicação específica no contexto da indústria gráfica.

## Estrutura do Projeto

O código foi desenvolvido em C# e está organizado em vários projetos. O projeto principal é o `Scheduling.Console`, que contém a interface de linha de comando para execução dos algoritmos.

## Como Executar

1. Compile o projeto `Scheduling.Console`
2. Execute o programa com os parâmetros necessários

### Formato do Comando

```bash
.\Scheduling.Console.exe -i {file} -o {output_dir} --runs {runs} {params}
```
Onde:
* `file`: caminho para o arquivo `.fjs` com a instância do problema
* `output_dir`: diretório de saída para os resultados (será criado se não existir)
* `runs`: número de execuções independentes
* `params`: parâmetros do algoritmo (detalhados abaixo)

### Exemplos de Execução

#### 1. Algoritmo Guloso (Greedy)
```bash
.\Scheduling.Console.exe -i instances/print_job.fjs -o results/greedy/ --runs 5 --solver greedy
```

#### 2. Ant System Versão 1 (Sequencial)
```bash
.\Scheduling.Console.exe -i instances/print_job.fjs -o results/asv1/ --runs 10 \
--solver ASV1 --iterations 50 --ants 20 --alpha 1 --beta 1.4 --rho 0.01 --tau0 100
```

#### 3. Ant System Versão 1 (Paralelo)
```bash
.\Scheduling.Console.exe -i instances/large_batch.fjs -o results/asv1_parallel/ --runs 8 \
--solver ASV1 --iterations 50 --ants 20 --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --parallel
```

#### 4. MAX-MIN Ant System (Com limites de feromônio)
```bash
.\Scheduling.Console.exe -i instances/complex_job.fjs -o results/mmas/ --runs 15 \
--solver MMASV1 --iterations 100 --ants 30 --alpha 1 --beta 2.0 --rho 0.05 --taumin 5 --taumax 500
```

#### 5. ACS com paralelismo (Versão 2)
```bash
.\Scheduling.Console.exe -i instances/urgent_order.fjs -o results/acsv2_parallel/ --runs 12 \
--solver ACSV2 --iterations 80 --ants 25 --alpha 0.8 --beta 1.8 --rho 0.1 --tau0 50 --phi 0.05 --parallel
```

#### Observações:
- Substitua `instances/*.fjs` pelos caminhos reais dos seus arquivos de instância
- O parâmetro `--runs` define quantas vezes o algoritmo será executado (útil para análise estatística)
- Para versões paralelas, sempre inclua a flag `--parallel`
- Diretórios de saída serão criados automaticamente se não existirem

### Parâmetros dos Algoritmos

#### Tabela Geral de Parâmetros
| Parâmetro        | Descrição                          | Valores Típicos      |
|------------------|------------------------------------|----------------------|
| `--solver`       | Versão do algoritmo                | `ASV1`, `MMASV2`, etc |
| `--iterations`   | Número de iterações                | 50-200               |
| `--ants`         | Número de formigas                 | 10-50                |
| `--alpha`        | Peso do feromônio                  | 0.8-2.0              |
| `--beta`         | Peso da heurística                 | 1.0-3.0              |
| `--rho`          | Taxa de evaporação                 | 0.01-0.2             |
| `--tau0`         | Feromônio inicial                  | 1-100                |
| `--parallel`     | Ativa execução paralela            | (flag sem valor)      |

#### Configurações por Algoritmo

##### Algoritmos Básicos
| Algoritmo  | Parâmetros Específicos           | Exemplo de Comando |
|------------|----------------------------------|--------------------|
| **Greedy** | - | `--solver greedy` |
| **ASV1**   | `--tau0` (feromônio inicial) | `--solver ASV1 --tau0 100` |

##### Algoritmos Avançados
| Algoritmo   | Parâmetros Exclusivos               | Exemplo Completo |
|-------------|-------------------------------------|------------------|
| **MMASV1**  | `--taumin`, `--taumax` (limites de feromônio) | `--solver MMASV1 --taumin 5 --taumax 500` |
| **EASV2**   | `--elitistweight` (peso da formiga elite) | `--solver EASV2 --elitistweight 50` |
| **ACSV1**   | `--phi` (taxa de atualização local) | `--solver ACSV1 --phi 0.05` |

#### Exemplo de Configuração Completa
```bash
# MMAS com paralelismo
.\Scheduling.Console.exe -i instances/print_job.fjs -o results/mmas_parallel/ --runs 10 \
--solver MMASV2 --iterations 100 --ants 30 --alpha 1.2 --beta 1.8 --rho 0.1 \
--taumin 10 --taumax 1000 --parallel
```

#### Legenda:
- 📌 **Obrigatório**: Todos os algoritmos requerem `--solver`, `--iterations` e `--ants`
- 🔄 **Paralelizável**: Adicione `--parallel` para versões paralelas (exceto Greedy)
- ⚠️ **Valores padrão**: Se omitidos, usam valores internos (consulte o PDF do TCC)
