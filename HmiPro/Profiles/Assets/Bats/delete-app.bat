@ECHO off
ping 1.1.1.1 -n 1 -w 5000 > nul
rd /s/q ..\Debug\