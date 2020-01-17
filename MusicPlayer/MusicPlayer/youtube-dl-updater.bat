
@echo off
echo Waiting for file handle to be closed ...
ping 127.0.0.1 -n 5 -w 1000 > NUL
move /Y "C:\Users\One\Documents\Visual Studio 2015\Projects\MusicPlayer using NAudio\MusicPlayer\MusicPlayer\youtube-dl.exe.new" "C:\Users\One\Documents\Visual Studio 2015\Projects\MusicPlayer using NAudio\MusicPlayer\MusicPlayer\youtube-dl.exe" > NUL
echo Updated youtube-dl to version 2019.11.28.
start /b "" cmd /c del "%~f0"&exit /b"
                
