# Variables
TEST_DIR = EasySave_Project/EasySave_Project_Test
TOOL_PATH = .tools
REPORTS_PATH = $(TEST_DIR)/TestResults/**/coverage.cobertura.xml
REPORT_DIR = $(TEST_DIR)/coverage-report
DOTNET_REPORTGENERATOR = $(TOOL_PATH)/reportgenerator

coverage:
	cd $(TEST_DIR) && \
	dotnet test --collect:"XPlat Code Coverage" --verbosity normal && \
	dotnet tool install dotnet-reportgenerator-globaltool --tool-path $(TOOL_PATH) && \
	$(DOTNET_REPORTGENERATOR) -reports:'TestResults/**/coverage.cobertura.xml' -targetdir:'coverage-report' && \
	rm -rf $(TEST_DIR)/TestResults