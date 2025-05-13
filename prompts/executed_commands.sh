git checkout development

dotnet new webapi -n OrchestratorService -o OrchestratorService
dotnet sln AgentAsAService.sln add OrchestratorService/OrchestratorService.csproj

dotnet build AgentAsAService.sln

dotnet new blazorwasm -n OrchestratorWebApp -o OrchestratorWebApp
dotnet sln AgentAsAService.sln add OrchestratorWebApp/OrchestratorWebApp.csproj

dotnet build AgentAsAService.sln

dotnet new classlib -n SharedLib -f netstandard2.0 -o SharedLib
dotnet sln AgentAsAService.sln add SharedLib/SharedLib.csproj
dotnet add AgentService/AgentService.csproj reference SharedLib/SharedLib.csproj
dotnet add OrchestratorService/OrchestratorService.csproj reference SharedLib/SharedLib.csproj

dotnet build AgentAsAService.sln

git add .
git commit -S -m "Add OrchestratorService, OrchestratorWebApp (Blazor WASM), SharedLib (.NET Standard 2.0), and update solution/project references"
git push origin development
