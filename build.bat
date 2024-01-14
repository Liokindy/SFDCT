SET SFD="C:\Program Files (x86)\Steam\steamapps\common\Superfighters Deluxe"
cd ..\..\..\

copy SFR\bin\%1\SFDCT.exe.config %SFD%
copy SFR\bin\%1\SFDCT.exe %SFD%
copy SFR\bin\%1\0Harmony.dll %SFD%\SFDCT