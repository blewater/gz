@echo off
REM build & push gz.sln to stage

git -C .. checkout develop
git -C .. pull
git -C .. status

REM Default choice N to switch to master after 120 seconds

Choice /M "Press Y to continue with master or N to stop?" /c YN /D N /T 120
If Errorlevel 2 exit /B
git -C .. checkout master
git -C .. merge develop
msbuild ../gz.sln /t:Build /p:Configuration=Release

REM Default choice N to push to stage after 120 seconds

choice /M "Press Y to continue pushing latest to origin/master or N to stop" /c YN /D N /T 120
If Errorlevel 2 exit /B
git -C .. push

SET readSha = curl -s https://greenzorro-sgn.azurewebsites.net | grep -Eo "master.Sha.(\w+)" | cut -c12-
SET remoteSha = git -C .. rev-parse origin/master

if readSha == remoteSha
(
	echo echo "Successfully pushed latest and built master @ https://greenzorro-sgn.azurewebsites.net
	echo "opening now in default browser..."
	start "" https://greenzorro-sgn.azurewebsites.net
	git -C .. checkout develop
)
else
(	
	echo "Build failed in Azure. Please check in portal..."
	start "" https://portal.azure.com/#resource/subscriptions/d92ca232-a672-424c-975d-1dcf45a58b0b/resourceGroups/GreenzorroBizSpark/providers/Microsoft.Web/sites/greenzorro/slots/sgn/DeploymentSource
)
