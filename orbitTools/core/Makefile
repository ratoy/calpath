cc = gcc
prom = coreLib.a
deps = $(shell find ./ -name "*.h")
src = $(shell find ./ -name "*.cpp")
obj = $(src:%.cpp=%.o) 

#$(prom): $(obj)
#	$(cc) -o $(prom) $(obj)
$(prom): $(obj)
	#ar rv $(prom) $(obj)
	ar rcs $(prom) $(obj)

%.o: %.cpp $(deps)
	$(cc) -c $< -o $@

clean:
	rm -rf $(obj) $(prom)
