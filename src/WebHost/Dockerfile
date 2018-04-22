FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
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
ENTRYPOINT ["dotnet", "WebHost.dll"]
