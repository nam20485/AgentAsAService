param (
    [Parameter(Mandatory = $true)]
    [string]$ServiceName
)

dotnet publish ./${ServiceName}/${ServiceName}.csproj -c Release -o ./${ServiceName}/publish /p:UseAppHost=false
