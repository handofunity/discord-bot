DOCKER_CONTAINER_IP="$(sed -nr "/^\[Network\]/ { :l /^DockerContainerIP[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.production.ini)"

docker run \
  -d \
  --name seq \
  --restart unless-stopped \
  --cpus="1.0" \
  --cpu-shares=256 \
  --memory=192m \
  --memory-swap=400m \
  --security-opt="no-new-privileges:true" \
  --pids-limit=100 \
  --network docker-network \
  --ip $DOCKER_CONTAINER_IP \
  -e "ACCEPT_EULA=Y" \
  -e "BASE_URI=https://logs.handofunity.eu/" \
  -v "/var/log/hand-of-unity/seq:/data" \
  datalust/seq:latest