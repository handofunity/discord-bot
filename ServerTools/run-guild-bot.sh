CS_GUILD_BOT="$(sed -nr "/^\[DatabaseConnectionStrings\]/ { :l /^GuildBot[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.production.ini)"
CS_HANG_FIRE="$(sed -nr "/^\[DatabaseConnectionStrings\]/ { :l /^HangFire[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.production.ini)"
SEQ_SERVER_URL="$(sed -nr "/^\[Seq\]/ { :l /^ServerUrl[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.production.ini)"
SEQ_API_KEY="$(sed -nr "/^\[Seq\]/ { :l /^ApiKey[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.production.ini)"
DISCORD_BOT_TOKEN="$(sed -nr "/^\[Discord\]/ { :l /^BotToken[ ]*=/ { s/.*=[ ]*//; p; q;}; n; b l;}" ./settings.production.ini)"

docker run -d \
  --name guildbot \
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
  -v /usr/share/fonts/:/usr/share/fonts/external/:ro \
  -e COMPlus_EnableDiagnostics=0 \
  -e ConnectionStrings__HandOfUnityGuild="$CS_GUILD_BOT" \
  -e ConnectionStrings__HangFire="$CS_HANG_FIRE" \
  -e Seq__serverUrl="$SEQ_SERVER_URL" \
  -e Seq__apiKey="$SEQ_API_KEY" \
  -e Discord__botToken="$DISCORD_BOT_TOKEN" \
  handofunity/guildbot:latest