@setlocal
@echo off

sig\crypt.exe decrypt sig\sns key.snk
sig\crypt.exe enablesigning src\System.Extensions\System.Extensions.NET4.csproj ..\..\key.snk
sig\crypt.exe enablesigning src\System.Extensions\System.Extensions.NETStandard.csproj ..\..\key.snk netstandard
sig\crypt.exe enablesigning src\System.Extensions.Test\System.Extensions.Test.NET4.csproj ..\..\key.snk
sig\crypt.exe enablesigning src\System.Extensions.Test\System.Extensions.Test.NETStandard.csproj ..\..\key.snk netstandard
