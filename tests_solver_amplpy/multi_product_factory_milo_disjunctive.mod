
var profit >= -1000, <= 10000;
var x >= 0, <= 1000;
var y >= 0, <= 1000;

maximize maximize_profit: profit;

s.t. demand: x <= 40;
s.t. laborA: x + y <= 80;
s.t. technologies:
    ((profit == 40 * x + 30 * y and 2 * x + y <= 100)
    or
    (profit == 60 * x + 30 * y and 1.5 * x + y <= 100))
    and not
    ((profit == 40 * x + 30 * y and 2 * x + y <= 100)
    and
    (profit == 60 * x + 30 * y and 1.5 * x + y <= 100))
    ;
