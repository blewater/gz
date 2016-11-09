@echo off
REM build & push gz.sln to stage

git -C .. checkout develop
git -C .. pull
git -C .. status

REM Default choice N to switch to master after 20 seconds

Choice /M "Press Y to continue with master or N to stop?" /c YN /D N /T 60
If Errorlevel 2 exit /B
git -C .. checkout master
git -C .. merge develop
msbuild ../gz.sln /t:Build /p:Configuration=Release

REM Default choice N to push to stage after 20 seconds

choice /M "Press Y to continue pushing latest to origin/master or N to stop" /c YN /D N /T 60
If Errorlevel 2 exit /B
git -C .. push
git -C .. checkout develop
git -C .. status

curl https://greenzorro-sgn.azurewebsites.net
echo "Successfully pushed to https://greenzorro-sgn.azurewebsites.net"