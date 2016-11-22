﻿REM One time Fake bootstraper. See build.fsx for instructions
@echo off
SETLOCAL

cls

.paket\paket.bootstrapper.exe
if errorlevel 1 (
    exit /b %errorlevel%
)

.paket\paket.exe restore
if errorlevel 1 (
    exit /b %errorlevel%
)

SET FAKE_PATH=packages\FAKE\tools\Fake.exe

IF [%1]==[] (
    "%FAKE_PATH%" "build.fsx" "mode=prod" 
) ELSE (
    "%FAKE_PATH%" "build.fsx" %*
)