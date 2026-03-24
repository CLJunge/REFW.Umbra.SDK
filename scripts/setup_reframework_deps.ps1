#Requires -Version 5.1
<#
.SYNOPSIS
    REFramework C# API dependency setup.
.DESCRIPTION
    Downloads the latest C# API DLLs from REFramework-nightly, copies them to the
    project, and optionally installs them into your RE Engine game directory.
.PARAMETER NoPrompt
    Run non-interactively. Skips all optional prompts; only the required steps are performed.
.PARAMETER GamePath
    Full path to the RE Engine game executable. Bypasses the file-browser prompt.
#>
param(
    [switch]$NoPrompt,
    [string]$GamePath
)

$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------
$AssetName   = 'csharp-api.zip'
$ReleasesApi = 'https://api.github.com/repos/praydog/REFramework-nightly/releases'
$UserAgent   = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36'
$ApiDest     = Join-Path $PSScriptRoot '..\dependencies\reframework\api'
$GenDest     = Join-Path $PSScriptRoot '..\dependencies\reframework\generated'
$TempDir     = Join-Path $env:TEMP "refw_setup_$(Get-Random)"

# ---------------------------------------------------------------
# Utilities
# ---------------------------------------------------------------
function Invoke-Pause {
    if ($NoPrompt) { return }
    Write-Host 'Press any key to continue . . .' -NoNewline
    try   { [void][System.Console]::ReadKey($true) }
    catch { $null = Read-Host }
    Write-Host ''
}

function Invoke-Cleanup {
    Remove-Item -Recurse -Force $TempDir -ErrorAction SilentlyContinue
}

function Show-SetupError {
    Write-Host ''
    Write-Host ' ============================================================'
    Write-Host '   Setup did not complete successfully.'
    Write-Host '   Please review the error messages above and try again.'
    Write-Host ' ============================================================'
    Write-Host ''
    Invoke-Pause
    exit 1
}

# Prompts for a Y/N answer and returns $true for Yes, $false for No.
function Read-YesNo {
    param([string]$Prompt)
    do {
        $raw = (Read-Host $Prompt).Trim().ToUpper()
        if ($raw -eq 'Y' -or $raw -eq 'YES') { return $true  }
        if ($raw -eq 'N' -or $raw -eq 'NO')  { return $false }
        Write-Host '  Please enter Y or N.'
    } while ($true)
}

# Normalises ConvertTo-Json output to 2-space indentation and removes the
# double-space-after-colon artifact produced by PowerShell 5.1.
function Format-Json {
    param([Parameter(Mandatory, ValueFromPipeline)][string]$Json, [int]$IndentWidth = 2)
    process {
        $depth  = 0
        $result = foreach ($line in ($Json -split '\r?\n')) {
            $trimmed = $line.Trim()
            if (-not $trimmed) { continue }
            if ($trimmed -match '^[}\]]') { $depth = [Math]::Max(0, $depth - 1) }
            $padded = (' ' * ($depth * $IndentWidth)) + ($trimmed -replace ':\s{2,}', ': ')
            $padded
            if ($trimmed -match '[{\[]$') { $depth++ }
        }
        return $result -join [System.Environment]::NewLine
    }
}

# ---------------------------------------------------------------
# Step functions
# ---------------------------------------------------------------
function Get-LatestReleaseUrl {
    Write-Host '[1/4] Fetching latest nightly release info from GitHub...'
    Write-Host ''
    try {
        $releases = Invoke-RestMethod -Uri $ReleasesApi -Headers @{ 'User-Agent' = $UserAgent }
        $asset    = $releases[0].assets | Where-Object { $_.name -eq $AssetName } | Select-Object -First 1
        if (-not $asset) { throw "Asset '$AssetName' was not found in the latest release." }
        Write-Host "  Asset : $AssetName"
        Write-Host "  URL   : $($asset.browser_download_url)"
        Write-Host ''
        return $asset.browser_download_url
    } catch {
        Write-Host "[ERROR] GitHub API request failed: $($_.Exception.Message)"
        Show-SetupError
    }
}

function Download-Archive {
    param([string]$Url)
    Write-Host "[2/4] Downloading $AssetName..."
    Write-Host ''
    $null    = New-Item -ItemType Directory -Path $TempDir -Force
    $zipPath = Join-Path $TempDir $AssetName
    try {
        Invoke-WebRequest -Uri $Url -OutFile $zipPath
        if (-not (Test-Path $zipPath)) { throw 'Output file was not created.' }
    } catch {
        Write-Host "[ERROR] Download failed: $($_.Exception.Message)"
        Show-SetupError
    }
    Write-Host "  Saved to: $zipPath"
    Write-Host ''
    return $zipPath
}

