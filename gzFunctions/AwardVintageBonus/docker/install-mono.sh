#!/usr/bin/env bash

wget http://download.mono-project.com/repo/xamarin.gpg
sudo apt-key add xamarin.gpg
echo "deb http://download.mono-project.com/repo/debian wheezy main" | sudo tee --append /etc/apt/sources.list.d/mono-xamarin.list
sudo apt -qqy update \
    && apt -qqy install \
    mono-complete \
    fsharp \
    unzip \
    wget \
    curl \
    && unzip -d /usr/local/bin chromedriver_linux64.zip

sudo certmgr -ssl -m https://go.microsoft.com
sudo certmgr -ssl -m https://nugetgallery.blob.core.windows.net
sudo certmgr -ssl -m https://nuget.org
sudo mozroots --import --sync