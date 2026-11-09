# ==============================================================================
# DOCKERFILE CHO BACKEND MDMSv2 (.NET 8 WEB API & QUESTPDF ENGINE)
# Multi-stage build tối ưu kích thước image và bảo mật
# ==============================================================================

# STAGE 1: Base Runtime Image (ASP.NET Core 8.0 Linux Debian)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Cài đặt font tiếng Việt và thư viện đồ họa để QuestPDF & SkiaSharp hiển thị chuẩn xác
RUN apt-get update && apt-get install -y --no-install-recommends \
    fontconfig \
    fonts-dejavu \
    fonts-liberation \
    libgdiplus \
    iputils-ping \
    curl \
    && fc-cache -f -v \
    && rm -rf /var/lib/apt/lists/*

# STAGE 2: Build & Restore SDK Image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy toàn bộ cấu trúc các project file để tối ưu hóa Docker Layer Cache
COPY ["Backend/AMR.sln", "Backend/"]
COPY ["Backend/Core/Nobisoft.Core/Nobisoft.Core.csproj", "Backend/Core/Nobisoft.Core/"]
COPY ["Backend/Core/Nobisoft.Core.AspNetCore/Nobisoft.Core.AspNetCore.csproj", "Backend/Core/Nobisoft.Core.AspNetCore/"]
COPY ["Backend/Core/Nobisoft.Core.Authorization/Nobisoft.Core.Authorization.csproj", "Backend/Core/Nobisoft.Core.Authorization/"]
COPY ["Backend/Core/Nobisoft.Core.Configuration/Nobisoft.Core.Configuration.csproj", "Backend/Core/Nobisoft.Core.Configuration/"]
COPY ["Backend/Core/Nobisoft.Core.Database/Nobisoft.Core.Database.csproj", "Backend/Core/Nobisoft.Core.Database/"]
COPY ["Backend/Core/Nobisoft.Core.DI/Nobisoft.Core.DI.csproj", "Backend/Core/Nobisoft.Core.DI/"]
COPY ["Backend/Core/Nobisoft.Core.Domain/Nobisoft.Core.Domain.csproj", "Backend/Core/Nobisoft.Core.Domain/"]
COPY ["Backend/Core/Nobisoft.Core.HttpContextAccessor/Nobisoft.Core.HttpContextAccessor.csproj", "Backend/Core/Nobisoft.Core.HttpContextAccessor/"]
COPY ["Backend/Core/Nobisoft.Core.Infrastructure/Nobisoft.Core.Infrastructure.csproj", "Backend/Core/Nobisoft.Core.Infrastructure/"]
COPY ["Backend/Core/Nobisoft.Core.Logger/Nobisoft.Core.Logger.csproj", "Backend/Core/Nobisoft.Core.Logger/"]
COPY ["Backend/Core/Nobisoft.Core.Mailer/Nobisoft.Core.Mailer.csproj", "Backend/Core/Nobisoft.Core.Mailer/"]
COPY ["Backend/Domain/Domain.csproj", "Backend/Domain/"]
COPY ["Backend/Application/Application.csproj", "Backend/Application/"]
COPY ["Backend/Infrastructure/Infrastructure.csproj", "Backend/Infrastructure/"]
COPY ["Backend/WebApi/WebApi.csproj", "Backend/WebApi/"]
COPY ["Backend/Tests/MDMS.UnitTests/MDMS.UnitTests.csproj", "Backend/Tests/MDMS.UnitTests/"]

# Restore dependencies
RUN dotnet restore "Backend/WebApi/WebApi.csproj"

# Copy toàn bộ mã nguồn
COPY Backend/ Backend/

# Build dự án
WORKDIR "/src/Backend/WebApi"
RUN dotnet build "WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# STAGE 3: Publish App
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

# STAGE 4: Final Production Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Thiết lập biến môi trường mặc định
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "WebApi.dll"]
