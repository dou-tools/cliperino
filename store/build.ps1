param([switch]$Install)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
$root = Split-Path $PSScriptRoot -Parent
$out = Join-Path $root "publish\store"
$layout = Join-Path $out "layout"
$staging = Join-Path $out "pri"
$cache = Join-Path $out "cache"

$makeappx = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.*\x64\makeappx.exe" -ErrorAction SilentlyContinue |
    Sort-Object { [version]$_.Directory.Parent.Name } | Select-Object -Last 1
if (-not $makeappx) { throw "No se encontró makeappx.exe. Instalá el Windows SDK." }
$makepri = Join-Path $makeappx.DirectoryName "makepri.exe"

$version = (Select-Xml -Path (Join-Path $root "Cliperino.csproj") -XPath "//Version").Node.InnerText

foreach ($dir in $layout, $staging) {
    if (Test-Path $dir) { Remove-Item $dir -Recurse -Force }
}

dotnet publish (Join-Path $root "Cliperino.csproj") -c Release -p:Store=true
if ($LASTEXITCODE -ne 0) { throw "Falló dotnet publish." }

$locator = Get-Content (Join-Path $root "Ffmpeg\FfmpegLocator.cs") -Raw
$hash = [regex]::Match($locator, 'DownloadSha256 = "([0-9a-f]{64})"').Groups[1].Value
$urls = [regex]::Matches($locator, '"(https://[^"]+\.zip)"') | ForEach-Object { $_.Groups[1].Value }
$zip = Join-Path $cache ([IO.Path]::GetFileName(([Uri]$urls[0]).AbsolutePath))
New-Item -ItemType Directory -Force $cache | Out-Null
if (-not (Test-Path $zip) -or (Get-FileHash $zip -Algorithm SHA256).Hash -ne $hash) {
    foreach ($url in $urls) {
        try {
            Invoke-WebRequest $url -OutFile $zip -UseBasicParsing
            break
        }
        catch {
            Write-Warning "No se pudo descargar $url"
        }
    }
    if (-not (Test-Path $zip) -or (Get-FileHash $zip -Algorithm SHA256).Hash -ne $hash) {
        Remove-Item $zip -ErrorAction SilentlyContinue
        throw "El ffmpeg descargado no coincide con el SHA-256 esperado."
    }
}

$licenses = Join-Path $layout "licenses"
New-Item -ItemType Directory -Force $licenses | Out-Null
Copy-Item (Join-Path $root "LICENSE") (Join-Path $licenses "Cliperino-LICENSE.txt")
$wanted = @{
    "bin/ffmpeg.exe" = Join-Path $layout "ffmpeg.exe"
    "LICENSE" = Join-Path $licenses "ffmpeg-LICENSE.txt"
    "README.txt" = Join-Path $licenses "ffmpeg-README.txt"
}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($zip)
try {
    foreach ($entry in $archive.Entries) {
        $relative = $entry.FullName.Substring($entry.FullName.IndexOf("/") + 1)
        if (-not $wanted.ContainsKey($relative)) { continue }
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $wanted[$relative], $true)
        $wanted.Remove($relative)
    }
}
finally {
    $archive.Dispose()
}
if ($wanted.Count -gt 0) { throw "Faltan archivos en el zip de ffmpeg: $($wanted.Keys -join ', ')" }

[xml]$manifest = Get-Content (Join-Path $PSScriptRoot "AppxManifest.xml") -Raw -Encoding UTF8
$manifest.Package.Identity.Version = "$version.0"
$manifest.Save((Join-Path $layout "AppxManifest.xml"))
Copy-Item (Join-Path $PSScriptRoot "Assets") (Join-Path $layout "Assets") -Recurse

New-Item -ItemType Directory -Force $staging | Out-Null
Copy-Item (Join-Path $PSScriptRoot "Assets") (Join-Path $staging "Assets") -Recurse
Copy-Item (Join-Path $layout "AppxManifest.xml") $staging
$config = Join-Path $out "priconfig.xml"
$log = & $makepri createconfig /cf $config /dq en-US /pv 10.0.0 /o 2>&1
if ($LASTEXITCODE -ne 0) { $log; throw "Falló makepri createconfig." }
$log = & $makepri new /pr $staging /cf $config /mn (Join-Path $staging "AppxManifest.xml") /of (Join-Path $layout "resources.pri") /o 2>&1
if ($LASTEXITCODE -ne 0) { $log; throw "Falló makepri new." }

$package = Join-Path $out "Cliperino-$version.msix"
$log = & $makeappx.FullName pack /d $layout /p $package /o 2>&1
if ($LASTEXITCODE -ne 0) { $log; throw "Falló makeappx." }
Write-Host "Paquete listo: $package"

if ($Install) {
    Add-AppxPackage -Register (Join-Path $layout "AppxManifest.xml")
    Write-Host "Cliperino (Store) quedó instalado desde $layout"
}
