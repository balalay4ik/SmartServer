DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 ASPNETCORE_URLS="http://0.0.0.0:5000" dotnet HomeServer/HomeServer.Api.dll

dotnet publish src/HomeServer.Api/HomeServer.Api.csproj -c Release -r linux-bionic-arm64 -o ./publish/termux

scp -P 8022 -r publish/termux/* NMO-Server@192.168.68.109:~/HomeServer/