FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY HoU.GuildBot.sln ./
COPY WebHost/WebHost.csproj WebHost/
COPY Core/Core.csproj Core/
COPY DAL/DAL.csproj DAL/
COPY Shared/Shared.csproj Shared/
COPY BLL/BLL.csproj BLL/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/WebHost
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "HoU.GuildBot.WebHost.dll"]