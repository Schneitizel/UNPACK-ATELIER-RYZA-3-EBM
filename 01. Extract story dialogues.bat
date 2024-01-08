@echo off
color a

:: Extraction des dialogues du scénario
for /r event_en %%a in (*.ebm) do (
	"%~dp0\UnpackEBM.exe" %%~na.ebm %%~dpa
)
pause
exit