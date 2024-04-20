FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY Program/Program.csproj Program/Program.csproj
COPY kdz_last/kdz_last.csproj kdz_last/kdz_last.csproj
RUN dotnet restore Program/Program.csproj
COPY . .
WORKDIR /src
RUN dotnet build Program/Program.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Program/Program.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Program.dll"]
