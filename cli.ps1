$ErrorActionPreference = 'Stop'

$ExeUrl    = 'https://github.com/Orang-Studio/OrangBooster/releases/latest/download/OrangBooster.exe'
$InstallDir = Join-Path $env:APPDATA 'OrangStudio\OrangBooster'
$ExePath    = Join-Path $InstallDir 'OrangBooster.exe'

try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch {}
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$orange = "$([char]27)[38;2;255;140;0m"
$reset  = "$([char]27)[0m"
$dim    = "$([char]27)[90m"

function Write-Banner {
    $b64 = 'IOKWiOKWiOKWiOKWiOKWiOKWiOKVlyDilojilojilojilojilojilojilZcgIOKWiOKWiOKWiOKWiOKWiOKVlyDilojilojilojilZcgICDilojilojilZcg4paI4paI4paI4paI4paI4paI4pWXIOKWiOKWiOKWiOKWiOKWiOKWiOKVlwrilojilojilZTilZDilZDilZDilojilojilZfilojilojilZTilZDilZDilojilojilZfilojilojilZTilZDilZDilojilojilZfilojilojilojilojilZcgIOKWiOKWiOKVkeKWiOKWiOKVlOKVkOKVkOKVkOKVkOKVnSDilojilojilZTilZDilZDilZDilZDilZ0K4paI4paI4pWRICAg4paI4paI4pWR4paI4paI4paI4paI4paI4paI4pWU4pWd4paI4paI4paI4paI4paI4paI4paI4pWR4paI4paI4pWU4paI4paI4pWXIOKWiOKWiOKVkeKWiOKWiOKVkSAg4paI4paI4paI4pWX4paI4paI4paI4paI4paI4pWXICAK4paI4paI4pWRICAg4paI4paI4pWR4paI4paI4pWU4pWQ4pWQ4paI4paI4pWX4paI4paI4pWU4pWQ4pWQ4paI4paI4pWR4paI4paI4pWR4pWa4paI4paI4pWX4paI4paI4pWR4paI4paI4pWRICAg4paI4paI4pWR4paI4paI4pWU4pWQ4pWQ4pWdICAK4pWa4paI4paI4paI4paI4paI4paI4pWU4pWd4paI4paI4pWRICDilojilojilZHilojilojilZEgIOKWiOKWiOKVkeKWiOKWiOKVkSDilZrilojilojilojilojilZHilZrilojilojilojilojilojilojilZTilZ3ilojilojilojilojilojilojilojilZcKIOKVmuKVkOKVkOKVkOKVkOKVkOKVnSDilZrilZDilZ0gIOKVmuKVkOKVneKVmuKVkOKVnSAg4pWa4pWQ4pWd4pWa4pWQ4pWdICDilZrilZDilZDilZDilZ0g4pWa4pWQ4pWQ4pWQ4pWQ4pWQ4pWdIOKVmuKVkOKVkOKVkOKVkOKVkOKVkOKVnQ=='
    $art = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($b64)) -split "`n"
    Write-Host ''
    foreach ($line in $art) { Write-Host "$orange$($line.TrimEnd([char]13))$reset" }
    Write-Host "$dim              Windows debloater CLI by OrangStudio$reset"
    Write-Host ''
}

function Download-WithProgress {
    param([string]$Url, [string]$OutFile)

    $req = [System.Net.HttpWebRequest]::Create($Url)
    $req.AllowAutoRedirect = $true
    $req.UserAgent = 'OrangBooster-Installer'
    $resp = $req.GetResponse()
    $total = $resp.ContentLength
    $totalMB = if ($total -gt 0) { [math]::Round($total / 1MB, 0) } else { 0 }

    $in  = $resp.GetResponseStream()
    $out = [System.IO.File]::Create($OutFile)
    try {
        $buffer = New-Object byte[] (1MB)
        $read = 0
        $done = 0
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        while (($read = $in.Read($buffer, 0, $buffer.Length)) -gt 0) {
            $out.Write($buffer, 0, $read)
            $done += $read
            if ($sw.ElapsedMilliseconds -ge 100) {
                $doneMB = [math]::Round($done / 1MB, 0)
                if ($total -gt 0) {
                    $pct = [int](($done / $total) * 100)
                    Write-Progress -Activity 'Downloading OrangBooster' `
                        -Status "$doneMB MB / $totalMB MB" -PercentComplete $pct
                } else {
                    Write-Progress -Activity 'Downloading OrangBooster' -Status "$doneMB MB"
                }
                $sw.Restart()
            }
        }
        Write-Progress -Activity 'Downloading OrangBooster' -Completed
    } finally {
        $out.Close(); $in.Close(); $resp.Close()
    }
}

Write-Banner

New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null

Write-Host "$orange>>Downloading$reset OrangBooster.exe ..."
Download-WithProgress -Url $ExeUrl -OutFile $ExePath

Write-Host ''
Write-Host "$orange>>Launching$reset OrangBooster CLI ... $dim(accept the UAC prompt)$reset"
Start-Process -FilePath $ExePath -ArgumentList '--cli' | Out-Null
