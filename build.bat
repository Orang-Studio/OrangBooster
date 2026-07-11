@echo off
setlocal
set "PROJ=%~dp0OrangBooster.csproj"
set "PUBDIR=%~dp0bin\Release\net8.0-windows10.0.19041.0\win-x86\publish"
set "DIST=%~dp0dist"
echo.
echo "Publishing" OrangBooster
dotnet publish "%PROJ%" -c Release -r win-x86 -p:Platform=x86 -p:PublishProfile=win-x86
if errorlevel 1 (
  echo.
  echo BUILD FAILED.
  exit /b 1
)

if not exist "%PUBDIR%\OrangBooster.exe" (
  echo.
  echo ERROR: OrangBooster.exe not found in publish folder.
  exit /b 1
)

echo.
echo Collecting artifact into dist
if not exist "%DIST%" mkdir "%DIST%"
copy /y "%PUBDIR%\OrangBooster.exe" "%DIST%\OrangBooster.exe" >nul
echo.
echo Verifying Authenticode signature
powershell -NoProfile -Command ^
  "$s = Get-AuthenticodeSignature '%DIST%\OrangBooster.exe';" ^
  "Write-Host ('Status : ' + $s.Status);" ^
  "if ($s.SignerCertificate) { Write-Host ('Signer : ' + $s.SignerCertificate.Subject); Write-Host ('Thumb  : ' + $s.SignerCertificate.Thumbprint) } else { Write-Host 'NOT SIGNED' }"

echo.
echo Done
echo Artifacts in: %DIST%
echo   - OrangBooster.exe
echo   - OrangBooster.exe.sha256
echo.
endlocal