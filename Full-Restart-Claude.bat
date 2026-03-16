@echo off
:: ============================================
::   Full Close and Restart Claude + Service
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
echo   Full Close and Restart Claude + Service
echo ============================================
echo.

:: --- 1. Close Claude app ---
echo [1/4] Closing Claude...
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
echo [2/4] Stopping CoworkVMService...
sc query CoworkVMService >nul 2>&1
if %errorlevel% neq 0 (
    echo   CoworkVMService not found, skipping.
    goto start_service
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

:: --- 3. Start CoworkVMService ---
:start_service
echo [3/4] Starting CoworkVMService...
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

:: --- 4. Reopen Claude via Windows Store app launcher ---
:launch_claude
echo [4/4] Starting Claude...
start "" explorer.exe shell:AppsFolder\Claude_pzs8sxrjxfjjc!Claude
echo   Claude launched.

echo.
echo ============================================
echo   Claude fully restarted!
echo ============================================
timeout /t 3 /nobreak >nul
exit
