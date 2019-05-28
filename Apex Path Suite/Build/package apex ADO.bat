set msbpath="C:\Windows\Microsoft.NET\Framework\v4.0.30319"
%msbpath%\msbuild build.msbuild /p:Solution="Apex Advanced Dynamic Obstacles" /t:PackageSourceProduct
pause