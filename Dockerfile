# Stage 1: Build Environment
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project files maintaining their folder structure
COPY ["FileCraftAPI/FileCraftAPI.csproj", "FileCraftAPI/"]
COPY ["FileSharingSystem/FileSharingSystem.csproj", "FileSharingSystem/"]

# Restore dependencies for the main API (this automatically restores referenced projects too)
RUN dotnet restore "FileCraftAPI/FileCraftAPI.csproj"

# Copy the rest of the source code for all projects
COPY . .

# Change working directory to the API project to build it
WORKDIR "/src/FileCraftAPI"
RUN dotnet build "FileCraftAPI.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "FileCraftAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final Runtime Environment
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

# Start the API
ENTRYPOINT ["dotnet", "FileCraftAPI.dll"]