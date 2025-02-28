import os
import subprocess


def listar_benchmarks(dir, extensao):
    benchmarks = {}
    for root, dirs, files in os.walk(dir):
        for arquivo in files:
            if arquivo.endswith(extensao):
                filepath = os.path.join(root, arquivo)
                parent_path = os.path.dirname(filepath)
                benchmark_name = os.path.basename(parent_path)
                if benchmark_name not in benchmarks:
                    benchmarks[benchmark_name] = []
                
                benchmarks[benchmark_name].append(filepath)
    return benchmarks



if __name__ == "__main__":
    runs = 10
    output_dir = '//workspaces//SchedulingAlgorithms//scripts//output'
    benchmarks_dir = '//workspaces//SchedulingAlgorithms//Scheduling.Benchmarks//Data//'
    scheduler_dir = '//workspaces//SchedulingAlgorithms//Scheduling.Console//bin//Release//net8.0//linux-x64//publish//Scheduling.Console'
    benchmarks = listar_benchmarks(benchmarks_dir, '.fjs')
    fails = []

    if not os.path.exists(output_dir):
        os.makedirs(output_dir)

    approachs = {
        'greedy': '--solver greedy',
        'as-i': '--solver as --iterations 50 --ants 20  --alpha 1 --beta 1.4 --tau0 100',
        'as-p': '--solver as --iterations 50 --ants 20  --alpha 1 --beta 1.4 --tau0 100 --parallel',
        'eas-i': '--solver eas --iterations 50 --ants 20  --alpha 1 --beta 1.4 --tau0 100 --ew 100',
        'eas-p': '--solver eas --iterations 50 --ants 20  --alpha 1 --beta 1.4 --tau0 100 --ew 100 --parallel',
        'mmas-i': '--solver mmas --iterations 50 --ants 20  --alpha 1 --beta 1.4 --taumin 10 --taumax 1000',
        'mmas-p': '--solver mmas --iterations 50 --ants 20  --alpha 1 --beta 1.4 --taumin 10 --taumax 1000 --parallel',
    }


    for name, files in benchmarks.items():
        print(f'Executando instâncias de {name}...\n')
        i = 0
        for file in files:
            i += 1
            if i > 1:
                break

            print(f'Instância {i}/{len(files)}...\n')
            
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
        
        print(f'Finalizando instâncias de {name}...\n')

    print(f'Finalizando execução com {len(fails)} falhas...\n')
