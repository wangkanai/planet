param(
    [parameter(Mandatory=$false)] [string]$add,
    [Parameter(Mandatory=$false)] [switch]$list,
    [Parameter(Mandatory=$false)] [switch]$remove,
    [Parameter(Mandatory=$false)] [switch]$clean,
    [Parameter(Mandatory=$false)] [switch]$update,
    [Parameter(Mandatory=$false)] [switch]$reset
)

$startup="./src/Server/Wangkanai.Planet.Portal.csproj"
$project="./src/Persistence/Wangkanai.Planet.Portal.Persistence.csproj"

if ($add)
{
    dotnet ef migrations add $add --startup-project $startup --project $project
}
if ($list -eq $true)
{
    dotnet ef migrations list --startup-project $startup --project $project --no-connect
}
if ($remove)
{
    dotnet ef migrations remove --startup-project $startup --project $project --force
}
if ($update)
{
    dotnet ef database update --startup-project $startup --project $project
}
if($clean)
{
    rimraf ./src/Persistence/Migrations
}
if($reset)
{
    ./db.ps1 -clean
    ./db.ps1 -add "initial"
}
