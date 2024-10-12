@echo off
cmd.exe /c ""C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat" && cl /c enet.cpp && lib enet.obj && del enet.obj"
echo Library successfully created