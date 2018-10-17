REGISTRY_HOST="$(sed -nr "/^\[Network\]/ { :l /^RegistryHost[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.registry.production.ini)"
REGISTRY_EXTERNAL_PORT="$(sed -nr "/^\[Network\]/ { :l /^RegistryExternalPort[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.registry.production.ini)"

docker run -d \
  --name houguildbot \
  --restart unless-stopped \
  -v hou-guildbot-logs-volume:/app/logs \
  $REGISTRY_HOST:$REGISTRY_EXTERNAL_PORT/houguildbotwebhost:latest
