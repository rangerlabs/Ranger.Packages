rm ./pkgs/*.Redis*.nupkg
nuget pack ./src/Ranger.Redis -Build -Version $1 -OutputDirectory ./pkgs  -Properties Configuration=Release;AssemblyVersion=$1