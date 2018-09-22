FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
RUN ln -s /lib/x86_64-linux-gnu/libdl.so.2 /lib/x86_64-linux-gnu/libdl.so
RUN apt update
RUN apt install -y libgdiplus
RUN ln -s /usr/lib/libgdiplus.so /lib/x86_64-linux-gnu/libgdiplus.so

WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY HoU.GuildBot.sln ./
COPY WebHost/WebHost.csproj WebHost/
COPY Core/Core.csproj Core/
COPY DAL/DAL.csproj DAL/
COPY Shared/Shared.csproj Shared/
COPY BLL/BLL.csproj BLL/
RUN dotnet restore -nowarn:msb3202,nu1503 -p:RestoreUseSkipNonexistentTargets=false
COPY . .
WORKDIR /src/WebHost
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "HoU.GuildBot.WebHost.dll"]