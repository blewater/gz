# Ref: https://github.com/SeleniumHQ/docker-selenium
# Ref: https://github.com/TheAngryByrd/docker-canopy/blob/master/Dockerfile
# Ref: https://github.com/RobCherry/docker-chromedriver/blob/master/Dockerfile

FROM fsharpcanopy
WORKDIR     /app
COPY        awardbonus.fsx ./
# RUN         fsharpc awardbonus.fsx
# RUN         rm awardbonus.fsx
COPY ./bin/Debug/*.XML ./
COPY ./bin/Debug/*.dll ./
COPY ./bin/Debug/*.config ./
COPY ./bin/Debug/awardbonus.exe ./

RUN         ls -la
ENTRYPOINT ["mono", "awardbonus.exe"]
# ENTRYPOINT ["fsharpi", "awardbonus.fsx"]
# ENTRYPOINT ["sh","-c", "mono awardbonus.exe"]
# ENTRYPOINT ["sh","-c", "xvfb-run -a mono awardbonus.exe"]
