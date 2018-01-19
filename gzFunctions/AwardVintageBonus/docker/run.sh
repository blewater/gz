#!/usr/bin/env bash


mono .paket/paket.exe restore
docker build -t gz-docker-canopy .
docker run  gz-docker-canopy