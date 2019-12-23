rm ./pkgs/*.Mongo*.nupkg
nuget pack ./src/Ranger.Mongo -Build -Version $1 -OutputDirectory ./pkgs -Properties Configuration=Release;AssemblyVersion=$1