FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN ln -s /lib/x86_64-linux-gnu/libdl.so.2 /lib/x86_64-linux-gnu/libdl.so \
  && apt update \
  && apt install -y libgdiplus \
  && ln -s /usr/lib/libgdiplus.so /lib/x86_64-linux-gnu/libgdiplus.so

WORKDIR /app
COPY ./WebHost/fonts/Marcellus-Regular.ttf ./WebHost/fonts/linlibertine_regular.ttf ./
RUN mkdir -p /usr/share/fonts/truetype/ \
  && install -m644 Marcellus-Regular.ttf /usr/share/fonts/truetype/ \
  && install -m644 linlibertine_regular.ttf /usr/share/fonts/truetype/ \
  && rm ./Marcellus-Regular.ttf \
  && rm ./linlibertine_regular.ttf

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WebHost/WebHost.csproj", "WebHost/"]
COPY ["Keycloak/Keycloak.csproj", "Keycloak/"]
COPY ["DAL/DAL.csproj", "DAL/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["BLL/BLL.csproj", "BLL/"]
RUN dotnet restore "./WebHost/WebHost.csproj"
COPY . .
WORKDIR "/src/WebHost"
RUN dotnet build "./WebHost.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WebHost.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER app
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "guildbot.dll"]