@setlocal
@echo off

sig\crypt.exe decrypt sig\sns key.snk
sig\crypt.exe enablesigning src\System.Extensions\System.Extensions.csproj ..\..\key.snk
sig\crypt.exe enablesigning src\System.Extensions.Test\System.Extensions.Test.csproj ..\..\key.snk

more src\System.Extensions\System.Extensions.csproj
