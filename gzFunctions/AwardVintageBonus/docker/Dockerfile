# Ref: https://github.com/TheAngryByrd/docker-canopy/blob/master/Dockerfile
# Ref: https://github.com/RobCherry/docker-chromedriver/blob/master/Dockerfile
FROM fsharpcanopy

ENV CHROMEDRIVER_PORT 4444
ENV CHROMEDRIVER_WHITELISTED_IPS ""
ENV CHROMEDRIVER_URL_BASE ''

EXPOSE 4444

WORKDIR     /src
COPY        src/*.dll .
COPY        src/*.fsx .
RUN         ls -la
ENTRYPOINT ["sh","-c", "xvfb-run -a fsharpi awardbonus.fsx"]