cc = gcc
prom = orbitTools.a
deps = $(shell find ./ -name "*.h")
src = $(shell find ./ -name "*.cpp")
obj = $(src:%.cpp=%.o) 

#$(prom): $(obj)
#	$(cc) -o $(prom) $(obj)
$(prom): $(obj)
	ar rv $(prom) $(obj) 

%.o: %.cpp $(deps)
	$(cc) -I ../core -c $< -o $@ 

clean:
	rm -rf $(obj) $(prom)
