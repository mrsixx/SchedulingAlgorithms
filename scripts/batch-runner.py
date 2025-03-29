import os
import subprocess
from timeit import default_timer as timer
from datetime import timedelta


def listar_benchmarks(dir, extensao):
    benchmarks = { '/': [] }
    for root, dirs, files in os.walk(dir):
        for arquivo in files:
            if arquivo.endswith(extensao):
                filepath = os.path.join(root, arquivo)
                parent_path = os.path.dirname(filepath)
                benchmark_name = os.path.basename(parent_path)
                if(benchmark_name == dir):
                    benchmark_name = '/'
                if benchmark_name not in benchmarks:
                    benchmarks[benchmark_name] = []
                
                benchmarks[benchmark_name].append(filepath)
    return benchmarks



if __name__ == "__main__":
    runs = 10
    output_dir = '//workspaces//SchedulingAlgorithms//scripts//output'
    benchmarks_dir = '//workspaces//SchedulingAlgorithms//Scheduling.Benchmarks//Data//teste'
    scheduler_dir = '//workspaces//SchedulingAlgorithms//Scheduling.Console//bin//Release//net8.0//linux-x64//publish//Scheduling.Console'
    benchmarks = listar_benchmarks(benchmarks_dir, '.fjs')
    fails = []

    if not os.path.exists(output_dir):
        os.makedirs(output_dir)

    approachs = {
        #'greedy': '--solver greedy',
        'asv2-i': '--solver ASV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100',
        'asv2-p': '--solver ASV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --parallel',
        'rbasv2-i': '--solver RBASV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --ranksize 5',
        'rbasv2-p': '--solver RBASV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --ranksize 5 --parallel',
        'easv2-i': '--solver EASV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --elitistweight 100',
        'easv2-p': '--solver EASV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --elitistweight 100 --parallel',
        'mmasv2-i': '--solver MMASV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --taumin 10 --taumax 1000',
        'mmasv2-p': '--solver MMASV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --taumin 10 --taumax 1000 --parallel',
        'acsv2-i': '--solver ACSV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --phi 0.04',
        'acsv2-p': '--solver ACSV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --phi 0.04 --parallel',
    }

    # approachs = {
    #     'greedy': '--solver greedy',
    #     'asv1-i': '--solver ASV1 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100',
    #     'asv1-p': '--solver ASV1 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --parallel',
    #     'asv2-i': '--solver ASV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100',
    #     'asv2-p': '--solver ASV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --parallel',
    #     'rbasv1-i': '--solver RBASV1 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --ranksize 5',
    #     'rbasv1-p': '--solver RBASV1 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --ranksize 5 --parallel',
    #     'rbasv2-i': '--solver RBASV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --ranksize 5',
    #     'rbasv2-p': '--solver RBASV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --ranksize 5 --parallel',
    #     'easv1-i': '--solver EASV1 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --elitistweight 100',
    #     'easv1-p': '--solver EASV1 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --elitistweight 100 --parallel',
    #     'easv2-i': '--solver EASV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --elitistweight 100',
    #     'easv2-p': '--solver EASV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --elitistweight 100 --parallel',
    #     'mmasv1-i': '--solver MMASV1 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --taumin 10 --taumax 1000',
    #     'mmasv1-p': '--solver MMASV1 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --taumin 10 --taumax 1000 --parallel',
    #     'mmasv2-i': '--solver MMASV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --taumin 10 --taumax 1000',
    #     'mmasv2-p': '--solver MMASV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --taumin 10 --taumax 1000 --parallel',
    #     'acsv0-i': '--solver ACSV0 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --phi 0.04',
    #     'acsv0-p': '--solver ACSV0 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --phi 0.04 --parallel',
    #     'acsv1-i': '--solver ACSV1 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --phi 0.04',
    #     'acsv1-p': '--solver ACSV1 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --phi 0.04 --parallel',
    #     'acsv2-i': '--solver ACSV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --phi 0.04',
    #     'acsv2-p': '--solver ACSV2 --iterations 50 --ants 20  --alpha 1 --beta 1.4 --rho 0.01 --tau0 100 --phi 0.04 --parallel',
    # }

    i = 0
    benchmarks_group = benchmarks.items()
    for name, files in benchmarks_group:
        i += 1
        print(f'\n\n\nExecutando instâncias de {name} ({i}/{len(benchmarks_group)})...\n')
        start = timer()
        j = 0
        for file in files:
            j += 1
            print(f'Instância {j}/{len(files)}...\n')
            
            for solver, params in approachs.items():
                cmd = f'{scheduler_dir} -i {file} -o {output_dir} --runs {runs} {params}'
                try:
                    resultado = subprocess.run(
                        [cmd], shell=True,
                        cwd=output_dir,
                        check=True,
                        # stdout=subprocess.PIPE,
                        # stderr=subprocess.PIPE,
                        text=True
                    )
                    if resultado.returncode != 0:
                        fails.append([solver, f'return code {resultado.returncode}', file])
                except subprocess.CalledProcessError as e:
                    fails.append([solver, e, file])

        end = timer()
        print(f'Finalizando instâncias de {name} em {timedelta(seconds=end-start)}...\n')

    print(f'Finalizando execução com {len(fails)} falhas...\n')
