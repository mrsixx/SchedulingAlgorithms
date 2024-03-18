var sofaA integer;
var sofaB integer;

maximize z: 500*sofaA + 750*sofaB;

s.t. r1: sofaA <= 60;
s.t. r2: sofaB <= 50;
s.t. r3: sofaA + 2*sofaB<= 120;
s.t. r4: sofaA >= 0;
s.t. r5: sofaB >= 0;