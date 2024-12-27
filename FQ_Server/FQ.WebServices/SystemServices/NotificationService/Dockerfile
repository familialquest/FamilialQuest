#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["FQ.WebServices/SystemServices/NotificationService/NotificationService.csproj", "FQ.WebServices/SystemServices/NotificationService/"]
COPY ["FQ.Common/CommonDB/CommonDB.csproj", "FQ.Common/CommonDB/"]
COPY ["FQ.Common/CommonRoutes/CommonRoutes.csproj", "FQ.Common/CommonRoutes/"]
COPY ["FQ.Common/CommonLib/CommonLib.csproj", "FQ.Common/CommonLib/"]
COPY ["FQ.Common/CommonTypes/CommonTypes.csproj", "FQ.Common/CommonTypes/"]
RUN dotnet restore "FQ.WebServices/SystemServices/NotificationService/NotificationService.csproj"
COPY . .
WORKDIR "/src/FQ.WebServices/SystemServices/NotificationService"
RUN dotnet build "NotificationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.dll"]
