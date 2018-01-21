# Ref: https://github.com/SeleniumHQ/docker-selenium
# Ref: https://github.com/TheAngryByrd/docker-canopy/blob/master/Dockerfile
# Ref: https://github.com/RobCherry/docker-chromedriver/blob/master/Dockerfile

FROM fsharpcanopy

ENV CHROMEDRIVER_PORT 4444
ENV CHROMEDRIVER_WHITELISTED_IPS "127.0.0.1"
ENV CHROMEDRIVER_URL_BASE ''

EXPOSE 4444

RUN         mkdir -p /app
WORKDIR     /app
RUN         ln -fs /usr/local/bin/chromedriver ./chromedriver
COPY        ./packages/FSharp.Core/lib/net45/FSharp.Core.dll ./
COPY        ./packages/canopy/lib/canopy.dll ./packages/canopy/lib/
COPY        ./packages/FSharp.Data/lib/net45/*.dll ./
COPY        ./packages/FSharp.Data.TypeProviders/lib/net40/FSharp.Data.TypeProviders.dll ./
COPY        ./packages/FSharp.Data/lib/net45/*.dll ./packages/FSharp.Data/lib/net45/
COPY        ./packages/FSharp.Data.TypeProviders/lib/net40/FSharp.Data.TypeProviders.dll ./packages/FSharp.Data.TypeProviders/lib/net40/
COPY        ./packages/NLog/lib/net45/NLog.dll ./packages/NLog/lib/net45/
COPY        ./packages/Selenium.WebDriver/lib/net45/WebDriver.dll ./packages/Selenium.WebDriver/lib/net45/
COPY        ./packages/Selenium.WebDriver/lib/net45/WebDriver.dll ./
COPY        awardbonus.fsx ./
# RUN         fsharpc awardbonus.fsx
# RUN         rm awardbonus.fsx
RUN         ls -la
# ENTRYPOINT ["mono", "awardbonus.exe"]
ENTRYPOINT ["fsharpi", "awardbonus.fsx"]
# ENTRYPOINT ["sh","-c", "xvfb-run -a mono awardbonus.exe"]
