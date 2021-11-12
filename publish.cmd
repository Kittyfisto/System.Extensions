@setlocal
@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

echo Files in directory:
dir . /a-d

echo Looking for nuget package...
set FOLDER=Bin/Release
FOR %%F IN (%FOLDER%/System.Threading.Extensions.*.nupkg) DO (
 set NUGET_PACKAGE=%FOLDER%/%%F
)

if not "%NUGET_PACKAGE%"=="" (
	echo Found nuget package: %NUGET_PACKAGE%
	if "%APPVEYOR_REPO_BRANCH%"=="master" (
		echo Publishing nuget package...
		@nuget push -Source https://api.nuget.org/v3/index.json -ApiKey %NUGET_API_KEY% %NUGET_PACKAGE%
	) else (
		echo Not publishing from branch "%APPVEYOR_REPO_BRANCH%"...
	)
) else (
	echo Did not find anything to publish!
)
