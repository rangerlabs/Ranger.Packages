rm ./pkgs/*.Monitoring*.nupkg
nuget pack ./src/Ranger.Monitoring -Build -Version $1 -OutputDirectory ./pkgs -Properties Configuration=Release;AssemblyVersion=$1