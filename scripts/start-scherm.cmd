@echo off
rem Start de ICT-promotie-app en opent de schermweergave schermvullend in Edge.
rem Zet dit bestand in dezelfde map als IctPromotie.exe.
cd /d "%~dp0"
start "ICT-promotie" /min IctPromotie.exe
timeout /t 5 /nobreak >nul
start "" msedge --kiosk "http://localhost:5000/Scherm" --edge-kiosk-type=fullscreen --no-first-run
