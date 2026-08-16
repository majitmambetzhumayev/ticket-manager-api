FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["TicketManager.API/TicketManager.API.csproj", "TicketManager.API/"]
COPY ["TicketManager.Application/TicketManager.Application.csproj", "TicketManager.Application/"]
COPY ["TicketManager.Domain/TicketManager.Domain.csproj", "TicketManager.Domain/"]
COPY ["TicketManager.Infrastructure/TicketManager.Infrastructure.csproj", "TicketManager.Infrastructure/"]
RUN dotnet restore "TicketManager.API/TicketManager.API.csproj"
COPY . .
RUN dotnet publish "TicketManager.API/TicketManager.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TicketManager.API.dll"]
