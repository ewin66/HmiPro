@ECHO off
ping 1.1.1.1 -n 1 -w 1000 > nul
start "" .\HmiPro.exe %*