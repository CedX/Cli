#load "Tools.fsx"
open Tools

writeOutput "Running the test suite..."
startProcess "dotnet" "test --settings .runsettings"
