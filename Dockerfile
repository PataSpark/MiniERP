
# 第一階段：使用 .NET 10 SDK 建置專案
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# 複製專案檔並還原 NuGet 套件
COPY ["MiniERP.csproj", "./"]
RUN dotnet restore "MiniERP.csproj"

# 複製其餘程式碼
COPY . .

# 發佈 Release 版本
RUN dotnet publish "MiniERP.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false


# 第二階段：使用 ASP.NET Core Runtime 執行
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# 使用非 root 帳號執行
USER $APP_UID

# 容器內的 HTTP 連接埠
ENV ASPNETCORE_HTTP_PORTS=10000

EXPOSE 10000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MiniERP.dll"]
