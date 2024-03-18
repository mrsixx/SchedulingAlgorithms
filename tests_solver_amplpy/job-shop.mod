set V=1..n;
set E within {i in V, j in V: i<j};
set ED = {i in V, j in V: (i,j) in E or (j,i) in E};


set C = { i in V, j in V: (i,j) in ED };
set D = { i in V, j in V: (i,j) in E };

param p {V};
var s{V} >= 0;
minimize minimize_makespan: ??????? como representar S_(n+1) aqui???????;
s.t. conjunctions {(i,j) in C}: s[i] + p[i] <= s[j];
s.t disjunctions {(i,j) in D }: 
	((s[i] + p[i] <= s[j])
    or
    (s[j] + p[j] <= s[i]));