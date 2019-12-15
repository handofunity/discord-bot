FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
RUN ln -s /lib/x86_64-linux-gnu/libdl.so.2 /lib/x86_64-linux-gnu/libdl.so \
  && apt update \
  && apt install -y libgdiplus \
  && ln -s /usr/lib/libgdiplus.so /lib/x86_64-linux-gnu/libgdiplus.so

WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["WebHost/WebHost.csproj", "WebHost/"]
COPY ["DAL/DAL.csproj", "DAL/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["BLL/BLL.csproj", "BLL/"]
RUN dotnet restore "WebHost/WebHost.csproj"
COPY . .
WORKDIR "/src/WebHost"
RUN dotnet build "WebHost.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WebHost.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

RUN useradd -d /home/houguildbotuser -m -s /bin/bash houguildbotuser
USER houguildbotuser

ENTRYPOINT ["dotnet", "HoU.GuildBot.WebHost.dll"]