REGISTRY_HOST="$(sed -nr "/^\[Network\]/ { :l /^RegistryHost[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.registry.production.ini)"
REGISTRY_EXTERNAL_PORT="$(sed -nr "/^\[Network\]/ { :l /^RegistryExternalPort[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.registry.production.ini)"

docker run -d \
  --name houguildbot \
  --restart unless-stopped \
  -v /var/log/hand-of-unity/guild-bot:/app/logs \
  -v /usr/share/fonts/:/usr/share/fonts/external/ \
  $REGISTRY_HOST:$REGISTRY_EXTERNAL_PORT/houguildbotwebhost:latest