function Extract-Archive {
    param([string]$ZipPath)
    Write-Host "[3/4] Extracting $AssetName..."
    Write-Host ''
    $extractDir = Join-Path $TempDir 'extracted'
    $null = New-Item -ItemType Directory -Path $extractDir -Force
    try {
        Expand-Archive -Path $ZipPath -DestinationPath $extractDir -Force
    } catch {
        Write-Host "[ERROR] Extraction failed: $($_.Exception.Message)"
        Show-SetupError
    }
    Write-Host "  Extracted to: $extractDir"
    Write-Host ''
    return $extractDir
}

function Copy-ApiDlls {
    param([string]$ExtractDir)
    Write-Host '[4/4] Copying API DLLs to dependencies\reframework\api...'
    Write-Host ''
    $null  = New-Item -ItemType Directory -Path $ApiDest -Force
    $count = 0

    # reframework/plugins/REFramework.NET.dll
    $refwDll = Join-Path $ExtractDir 'reframework\plugins\REFramework.NET.dll'
    if (Test-Path $refwDll) {
        Copy-Item -Path $refwDll -Destination $ApiDest -Force
        Write-Host '    + REFramework.NET.dll'
        $count++
    } else {
        Write-Host '[WARN] Expected file not found: reframework\plugins\REFramework.NET.dll'
    }

    # reframework/plugins/managed/dependencies/ — copy only the two DLLs directly
    # referenced by the SDK and plugin projects. AssemblyGenerator, REFCoreDeps, and
    # Microsoft.CodeAnalysis.* are REFramework host internals and are not referenced
    # by any project code; staging them would only add noise to the dependencies folder.
    $depsDir = Join-Path $ExtractDir 'reframework\plugins\managed\dependencies'
    $sdkDeps = @('Hexa.NET.ImGui.dll', 'HexaGen.Runtime.dll')
    if (Test-Path $depsDir) {
        foreach ($name in $sdkDeps) {
            $src = Join-Path $depsDir $name
            if (Test-Path $src) {
                Copy-Item -Path $src -Destination $ApiDest -Force
                Write-Host "    + $name"
                $count++
            } else {
                Write-Host "[WARN] Expected file not found in managed/dependencies: $name"
            }
        }
    } else {
        Write-Host '[WARN] Expected folder not found: reframework\plugins\managed\dependencies'
    }

    Write-Host ''
    Write-Host "  Done. Copied $count DLL file(s) to:"
    Write-Host "  $ApiDest"
    Write-Host ''

    if (-not (Get-ChildItem -Path $ApiDest -ErrorAction SilentlyContinue)) {
        Write-Host '[ERROR] No DLLs were found in the API destination folder.'
        Show-SetupError
    }
}

