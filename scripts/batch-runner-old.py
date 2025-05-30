import sys
import os
import subprocess
import time
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
	bash_file = "/home/matheus.antonio/Documentos/PGC/SchedulingAlgorithms/scripts/auto_push.sh"
	subprocess.run([bash_file, repo_dir, message], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

def log(msg):
	print(f'[{datetime.now().strftime('%m/%d/%Y %I:%M:%S %p')}] ORCHESTRATOR: {msg}')

if __name__ == "__main__":
	runs = 30
	iterations = 50
	lazygens = 10
	output_dir = f'/home/matheus.antonio/Documentos/PGC/{sys.argv[1]}'
	benchmarks_dir = f'/home/matheus.antonio/Documentos/PGC/SchedulingAlgorithms/Scheduling.Benchmarks/Data/{sys.argv[2]}/'
	versions = sys.argv[3].upper().split(',') if len(sys.argv) > 3 else []
	scheduler_dir = '/home/matheus.antonio/Documentos/PGC/scheduler/Scheduling.Console'
	benchmarks = listar_benchmarks(benchmarks_dir, '.fjs')
	fails = []

	if not os.path.exists(output_dir):
		os.makedirs(output_dir)
	# region versions
	v0 = {
		'acsv0-i': f'--solver ACSV0 --iterations {iterations} --lazygens {lazygens} --beta 1.4',
		'acsv0-p': f'--solver ACSV0 --iterations {iterations} --lazygens {lazygens} --beta 1.4 --parallel',
	}

	v1 = {
		'asv1-i': f'--solver ASV1 --iterations {iterations} --lazygens {lazygens} --beta 1.4',
		'asv1-p': f'--solver ASV1 --iterations {iterations} --lazygens {lazygens} --beta 1.4 --parallel',
		'easv1-i': f'--solver EASV1 --iterations {iterations} --lazygens {lazygens} --beta 1.4',
		'easv1-p': f'--solver EASV1 --iterations {iterations} --lazygens {lazygens} --beta 1.4 --parallel',
		'rbasv1-i': f'--solver RBASV1 --iterations {iterations} --lazygens {lazygens} --beta 1.4',
		'rbasv1-p': f'--solver RBASV1 --iterations {iterations} --lazygens {lazygens} --beta 1.4 --parallel',
		'mmasv1-i': f'--solver MMASV1 --iterations {iterations} --lazygens {lazygens} --beta 1.4',
		'mmasv1-p': f'--solver MMASV1 --iterations {iterations} --lazygens {lazygens} --beta 1.4 --parallel',
		'acsv1-i': f'--solver ACSV1 --iterations {iterations} --lazygens {lazygens} --beta 1.4',
		'acsv1-p': f'--solver ACSV1 --iterations {iterations} --lazygens {lazygens} --beta 1.4 --parallel',
	}

	v2 = {
		'asv2-i': f'--solver ASV2 --iterations {iterations} --lazygens {lazygens} --beta 1.4',
		'asv2-p': f'--solver ASV2 --iterations {iterations} --lazygens {lazygens} --beta 1.4 --parallel',
		'easv2-i': f'--solver EASV2 --iterations {iterations} --lazygens {lazygens} --beta 1.4',
		'easv2-p': f'--solver EASV2 --iterations {iterations} --lazygens {lazygens} --beta 1.4 --parallel',
		'rbasv2-i': f'--solver RBASV2 --iterations {iterations} --lazygens {lazygens} --beta 1.4 ',
		'rbasv2-p': f'--solver RBASV2 --iterations {iterations} --lazygens {lazygens} --beta 1.4  --parallel',
		'mmasv2-i': f'--solver MMASV2 --iterations {iterations} --lazygens {lazygens} --beta 1.4',
		'mmasv2-p': f'--solver MMASV2 --iterations {iterations} --lazygens {lazygens} --beta 1.4 --parallel',
		'acsv2-i': f'--solver ACSV2 --iterations {iterations} --lazygens {lazygens} --beta 1.4',
		'acsv2-p': f'--solver ACSV2 --iterations {iterations} --lazygens {lazygens} --beta 1.4 --parallel',
	}
	
	v3 = {
		'asv3-i': f'--solver ASV3 --iterations {iterations} --lazygens {lazygens} --beta 1.4',
		'asv3-p': f'--solver ASV3 --iterations {iterations} --lazygens {lazygens} --beta 1.4 --parallel',
		'easv3-i': f'--solver EASV3 --iterations {iterations} --lazygens {lazygens} --beta 1.4',
		'easv3-p': f'--solver EASV3 --iterations {iterations} --lazygens {lazygens} --beta 1.4 --parallel',
		'rbasv3-i': f'--solver RBASV3 --iterations {iterations} --lazygens {lazygens} --beta 1.4 ',
		'rbasv3-p': f'--solver RBASV3 --iterations {iterations} --lazygens {lazygens} --beta 1.4  --parallel',
		'mmasv3-i': f'--solver MMASV3 --iterations {iterations} --lazygens {lazygens} --beta 1.4',
		'mmasv3-p': f'--solver MMASV3 --iterations {iterations} --lazygens {lazygens} --beta 1.4 --parallel',
		'acsv3-i': f'--solver ACSV3 --iterations {iterations} --lazygens {lazygens} --beta 1.4',
		'acsv3-p': f'--solver ACSV3 --iterations {iterations} --lazygens {lazygens} --beta 1.4 --parallel',
	}

	approachs = {
		'greedy': '--solver greedy',
	}

	if 'V0' in versions:
		approachs = {**approachs, **v0}
	if 'V1' in versions:
		approachs = {**approachs, **v1}
	if 'V2' in versions:
		approachs = {**approachs, **v2}
	if 'V3' in versions:
		approachs = {**approachs, **v3}

	# endregion
	
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
					proc = subprocess.Popen(
						cmd, 
						shell=True,
						cwd=output_dir,
						stdout=subprocess.PIPE,
						stderr=subprocess.STDOUT,
						text=True,
						bufsize=1
					)
					for line in proc.stdout:
						print(line, end='', flush=True)
					proc.wait()
					if proc.returncode != 0:
						fails.append([solver, f'return code {resultado.returncode}', file])
				except subprocess.SubprocessError as e:
					fails.append([solver, e, file])

				log(f'Finalizando com {solver}')
				commit(output_dir, f'Resultados {solver} para instância {os.path.basename(file)} ({j}/{len(files)}) de {name} {[{datetime.now().strftime('%m/%d/%Y %I:%M:%S %p')}]}')
				time.sleep(5)
			end = timer()
			log(f'Finalizando instâncias de {name} em {timedelta(seconds=end-start)}...\n')

	log(f'Finalizando execução com {len(fails)} falhas...\n')
