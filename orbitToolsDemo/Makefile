cc = g++
prom = demo
deps = $(shell find ./ -name "*.h")
src = $(shell find ./ -name "*.cpp")
obj = $(src:%.cpp=%.o) 

$(prom): $(obj)
	$(cc) -o $(prom) $(obj) ../orbitTools/orbit/orbitTools.a ../orbitTools/core/coreLib.a

%.o: %.cpp $(deps)
	$(cc) -I ../orbitTools/orbit -I ../orbitTools/core -c $< -o $@ 

clean:
	rm -rf $(obj) $(prom)
