import sys
import os
import subprocess
from timeit import default_timer as timer
from datetime import timedelta, datetime


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


def commit(repo_dir, message):
	bash_file = "/home/matheus.antonio/Documents/PGC/SchedulingAlgorithms/scripts/auto_push.sh"
	subprocess.run([bash_file, repo_dir, message])

def log(msg):
	print(f'[{datetime.now().strftime('%m/%d/%Y %I:%M:%S %p')}] {msg}')

if __name__ == "__main__":
	runs = 1
	iterations = 1
	output_dir = f'//home//matheus.antonio//Documents//PGC//{sys.argv[1]}'
	benchmarks_dir = '//home//matheus.antonio//Documents//PGC//SchedulingAlgorithms//Scheduling.Benchmarks//Data//test//'
	scheduler_dir = '//home//matheus.antonio//Documents//PGC//scheduler//Scheduling.Console'
	benchmarks = listar_benchmarks(benchmarks_dir, '.fjs')
	fails = []

	if not os.path.exists(output_dir):
		os.makedirs(output_dir)

	approachs = {
		'greedy': '--solver greedy',
		#'asv1-i': f'--solver ASV1 --iterations {iterations} --beta 1.4',
		#'asv1-p': f'--solver ASV1 --iterations {iterations} --beta 1.4 --parallel',
		'asv2-i': f'--solver ASV2 --iterations {iterations} --beta 1.4',
		'asv2-p': f'--solver ASV2 --iterations {iterations} --beta 1.4 --parallel',
		#'rbasv1-i': f'--solver RBASV1 --iterations {iterations} --beta 1.4',
		#'rbasv1-p': f'--solver RBASV1 --iterations {iterations} --beta 1.4 --parallel',
		'rbasv2-i': f'--solver RBASV2 --iterations {iterations} --beta 1.4 ',
		'rbasv2-p': f'--solver RBASV2 --iterations {iterations} --beta 1.4  --parallel',
		#'easv1-i': f'--solver EASV1 --iterations {iterations} --beta 1.4',
		#'easv1-p': f'--solver EASV1 --iterations {iterations} --beta 1.4 --parallel',
		'easv2-i': f'--solver EASV2 --iterations {iterations} --beta 1.4',
		'easv2-p': f'--solver EASV2 --iterations {iterations} --beta 1.4 --parallel',
		#'mmasv1-i': f'--solver MMASV1 --iterations {iterations} --beta 1.4',
		#'mmasv1-p': f'--solver MMASV1 --iterations {iterations} --beta 1.4 --parallel',
		'mmasv2-i': f'--solver MMASV2 --iterations {iterations} --beta 1.4',
		'mmasv2-p': f'--solver MMASV2 --iterations {iterations} --beta 1.4 --parallel',
		#'acsv0-i': f'--solver ACSV0 --iterations {iterations} --beta 1.4',
		#'acsv0-p': f'--solver ACSV0 --iterations {iterations} --beta 1.4 --parallel',
		#'acsv1-i': f'--solver ACSV1 --iterations {iterations} --beta 1.4',
		#'acsv1-p': f'--solver ACSV1 --iterations {iterations} --beta 1.4 --parallel',
		'acsv2-i': f'--solver ACSV2 --iterations {iterations} --beta 1.4',
		'acsv2-p': f'--solver ACSV2 --iterations {iterations} --beta 1.4 --parallel',
	}

	i = 0
	benchmarks_group = benchmarks.items()
	for name, files in benchmarks_group:
		i += 1
		log(f'\n\n\nExecutando instâncias de {name} ({i}/{len(benchmarks_group)})...\n')
		start = timer()
		j = 0
		for file in files:
			j += 1
			log(f'Instância {j}/{len(files)}...\n')
			
			solver_count = len(approachs.items())
			solver_queue = list(approachs.items())
			while len(solver_queue) > 0:
				solver, params = solver_queue.pop(0)
				log(f'Iniciando a execução do solver #{solver}')
				log(f'Solvers restantes: {list(map(lambda s: s[0],solver_queue))}')
				cmd = f'{scheduler_dir} -i {file} -o {output_dir} -v --runs {runs} {params}'
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

				log(f'Finalizando com {solver}')
				commit(output_dir, f'Resultados {solver} para instância {j}/{len(files)} de {name} {[{datetime.now().strftime('%m/%d/%Y %I:%M:%S %p')}]}')
			end = timer()
			log(f'Finalizando instâncias de {name} em {timedelta(seconds=end-start)}...\n')

	log(f'Finalizando execução com {len(fails)} falhas...\n')
