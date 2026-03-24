@echo off
::
:: deploy_reframework_deps.bat  <source-glob>
::
:: Called by the Umbra post-build event in Debug configuration.
:: Copies the built files matching <source-glob> to the REFramework
:: managed dependencies folder inside the configured game directory.
::
:: The game directory is read from game_dir.local.txt (solution root).
:: That file is written by setup_reframework_deps.bat and is gitignored,
:: so no paths are ever hardcoded in this script.
::
:: Exit codes:
::   0  — success, or gracefully skipped (config/dir not found)
::   1  — called without a source argument (programmer error)
::
setlocal EnableDelayedExpansion

set "SCRIPT_DIR=%~dp0"
set "GAME_DIR_FILE=%SCRIPT_DIR%..\game_dir.local.txt"
set "SOURCE_PATTERN=%~1"

if "%SOURCE_PATTERN%"=="" (
    echo [deploy] ERROR: No source pattern supplied.
    echo [deploy] Usage: deploy_reframework_deps.bat "path\to\output\Name*"
    exit /b 1
)

:: ---------------------------------------------------------------
:: Read the game directory
:: ---------------------------------------------------------------
if not exist "%GAME_DIR_FILE%" (
    echo [deploy] game_dir.local.txt not found — skipping deployment.
    echo [deploy] Run setup_reframework_deps.bat to configure your game directory.
    exit /b 0
)

for /f "usebackq tokens=* delims=" %%A in ("%GAME_DIR_FILE%") do set "GAME_DIR=%%A"

if "!GAME_DIR!"=="" (
    echo [deploy] Game directory is not configured in game_dir.local.txt — skipping.
    exit /b 0
)

:: ---------------------------------------------------------------
:: Validate paths
:: ---------------------------------------------------------------
if not exist "!GAME_DIR!\" (
    echo [deploy] Game directory not found: !GAME_DIR!
    echo [deploy] Update game_dir.local.txt or re-run setup_reframework_deps.bat.
    exit /b 0
)

set "DEST=!GAME_DIR!\reframework\plugins\managed\dependencies"

if not exist "!DEST!\" (
    echo [deploy] REFramework dependencies folder not found: !DEST!
    echo [deploy] Make sure REFramework is installed and the game has been launched at least once.
    exit /b 0
)

:: ---------------------------------------------------------------
:: Copy build output
:: ---------------------------------------------------------------
set "FILE_COUNT=0"

for %%F in (%SOURCE_PATTERN%) do (
    copy /Y "%%F" "!DEST!\" >nul
    echo [deploy] + %%~nxF
    set /a FILE_COUNT+=1
)

if !FILE_COUNT! equ 0 echo [deploy] No files matched pattern: %SOURCE_PATTERN%
if !FILE_COUNT! gtr 0 echo [deploy] Deployed !FILE_COUNT! file(s) to: !DEST!

endlocal
exit /b 0
