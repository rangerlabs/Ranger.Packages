rm ./pkgs/*.ApiUtilities*.nupkg
nuget pack ./src/Ranger.ApiUtilities -Build -Version $1 -OutputDirectory ./pkgs  -Properties Configuration=Release;AssemblyVersion=$1