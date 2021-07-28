FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["OrderService.API.csproj", "OrderService.API/"]
RUN dotnet restore "OrderService.API/OrderService.API.csproj"

COPY . OrderService.API/.
RUN dotnet publish -c Release -o /app OrderService.API/OrderService.API.csproj 

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS publish
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "OrderService.API.dll"]