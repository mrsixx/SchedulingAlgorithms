# from amplpy import AMPL
# m = AMPL()
# m.read("multi_product_factory_milo_disjunctive.mod")
# m.option["solver"] = 'highs'
# m.solve()

# print(f"Profit = {m.var['profit'].value():.2f} €")
# print(f"x = {m.var['x'].value()}")
# print(f"y = {m.var['y'].value()}")


# m = AMPL()
# m.read("sofa.mod")
# m.option["solver"] = 'highs'
# m.solve()

# print(f"Profit = {m.obj['z'].value():.2f} €")
# print(f"sofaA = {m.var['sofaA'].value()}")
# print(f"sofaB = {m.var['sofaB'].value()}")



from amplpy import AMPL
import networkx as nx
import matplotlib.pyplot as plt

# 0 and 9 are dummy operations (source and sink)
operations = list(range(12))

conjunctions = [(0,1), (0,4),(0,6),(0,9),
				(1,2),(2,3),
				(4,5),
				(6,7),(7,8),
				(9,10),
				(3,11),(5,11),(8,11), (10,11)]
				
disjunctions = [(1,6), (3,5),(3,8),(5,8),(2,4),(2,7)
				(2,9),(7,4),(4,9),(7,9)]	

# O -> R+ representando o tempo de processamento p_i da operação i
process_time = [(0,0), (11,0),(1,2),(2,2),(3,1),(4,2),(5,7),(6,4),(7,6),(8,2),(9,3),(10,7)]				
# DG = nx.DiGraph()
# DG.add_nodes_from(operations)
# DG.add_edge(0,1)
# DG.add_edge(0,4)
# DG.add_edge(0,6)
# DG.add_edge(1,2)
# DG.add_edge(2,3)
# DG.add_edge(4,5)
# DG.add_edge(6,7)
# DG.add_edge(7,8)
# DG.add_edge(3,9)
# DG.add_edge(5,9)
# DG.add_edge(8,9)
# nx.draw(DG, with_labels=True, font_weight='bold')
# plt.savefig("path.png")
m = AMPL()
m.read("job-shop.mod")
m.set["V"] = operations
m.set["C"] = conjunctions
m.set["D"] = disjunctions
m.param["p"] = process_time

m.option["solver"] = 'highs'
m.solve()

print(f"Profit = {m.obj['z'].value():.2f} €")
print(f"sofaA = {m.var['sofaA'].value()}")
print(f"sofaB = {m.var['sofaB'].value()}")