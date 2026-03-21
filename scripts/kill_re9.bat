@echo off
setlocal

REM Check if re9.exe is running
tasklist /FI "IMAGENAME eq re9.exe" 2>NUL | find /I "re9.exe" >NUL
if %ERRORLEVEL%==0 (
    echo [PreBuild] re9.exe detected. Terminating...
    taskkill /IM re9.exe /F >NUL 2>&1

    REM Wait ~1 second for file locks to release
    ping 127.0.0.1 -n 2 >NUL
) else (
    echo [PreBuild] re9.exe not running.
)

REM Always succeed so Visual Studio doesn't treat this as a failure
exit /b 0