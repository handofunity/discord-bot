REGISTRY_HOST="$(sed -nr "/^\[Network\]/ { :l /^RegistryHost[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.registry.production.ini)"
REGISTRY_EXTERNAL_PORT="$(sed -nr "/^\[Network\]/ { :l /^RegistryExternalPort[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.registry.production.ini)"
REGISTRY_INTERNAL_PORT="$(sed -nr "/^\[Network\]/ { :l /^RegistryInternalPort[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.registry.production.ini)"
IP_DOCKER_REGISTRY="$(sed -nr "/^\[Network\]/ { :l /^IpDockerRegistry[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.production.ini)"

docker run -d \
  --name registry \
  --restart always \
  --read-only \
  --cpus="0.5" \
  --cpu-shares=128 \
  --memory=192m \
  --memory-swap=400m \
  --security-opt="no-new-privileges:true" \
  --pids-limit=100 \
  --network docker-network \
  --ip $IP_DOCKER_REGISTRY \
  -v /var/docker-registry:/var/lib/registry \
  -v /etc/ssl/caddy/acme/acme-v02.api.letsencrypt.org/sites/$REGISTRY_HOST:/certs:ro \
  -e REGISTRY_HTTP_ADDR=0.0.0.0:$REGISTRY_INTERNAL_PORT \
  -e REGISTRY_HTTP_TLS_CERTIFICATE=/certs/$REGISTRY_HOST.crt \
  -e REGISTRY_HTTP_TLS_KEY=/certs/$REGISTRY_HOST.key \
  -e "REGISTRY_AUTH=htpasswd" \
  -e "REGISTRY_AUTH_HTPASSWD_REALM=Registry Realm" \
  -e REGISTRY_AUTH_HTPASSWD_PATH=/var/lib/registry/auth/htpasswd \
  -p $REGISTRY_EXTERNAL_PORT:$REGISTRY_INTERNAL_PORT \
  registry:2