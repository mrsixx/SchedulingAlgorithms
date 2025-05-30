#!/bin/bash

OUTPUT_BASE="$1"

#python 1: /home/matheus.antonio/Documentos/PGC/output_dir
#python 2: /home/matheus.antonio/Documentos/PGC/SchedulingAlgorithms/Scheduling.Benchmarks/Data/benchmark_dir'

#1 tests
# BENCHMARK="1_Brandimarte" && mkdir -p "$OUTPUT_BASE/$BENCHMARK" && python3 SchedulingAlgorithms/scripts/batch-runner.py "$OUTPUT_BASE/$BENCHMARK" "$BENCHMARK" "v0,v1,v2,v3" | tee "$OUTPUT_BASE/$BENCHMARK/.log"
# BENCHMARK="9_Ribeiro" && mkdir -p "$OUTPUT_BASE/$BENCHMARK" && python3 SchedulingAlgorithms/scripts/batch-runner.py "$OUTPUT_BASE/$BENCHMARK" "$BENCHMARK" "v2,v3" | tee "$OUTPUT_BASE/$BENCHMARK/.log"
# BENCHMARK="3_DPpaulli" && mkdir -p "$OUTPUT_BASE/$BENCHMARK" && python3 SchedulingAlgorithms/scripts/batch-runner.py "$OUTPUT_BASE/$BENCHMARK" "$BENCHMARK" "v2,v3" | tee "$OUTPUT_BASE/$BENCHMARK/.log"



#2 tests
BENCHMARK="6_Fattahi" && mkdir -p "$OUTPUT_BASE/$BENCHMARK" && python3 SchedulingAlgorithms/scripts/batch-runner.py "$OUTPUT_BASE/$BENCHMARK" "$BENCHMARK" "v0,v1" | tee "$OUTPUT_BASE/$BENCHMARK/.log"
BENCHMARK="5_Kacem" && mkdir -p "$OUTPUT_BASE/$BENCHMARK" && python3 SchedulingAlgorithms/scripts/batch-runner.py "$OUTPUT_BASE/$BENCHMARK" "$BENCHMARK" "v2,v3" | tee "$OUTPUT_BASE/$BENCHMARK/.log"
BENCHMARK="2a_Hurink_sdata" && mkdir -p "$OUTPUT_BASE/$BENCHMARK" && python3 SchedulingAlgorithms/scripts/batch-runner.py "$OUTPUT_BASE/$BENCHMARK" "$BENCHMARK" "v2Squad,v3Squad" | tee "$OUTPUT_BASE/$BENCHMARK/.log"
BENCHMARK="2d_Hurink_vdata" && mkdir -p "$OUTPUT_BASE/$BENCHMARK" && python3 SchedulingAlgorithms/scripts/batch-runner.py "$OUTPUT_BASE/$BENCHMARK" "$BENCHMARK" "v2Squad,v3Squad" | tee "$OUTPUT_BASE/$BENCHMARK/.log"