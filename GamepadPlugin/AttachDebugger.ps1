$processName = "LumosGUI"
$vsDteVersion = "17.0"  # Für Visual Studio 2022 → ggf. anpassen für andere Versionen

$launcherPath = "C:\Program Files (x86)\DMXControl3\Launcher\DMXCLauncher.exe"
$launcherArgs = "--fulldmxc"
$workingDir = "C:\Program Files (x86)\DMXControl3\GUI\Plugins\api-dlls"

# Starte den Launcher asynchron mit spezifischem Arbeitsverzeichnis
Start-Process -FilePath $launcherPath -ArgumentList $launcherArgs -WorkingDirectory $workingDir
Write-Host "🚀 Launcher gestartet: $launcherPath $launcherArgs"
Write-Host "📁 Arbeitsverzeichnis: $workingDir"

# Warte auf LumosGUI.exe
$proc = $null
for ($i = 0; $i -lt 30; $i++) {
    $proc = Get-Process -Name $processName -ErrorAction SilentlyContinue
    if ($proc) { break }
    Start-Sleep -Seconds 1
}
if (-not $proc) {
    Write-Host "Prozess $processName nicht gefunden."
    exit 1
}

# COM-Zugriff auf Visual Studio DTE holen
$dte = [Runtime.InteropServices.Marshal]::GetActiveObject("VisualStudio.DTE.$vsDteVersion")
$debugger = $dte.Debugger

# Debugger an den Prozess hängen
foreach ($p in $debugger.LocalProcesses) {
    if ($p.ProcessID -eq $proc.Id) {
        $p.Attach()
        Write-Host "Debugger an $processName (PID $($proc.Id)) angehängt."
        exit 0
    }
}

Write-Host "Prozess $processName nicht gefunden im Debugger."
exit 1
