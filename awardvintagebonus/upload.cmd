docker login gzcontainerregistry.azurecr.io -u gzContainerRegistry -p P7QqmKxsMPUyrPtB7gAT6lM94+7VUoRC
docker tag awardbonus gzcontainerregistry.azurecr.io/awardbonus:v%1
docker push gzcontainerregistry.azurecr.io/awardbonus:v%1
az acr repository show-tags --name gzContainerRegistry --repository awardbonus --output table
