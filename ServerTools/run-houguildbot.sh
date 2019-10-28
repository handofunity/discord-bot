REGISTRY_HOST="$(sed -nr "/^\[Network\]/ { :l /^RegistryHost[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.registry.production.ini)"
REGISTRY_EXTERNAL_PORT="$(sed -nr "/^\[Network\]/ { :l /^RegistryExternalPort[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.registry.production.ini)"

docker run -d \
  --name houguildbot \
  --restart unless-stopped \
  --read-only \
  --cap-drop=ALL \
  --cap-add=SETFCAP \
  --cpus="1.0" \
  --cpu-shares=256 \
  --memory=192m \
  --memory-swap=400m \
  --security-opt="no-new-privileges:true" \
  --pids-limit=100 \
  -v /var/log/hand-of-unity/guild-bot:/app/logs \
  -v /usr/share/fonts/:/usr/share/fonts/external/:ro \
  -e COMPlus_EnableDiagnostics=0 \
  $REGISTRY_HOST:$REGISTRY_EXTERNAL_PORT/houguildbotwebhost:latest