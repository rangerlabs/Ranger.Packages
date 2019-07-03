rm ./pkgs/*.RabbitMQ*.nupkg
nuget pack ./src/Ranger.RabbitMQ -Build -Version $1 -OutputDirectory ./pkgs  -Properties Configuration=Release;AssemblyVersion=$1