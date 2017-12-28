
# Executables
MSBUILD=xbuild
NUGET=nuget

SOLUTION=Pleisure
PROJECT=Pleisure

# Build configuration, usually Debug or Release
CONFIGURATION=Debug

# Build directory relevant to the project
OUTPUT_DIR=bin/$(CONFIGURATION)


OUTPUT=$(PROJECT)/$(OUTPUT_DIR)
FLAGS= /p:PostBuildEvent= /verbosity:minimal /p:OutputPath=$(OUTPUT_DIR) /p:Configuration=$(CONFIGURATION)


targets:	nuget	build


# Restore packages
nuget:	$(SOLUTION).sln
	@$(NUGET) sources add -Name private -Source https://nuget.gmantaos.com/api/v2/ 2> /dev/null || true
	$(NUGET) restore $(SOLUTION).sln
	@$(NUGET) sources remove -Name private 2> /dev/null || true




# Build the solution 
build: $(SOLUTION).sln
	$(MSBUILD) $(SOLUTION).sln $(FLAGS)
	rm -rf $(OUTPUT)/lib
	mkdir -p $(OUTPUT)/lib
	-mv $(OUTPUT)/*.dll $(OUTPUT)/lib/	2> /dev/null
	-mv $(OUTPUT)/*.xml $(OUTPUT)/lib/	2> /dev/null
	-mv $(OUTPUT)/*.so $(OUTPUT)/lib/	2> /dev/null
	-mv $(OUTPUT)/*dylib  $(OUTPUT)/lib/	2> /dev/null

clean:
	rm -r $(OUTPUT)/*
