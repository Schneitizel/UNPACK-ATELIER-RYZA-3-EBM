@echo off
color a

:: Extraction des dialogues du scénario
for /r event_en %%a in (*.txt) do (
	"%~dp0\UnpackEBM.exe" %%~na.txt %%~dpa
)
pause
exit