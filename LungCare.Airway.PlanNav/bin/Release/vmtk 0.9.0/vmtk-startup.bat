rem @echo off
set VMTK_DIR=%~dp0
set PATH=%VMTK_DIR%bin;%VMTK_DIR%lib\InsightToolkit;%VMTK_DIR%lib\Python;%PATH%
set PYTHONPATH=%VMTK_DIR%lib\site-packages;%VMTK_DIR%lib\vtk-5.6;%VMTK_DIR%lib\vmtk
cd %VMTK_DIR%
python bin\vmtk-exe.py
