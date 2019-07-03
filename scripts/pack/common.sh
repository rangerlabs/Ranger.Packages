rm ./pkgs/*.Common*.nupkg
nuget pack ./src/Ranger.Common -Build -Version $1 -OutputDirectory ./pkgs