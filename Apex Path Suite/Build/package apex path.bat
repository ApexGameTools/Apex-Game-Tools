set msbpath="C:\Windows\Microsoft.NET\Framework\v4.0.30319"
%msbpath%\msbuild build.msbuild /p:Solution="Apex Path";PlatformDependent=true /t:PackageSourceProduct
pause