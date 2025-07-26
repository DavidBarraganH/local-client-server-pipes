@echo off
echo ================================
echo Compilando el servidor...
echo ================================
dotnet build server/ServerPipeApp.csproj -c Release

echo ================================
echo Compilando el cliente...
echo ================================
dotnet build client/ClientPipeApp.csproj -c Release

echo ================================
echo Iniciando el servidor en nueva ventana...
echo ================================
start cmd /k "server\bin\Release\net8.0\ServerPipeApp.exe"

echo ================================
echo Iniciando el cliente en nueva ventana...
echo ================================
start cmd /k "client\bin\Release\net8.0\ClientPipeApp.exe"
