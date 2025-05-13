# create a standard .NET visual studio git ignore file
# (No shell command required for this prompt)

# create a vs code build config for the solution
# (No shell command required for this prompt)

# checkout development branch
git checkout development

# create another project simiar to the first once calls OrchestratorService
dotnet new webapi -n OrchestratorService -o OrchestratorService
dotnet sln AgentAsAService.sln add OrchestratorService/OrchestratorService.csproj

dotnet build AgentAsAService.sln

# rebuild the solutojh to validate
dotnet build AgentAsAService.sln

# add a Blazor WebAssembly project called OrchestratorWebApp
dotnet new blazorwasm -n OrchestratorWebApp -o OrchestratorWebApp
dotnet sln AgentAsAService.sln add OrchestratorWebApp/OrchestratorWebApp.csproj

dotnet build AgentAsAService.sln

# Create a .NET STandard 2.0 shared library project called SharedLib, add it ot the soluton, and add project references to it from the two *Service projects
dotnet new classlib -n SharedLib -f netstandard2.0 -o SharedLib
dotnet sln AgentAsAService.sln add SharedLib/SharedLib.csproj
dotnet add AgentService/AgentService.csproj reference SharedLib/SharedLib.csproj
dotnet add OrchestratorService/OrchestratorService.csproj reference SharedLib/SharedLib.csproj

dotnet build AgentAsAService.sln

# commit the changes
git add .
git commit -S -m "Add OrchestratorService, OrchestratorWebApp (Blazor WASM), SharedLib (.NET Standard 2.0), and update solution/project references"

# Modify your commit command to enable signing
# (Already included -S flag in previous commit command)

# push the commit
git push origin development
