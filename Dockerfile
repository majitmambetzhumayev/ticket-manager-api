FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM node:22-slim AS frontend-build
WORKDIR /frontend
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ .
RUN npm run build

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
COPY --from=frontend-build /frontend/dist ./wwwroot
ENTRYPOINT ["dotnet", "TicketManager.API.dll"]
