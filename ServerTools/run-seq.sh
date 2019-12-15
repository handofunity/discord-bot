docker run -d \
  --name seq \
  --restart unless-stopped \
  --cap-drop=ALL \
  --cap-add=SETFCAP \
  --cpus="1.0" \
  --cpu-shares=256 \
  --memory=192m \
  --memory-swap=400m \
  --security-opt="no-new-privileges:true" \
  --pids-limit=100 \
  -v /var/log/hand-of-unity/seq:/data \
  -e COMPlus_EnableDiagnostics=0 \
  -e ACCEPT_EULA=Y \
  -e BASE_URI=logs.handofunity.eu \
  -p 10661:80 \
  datalist/seq:latest