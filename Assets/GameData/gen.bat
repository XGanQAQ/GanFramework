set WORKSPACE=..
set LUBAN_DLL=%WORKSPACE%\GanFramework\Editor\Luban\Luban.dll
set CONF_ROOT=.

dotnet %LUBAN_DLL% ^
    -t all ^
    -c cs-simple-json ^
    -d json ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputCodeDir=%WORKSPACE%\Scripts\Gameplay\Auto\ConfigsData\ ^
    -x outputDataDir=Configs\Json\

pause