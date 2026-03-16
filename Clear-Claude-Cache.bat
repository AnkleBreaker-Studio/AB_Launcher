@echo off
:: ============================================
::   Clear Claude Cache & Restart
::   AnkleBreaker Studio
::   Version-proof (no hardcoded paths)
:: ============================================

:: --- Auto-elevate to admin (required for service management) ---
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Requesting admin privileges...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

echo ============================================
echo   Clear Claude Cache ^& Restart
echo ============================================
echo.

:: --- 1. Close Claude app ---
echo [1/5] Closing Claude...
taskkill /f /im Claude.exe >nul 2>&1

:wait_close
timeout /t 1 /nobreak >nul
tasklist /fi "imagename eq Claude.exe" 2>nul | find /i "Claude.exe" >nul
if %errorlevel% equ 0 (
    echo   Still running, waiting...
    goto wait_close
)
echo   Claude closed.
echo.

:: --- 2. Stop CoworkVMService ---
echo [2/5] Stopping CoworkVMService...
sc query CoworkVMService >nul 2>&1
if %errorlevel% neq 0 (
    echo   CoworkVMService not found, skipping.
    goto delete_cache
)

:: Check if already stopped
sc query CoworkVMService | find "STOPPED" >nul 2>&1
if %errorlevel% equ 0 (
    echo   Service already stopped.
    goto stop_done
)

net stop CoworkVMService >nul 2>&1

set svc_retries=0
:wait_stop
sc query CoworkVMService | find "STOPPED" >nul 2>&1
if %errorlevel% equ 0 goto stop_done
set /a svc_retries+=1
if %svc_retries% geq 15 (
    echo   WARNING: Service still not stopped after 15s, continuing...
    goto stop_done
)
timeout /t 1 /nobreak >nul
goto wait_stop

:stop_done
echo   Service stopped.
echo.

:: --- 3. Delete cache folders ---
:delete_cache
echo [3/5] Deleting cache folders...

set "claude_appdata=%APPDATA%\Claude"
set deleted=0
set skipped=0

call :delete_folder "vm_bundles"
call :delete_folder "claude-code"
call :delete_folder "Cache"
call :delete_folder "Code Cache"
call :delete_folder "logs"
call :delete_folder "GPUCache"
call :delete_folder "DawnGraphiteCache"
call :delete_folder "DawnWebGPUCache"
call :delete_folder "blob_storage"

echo   Done: %deleted% deleted, %skipped% skipped.
echo.

:: --- 4. Start CoworkVMService ---
echo [4/5] Starting CoworkVMService...
sc query CoworkVMService >nul 2>&1
if %errorlevel% neq 0 (
    echo   CoworkVMService not found, skipping.
    goto launch_claude
)
net start CoworkVMService >nul 2>&1

set svc_retries=0
:wait_start
sc query CoworkVMService | find "RUNNING" >nul 2>&1
if %errorlevel% equ 0 goto start_done
set /a svc_retries+=1
echo   Waiting for service to be ready... (%svc_retries%/20)
if %svc_retries% geq 20 (
    echo   WARNING: Service not running after 20s, launching Claude anyway...
    goto start_done
)
timeout /t 1 /nobreak >nul
goto wait_start

:start_done
echo   Service is RUNNING.
echo.

:: --- 5. Reopen Claude ---
:launch_claude
echo [5/5] Starting Claude...
start "" explorer.exe shell:AppsFolder\Claude_pzs8sxrjxfjjc!Claude
echo   Claude launched.

echo.
echo ============================================
echo   Cache cleared and Claude restarted!
echo ============================================
timeout /t 3 /nobreak >nul
exit

:: ============================================
::   Subroutine: delete a folder under %claude_appdata%
:: ============================================
:delete_folder
set "folder_name=%~1"
set "full_path=%claude_appdata%\%folder_name%"
if exist "%full_path%" (
    rmdir /s /q "%full_path%" >nul 2>&1
    if not exist "%full_path%" (
        echo   [OK] %folder_name%
        set /a deleted+=1
    ) else (
        echo   [FAIL] %folder_name% - could not delete
    )
) else (
    echo   [SKIP] %folder_name% (not found)
    set /a skipped+=1
)
goto :eof
