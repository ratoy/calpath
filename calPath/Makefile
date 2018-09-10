cc = g++
cxxflags = -std=c++11

prom = calpath
deps = $(shell find ./ -name "*.h")
src = $(shell find ./ -name "*.cpp")
obj = $(src:%.cpp=%.o) 

$(prom): $(obj)
	$(cc) $(cxxflags) -o $(prom) $(obj) ../orbitTools/orbit/orbitTools.a ../orbitTools/core/coreLib.a

%.o: %.cpp $(deps)
	$(cc) $(cxxflags) -I ../orbitTools/orbit -I ../orbitTools/core -c $< -o $@ 

clean:
	rm -rf $(obj) $(prom)
