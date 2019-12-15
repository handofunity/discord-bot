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
  -e "ACCEPT_EULA=Y" \
  -e "BASE_URI=https://logs.handofunity.eu/" \
  -v "/var/log/hand-of-unity/seq:/data" \
  -p "10661:80" \
  datalust/seq:latest