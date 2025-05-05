SET SFD="C:\Program Files (x86)\Steam\steamapps\common\Superfighters Deluxe"
cd ..\..\..\

copy CT\bin\%1\SFDCT.exe.config %SFD%
copy CT\bin\%1\SFDCT.exe %SFD%
copy CT\bin\%1\0Harmony.dll %SFD%\SFDCT