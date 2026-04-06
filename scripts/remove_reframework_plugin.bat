@echo off
::
:: remove_reframework_plugin.bat  <assembly-name>
::
:: Removes all plugin files from the REFramework managed plugin folder
:: that contain the given assembly name.
::
:: Uses game_dir.local.txt (solution root) to locate the game directory.
::
:: Exit codes:
::   0  — success, or gracefully skipped
::   1  — missing argument
::

setlocal EnableDelayedExpansion

set "SCRIPT_DIR=%~dp0"
set "GAME_DIR_FILE=%SCRIPT_DIR%..\game_dir.local.txt"
set "ASSEMBLY_NAME=%~1"

if "%ASSEMBLY_NAME%"=="" (
    echo [cleanup] ERROR: No assembly name supplied.
    echo [cleanup] Usage: remove_reframework_plugin.bat "MyPlugin"
    exit /b 1
)

:: ---------------------------------------------------------------
:: Read the game directory
:: ---------------------------------------------------------------
if not exist "%GAME_DIR_FILE%" (
    echo [cleanup] game_dir.local.txt not found — skipping.
    exit /b 0
)

for /f "usebackq tokens=* delims=" %%A in ("%GAME_DIR_FILE%") do set "GAME_DIR=%%A"

if "!GAME_DIR!"=="" (
    echo [cleanup] Game directory not configured — skipping.
    exit /b 0
)

:: ---------------------------------------------------------------
:: Validate paths
:: ---------------------------------------------------------------
if not exist "!GAME_DIR!\" (
    echo [cleanup] Game directory not found: !GAME_DIR!
    exit /b 0
)

set "DEST=!GAME_DIR!\reframework\plugins\managed"

if not exist "!DEST!\" (
    echo [cleanup] Plugin folder not found: !DEST!
    exit /b 0
)

:: ---------------------------------------------------------------
:: Delete matching files
:: ---------------------------------------------------------------
set "FILE_COUNT=0"

for %%F in ("!DEST!\*%ASSEMBLY_NAME%*") do (
    if exist "%%F" (
        del /Q "%%F"
        echo [cleanup] - %%~nxF
        set /a FILE_COUNT+=1
    )
)

if !FILE_COUNT! equ 0 echo [cleanup] No files matched: *%ASSEMBLY_NAME%*
if !FILE_COUNT! gtr 0 echo [cleanup] Removed !FILE_COUNT! file(s) from: !DEST!

endlocal
exit /b 0