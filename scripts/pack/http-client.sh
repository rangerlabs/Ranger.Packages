rm ./pkgs/*.InternalHttpClient*.nupkg
nuget pack ./src/Ranger.InternalHttpClient -Build -Version $1 -OutputDirectory ./pkgs  -Properties Configuration=Release;AssemblyVersion=$1