# Ref: https://github.com/SeleniumHQ/docker-selenium
# Ref: https://github.com/TheAngryByrd/docker-canopy/blob/master/Dockerfile
# Ref: https://github.com/RobCherry/docker-chromedriver/blob/master/Dockerfile

FROM fsharpcanopy
WORKDIR     /app
COPY        awardbonus.fsx ./
COPY ./bin/Release/*.XML ./
COPY ./bin/Release/*.dll ./
COPY ./bin/Release/*.config ./
COPY ./bin/Release/awardbonus.exe ./

RUN         ls -la
ENTRYPOINT ["mono", "awardbonus.exe"]