function Select-GameExecutable {
    $gameDirFile = Join-Path $PSScriptRoot '..\game_dir.local.txt'
    $gameExeFile = Join-Path $PSScriptRoot '..\game_exe.local.txt'
    $savedRoot   = if (Test-Path $gameDirFile) { (Get-Content $gameDirFile -Raw).Trim() } else { '' }
    $savedExe    = if (Test-Path $gameExeFile) { (Get-Content $gameExeFile -Raw).Trim() } else { '' }

    # Command-line override - bypass all prompts
    if ($GamePath) {
        $exe  = $GamePath.Trim()
        $root = [System.IO.Path]::GetDirectoryName($exe).TrimEnd('\')
        return [PSCustomObject]@{ Exe = $exe; Root = $root }
    }

    Write-Host ''
    Write-Host ' ============================================================'
    Write-Host '   Select Game Executable'
    Write-Host ' ============================================================'
    Write-Host ''
    Write-Host '  Selecting your game executable enables:'
    Write-Host '    - Automatic plugin deployment via deploy_reframework_deps.bat'
    Write-Host '    - Installing the C# API directly into your game folder'
    Write-Host '    - Copying generated game bindings for direct class references'
    Write-Host ''

    if ($NoPrompt) {
        Write-Host '  [INFO] -NoPrompt specified - skipping game executable selection.'
        return $null
    }

    if ($savedExe) {
        Write-Host '  Currently saved:'
        Write-Host "    Executable : $savedExe"
        Write-Host "    Directory  : $savedRoot"
        Write-Host ''
        Write-Host '   [Y]  Yes - use the saved settings above'
        Write-Host '   [C]  Change - select a different executable'
        Write-Host '   [N]  No  - skip'
    } else {
        Write-Host '   [Y]  Yes - browse for my game executable'
        Write-Host '   [N]  No  - skip'
    }
    Write-Host ''

    $action = ''
    do {
        $raw = (Read-Host '  Your answer').Trim().ToUpper()
        if ($raw -eq 'Y' -or $raw -eq 'YES') {
            $action = if ($savedExe) { 'use' } else { 'browse' }
        } elseif (($raw -eq 'C' -or $raw -eq 'CHANGE') -and $savedExe) {
            $action = 'browse'
        } elseif ($raw -eq 'N' -or $raw -eq 'NO') {
            $action = 'skip'
        } else {
            if ($savedExe) { Write-Host '  Please enter Y, C, or N.' }
            else           { Write-Host '  Please enter Y or N.' }
        }
    } while (-not $action)

    if ($action -eq 'skip') { return $null }
    if ($action -eq 'use')  { return [PSCustomObject]@{ Exe = $savedExe; Root = $savedRoot } }

    # Browse for executable
    Write-Host ''
    Write-Host ' Opening file browser...'
    Write-Host ' Please select your RE Engine game executable (.exe).'
    Write-Host ''

    Add-Type -AssemblyName System.Windows.Forms
    $dlg                  = New-Object System.Windows.Forms.OpenFileDialog
    $dlg.Title            = 'Select your RE Engine game executable'
    $dlg.Filter           = 'Executable files (*.exe)|*.exe|All files (*.*)|*.*'
    $dlg.InitialDirectory = if ($savedRoot -and (Test-Path $savedRoot)) { $savedRoot } else { [Environment]::GetFolderPath('ProgramFilesX86') }

    if ($dlg.ShowDialog() -ne [System.Windows.Forms.DialogResult]::OK -or -not $dlg.FileName) {
        Write-Host '  [INFO] No file selected. Skipping game configuration.'
        return $null
    }

    $exe  = $dlg.FileName.Trim()
    $root = [System.IO.Path]::GetDirectoryName($exe).TrimEnd('\')
    return [PSCustomObject]@{ Exe = $exe; Root = $root }
}

function Install-ApiToGame {
    param(
        [string]$ExtractDir,
        [string]$GameRoot
    )
    Write-Host ''
    Write-Host ' ============================================================'
    Write-Host '   C# API Not Installed in Game Directory'
    Write-Host ' ============================================================'
    Write-Host ''
    Write-Host '  The C# API assemblies were not found in:'
    Write-Host "    $GameRoot\reframework\plugins\"
    Write-Host ''
    Write-Host '  Would you like to install the C# API into your game now?'
    Write-Host '  (All files from the downloaded zip will be copied to the'
    Write-Host '   game root folder, preserving the directory structure.)'
    Write-Host ''
    Write-Host '   [Y]  Yes - install now'
    Write-Host '   [N]  No  - skip install'
    Write-Host ''

    if ($NoPrompt -or -not (Read-YesNo '  Your answer (Y/N)')) { return }

    Write-Host ''
    Write-Host "  Copying C# API files to: $GameRoot"
    Write-Host '  (Preserving original folder structure...)'
    Write-Host ''
    try {
        Copy-Item -Path "$ExtractDir\*" -Destination $GameRoot -Recurse -Force
        Write-Host "  Done. C# API installed to: $GameRoot"
        Write-Host ''
    } catch {
        Write-Host "[ERROR] Failed to copy C# API files to the game directory: $($_.Exception.Message)"
        Show-SetupError
    }
}

function Copy-GeneratedBindings {
    param([string]$GameRoot)
    $genSource = Join-Path $GameRoot 'reframework\plugins\managed\generated'
    $found     = (Test-Path $genSource) -and [bool](Get-ChildItem -Path $genSource -Filter '*.dll' -ErrorAction SilentlyContinue)

    if (-not $found) {
        Write-Host ''
        Write-Host ' ============================================================'
        Write-Host '   Generated Bindings Not Found'
        Write-Host ' ============================================================'
        Write-Host ''
        Write-Host '  No generated game binding assemblies were found at:'
        Write-Host "    $genSource"
        Write-Host ''
        Write-Host '  These files are created the first time you launch your game'
        Write-Host '  with the C# API installed. Until they are present you must'
        Write-Host '  use string-based lookups instead of direct class references.'
        Write-Host ''
        Write-Host '  What to do:'
        Write-Host '    1. Launch your RE Engine game with REFramework loaded.'
        Write-Host '    2. Wait for the game to fully start up, then close it.'
        Write-Host '    3. Run this setup script again to copy the bindings.'
        Write-Host ''
        return $false
    }

    Write-Host "  Game root : $GameRoot"
    Write-Host "  Source    : $genSource"
    Write-Host ''
    Write-Host '  Copying generated binding DLLs to dependencies\reframework\generated...'
    Write-Host ''

    $null  = New-Item -ItemType Directory -Path $GenDest -Force
    $count = 0
    foreach ($f in Get-ChildItem -Path $genSource -Filter '*.dll') {
        Copy-Item -Path $f.FullName -Destination $GenDest -Force
        Write-Host "    + $($f.Name)"
        $count++
    }

    Write-Host ''
    Write-Host "  Done. Copied $count generated DLL(s) to:"
    Write-Host "  $GenDest"
    return $true
}

function Update-LaunchSettings {
    param([string]$GameExePath)
    $dest = Join-Path $PSScriptRoot '..\Umbra\Properties\launchSettings.json'
    $exe  = Get-Item -Path $GameExePath -ErrorAction SilentlyContinue
    if (-not $exe) {
        Write-Host "   [INFO] Configured exe not found: $GameExePath - skipping."
        return
    }

    $null = New-Item -ItemType Directory -Path (Split-Path $dest -Parent) -Force

    $profileName   = $exe.BaseName.ToUpper()
    $launchProfile = [ordered]@{ commandName = 'Executable'; executablePath = $exe.FullName }

    if (Test-Path $dest) {
        $obj = Get-Content $dest -Raw | ConvertFrom-Json

        if (-not $obj.profiles) {
            $obj | Add-Member -NotePropertyName 'profiles' -NotePropertyValue ([PSCustomObject]@{}) -Force
        }

        $profileExists = $obj.profiles.PSObject.Properties.Name -icontains $profileName
        if ($profileExists) {
            Write-Host "   [!] Profile $profileName already exists in launchSettings.json."
            if ($NoPrompt) {
                Write-Host '   Skipped - no changes made.'
                return
            }
            if (-not (Read-YesNo '   Overwrite it? (Y/N)')) {
                Write-Host '   Skipped - no changes made.'
                return
            }
        }

        $obj.profiles | Add-Member -NotePropertyName $profileName -NotePropertyValue $launchProfile -Force
        $obj | ConvertTo-Json -Depth 10 | Format-Json | Set-Content -Path $dest -Encoding UTF8
        Write-Host "   + Updated launchSettings.json: added/replaced profile $profileName"
    } else {
        $newSettings = [ordered]@{
            profiles = [ordered]@{
                'Umbra'  = [ordered]@{ commandName = 'Project' }
                $profileName = $launchProfile
            }
        }
        $newSettings | ConvertTo-Json -Depth 10 | Format-Json | Set-Content -Path $dest -Encoding UTF8
        Write-Host "   + Created launchSettings.json: added profile $profileName"
    }
}

function Invoke-AskLaunchSettings {
    param([string]$GameExePath)

    if (-not $GameExePath) {
        Write-Host '  [INFO] No game executable configured - skipping launchSettings.json.'
        return
    }

    Write-Host ''
    Write-Host ' ============================================================'
    Write-Host '   Configure Visual Studio Debug Profile  (Optional)'
    Write-Host ' ============================================================'
    Write-Host ''
    Write-Host '  Would you like to create or update launchSettings.json with'
    Write-Host '  an executable profile for your game?'
    Write-Host '  (Enables F5 debugging against the game in Visual Studio.)'
    Write-Host ''
    Write-Host '   [Y]  Yes - create or update launchSettings.json'
    Write-Host '   [N]  No  - skip'
    Write-Host ''

    if ($NoPrompt) {
        Write-Host '  [INFO] -NoPrompt specified - skipping launch settings.'
        return
    }

    if (Read-YesNo '  Your answer (Y/N)') {
        Update-LaunchSettings $GameExePath
        Write-Host ''
    }
}

# ---------------------------------------------------------------
# Main
# ---------------------------------------------------------------
try { $host.UI.RawUI.WindowTitle = 'REFramework C# API Setup' } catch {}

Write-Host ''
Write-Host ' ============================================================'
Write-Host '   REFramework C# API Setup'
Write-Host ' ============================================================'
Write-Host ''
Write-Host ' This script will:'
Write-Host '   1. Download the latest C# API DLLs from REFramework-nightly'
Write-Host '   2. Copy them to: dependencies\reframework\api\'
Write-Host '   3. (Optional) Install the C# API directly to your game directory'
Write-Host '   4. (Optional) Copy generated game bindings to:'
Write-Host '                  dependencies\reframework\generated\'
Write-Host '   5. (Optional) Add a Visual Studio debug launch profile for the game'
Write-Host ''

try {
    $downloadUrl = Get-LatestReleaseUrl
    $zipPath     = Download-Archive  $downloadUrl
    $extractDir  = Extract-Archive   $zipPath
    Copy-ApiDlls $extractDir

    $game = Select-GameExecutable
    if (-not $game) {
        Write-Host ''
        Write-Host '  [INFO] Game directory setup skipped.'
        Write-Host '  API reference DLLs are available at:'
        Write-Host "    $ApiDest"
        Write-Host ''
        Write-Host '  To enable deployment scripts and generated bindings later,'
        Write-Host '  run this script again and configure the game directory.'
        Write-Host ''
        Invoke-Pause
        exit 0
    }

    $gameExe  = $game.Exe
    $gameRoot = $game.Root

    Write-Host ''
    Write-Host "  Game root : $gameRoot"
    Write-Host "  Game exe  : $gameExe"
    Write-Host ''

    Set-Content -Path (Join-Path $PSScriptRoot '..\game_dir.local.txt') -Value $gameRoot -Encoding ASCII
    Write-Host '  Directory saved to:  game_dir.local.txt'
    Set-Content -Path (Join-Path $PSScriptRoot '..\game_exe.local.txt') -Value $gameExe -Encoding ASCII
    Write-Host '  Executable saved to: game_exe.local.txt'
    Write-Host ''

    Write-Host '  Scanning for C# API assemblies...'
    Write-Host ''

    $gameApiDll = Join-Path $gameRoot 'reframework\plugins\REFramework.NET.dll'
    if (-not (Test-Path $gameApiDll)) {
        Install-ApiToGame -ExtractDir $extractDir -GameRoot $gameRoot
        Invoke-Cleanup
        Write-Host ''
        Write-Host ' ============================================================'
        Write-Host '   Run the Game to Generate Bindings'
        Write-Host ' ============================================================'
        Write-Host ''
        Write-Host '  The C# API generates game-specific binding assemblies the first'
        Write-Host '  time the game is launched with the API installed. Until they'
        Write-Host '  exist, you must use string-based lookups instead of referencing'
        Write-Host '  game classes directly.'
        Write-Host ''
        Write-Host '  What to do next:'
        Write-Host '    1. Ensure the C# API is installed in your game directory.'
        Write-Host '    2. Launch your RE Engine game with REFramework loaded.'
        Write-Host '    3. Wait for the game to fully start up, then close it.'
        Write-Host '    4. Run this setup script again to copy the generated bindings.'
        Write-Host ''
        Invoke-AskLaunchSettings $gameExe
        Invoke-Pause
        exit 0
    }

    Write-Host '  [OK] C# API is installed.'
    Write-Host ''
    Invoke-Cleanup

    Write-Host '  Scanning for generated game binding assemblies...'
    Write-Host ''

    $genCopied = Copy-GeneratedBindings -GameRoot $gameRoot
    if (-not $genCopied) {
        Invoke-AskLaunchSettings $gameExe
        Invoke-Pause
        exit 0
    }

    Write-Host ''
    Write-Host ' ============================================================'
    Write-Host '   Setup Complete!'
    Write-Host ' ============================================================'
    Write-Host ''
    Write-Host "  API DLLs       : $ApiDest"
    Write-Host "  Generated DLLs : $GenDest"
    Write-Host '  Game directory : saved to game_dir.local.txt'
    Write-Host ''
    Write-Host ' The Debug build will now deploy plugin files automatically via'
    Write-Host ' deploy_reframework_deps.bat using the saved game directory.'
    Write-Host ''
    Invoke-AskLaunchSettings $gameExe
    Invoke-Pause
    exit 0
} finally {
    Invoke-Cleanup
}
