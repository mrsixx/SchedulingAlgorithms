def pop_min(sink, fs, s):
    n = 0
    m = len(sink)
    for i in range(m):
        if fs[sink[i]](s) < fs[sink[n]](s):
            n = i
    tmp = sink[n]
    sink[n] = sink[m - 1]
    sink[m - 1] = tmp
    return sink.pop()

def prec_sched(D, ps, fs):
    n = len(D)
    s = 0
    for i in range(n): s += ps[i]
    out = [0 for i in range(n)]
    for x in range(n):
        for y in D[x]: out[y] += 1
    sink = [x for x in range(n) if out[x] == 0]
    sched = []
    while sink:
        y = pop_min(sink, fs, s)
        sched.append(y)
        s -= ps[y]
        for x in D[y]:
            out[x] -= 1
            if out[x] == 0: sink.append(x)
    sched.reverse()
    return sched

def main():
    D = [[], [0], [0], [1, 5, 0], [1], [4, 6], [4]]

    # 2 Exemplo de algoritmos guloso
    #ps = [3, 1, 4, 2, 9, 8, 7]
    ps = [4, 6, 3, 2, 1, 2, 5]
    f = [lambda x: x,
         lambda x: x**2,
         lambda x: x**3,
         lambda x: 100,
         lambda x: 0,
         lambda x: 0.5 * x,
         lambda x: x**2 + 1]
    
    sched = prec_sched(D, ps, f)
    #print(sched)
    for p in sched: print(p, end=", ")
    print('')



if __name__ == '__main__':
    main()