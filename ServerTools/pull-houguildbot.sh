REGISTRY_HOST="$(sed -nr "/^\[Network\]/ { :l /^RegistryHost[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.registry.production.ini)"
REGISTRY_EXTERNAL_PORT="$(sed -nr "/^\[Network\]/ { :l /^RegistryExternalPort[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.registry.production.ini)"

docker login $REGISTRY_HOST:$REGISTRY_EXTERNAL_PORT
docker pull $REGISTRY_HOST:$REGISTRY_EXTERNAL_PORT/houguildbotwebhost:latest
docker logout $REGISTRY_HOST:$REGISTRY_EXTERNAL_PORT