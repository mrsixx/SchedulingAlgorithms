const fs = require('fs');
var tsort = require('tsort');
var groupBy = require('lodash.groupby');

const FILENAME = 'RibeiroSuzarte4.json';

function readFile(err, json) {
    if(err) {
        console.error(err);
        return;
    }

    const { processos, maqXproc: processoMaquinas, menorInicio, maiorFim } = JSON.parse(json);

    console.log('Dumb makespan', new Date(maiorFim).getTime() - new Date(menorInicio).getTime());
    const processosPorOs = groupBy(processos, p => p.OS);
    const processosPorMaquina = groupBy(processoMaquinas, p => p.MaquinaId);
    const maquinasPorProcesso = groupBy(processoMaquinas, p => p.ProcessoId);

    const jobs = new Map(), machines = new Map();
    
    Object.keys(processosPorOs).forEach((key, idx) => jobs.set(key, idx+1));
    Object.keys(processosPorMaquina).forEach((key, idx) => machines.set(key, idx+1));

    const instanceCube = Object.keys(processosPorOs).map(osId => {
        const operations = processosPorOs[osId];

        var graph = tsort();
        operations.forEach(({Id: oId, Dependencias: precedence, OS: jobId }) => {
            graph.add(oId);
            precedence.forEach(tailId => {
                if(operations.some(o => o.Id === tailId))
                    graph.add(tailId, oId);
            })
        })
        const sort = graph.sort();
        const operationsData = sort.map(o => {
            const maquinasProcesso = maquinasPorProcesso[o];
            return maquinasProcesso?.map(m => [machines.get(m.MaquinaId.toString()),m.TempoProducao]) || [];
        });
        return operationsData;
    });
    const content = makeFileContent({
        nJobs: jobs.size,
        nMachines: machines.size,
        jobData: instanceCube,
        inicio: menorInicio, 
        fim: maiorFim
    });

    fs.writeFile(`${FILENAME.replaceAll('.json', '')}.fjs`, content, (err) => {
        if (err)
          console.error(err);
        else
          console.log(`content successfully written into ${FILENAME}.jsp`);
    });      
}


function makeFileContent({ nJobs, nMachines, jobData, inicio, fim }) {
    const rows = [];
    let pairsCount = 0;
    let operationsCount = 0;
    jobData.forEach(job => {
        const numOperations = job.length;
        operationsCount += numOperations;
        const operationData = job.map(op => {
            const numMachines = op.length;
            // processing time in seconds
            const machinesData = op.map(em => `${em[0]} ${Math.floor(em[1]* 3600)}`).join(' ');
            pairsCount += numMachines;
            return `${numMachines} ${machinesData}`;
            
        }).join(' ');
        rows.push(`${numOperations} ${operationData}`);
    })
    const makespan = new Date(fim).getTime() - new Date(inicio).getTime();
    return [`${nJobs} ${nMachines} ${Math.round(pairsCount/operationsCount)} ${makespan}`, ...rows].join('\n');
}

fs.readFile(FILENAME, 'utf8', readFile);