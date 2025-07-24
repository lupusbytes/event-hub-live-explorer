FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-noble AS build
ARG TARGETARCH
WORKDIR /source

# Copy .csproj file for LupusBytes.Azure.EventHubs.LiveExplorer.Web
COPY src/LupusBytes.Azure.EventHubs.LiveExplorer.Web/*.csproj ./src/LupusBytes.Azure.EventHubs.LiveExplorer.Web/

# Copy .csproj files for projects referenced by LupusBytes.Azure.EventHubs.LiveExplorer.Web
COPY src/LupusBytes.Azure.EventHubs.LiveExplorer.Web/*.csproj ./src/LupusBytes.Azure.EventHubs.LiveExplorer.Web/
COPY src/LupusBytes.Azure.EventHubs.LiveExplorer.Web.Client/*.csproj ./src/LupusBytes.Azure.EventHubs.LiveExplorer.Web.Client/
COPY src/LupusBytes.Azure.EventHubs.LiveExplorer.Web.Contracts/*.csproj ./src/LupusBytes.Azure.EventHubs.LiveExplorer.Web.Contracts/


# Restore as distinct layer
RUN dotnet restore --arch $TARGETARCH ./src/LupusBytes.Azure.EventHubs.LiveExplorer.Web

# Copy the rest of the source code
COPY src/ ./src/

# Copy required config files from root directory
COPY .editorconfig .
COPY Directory.Build.props .

# Build the app
RUN dotnet publish --arch $TARGETARCH --no-restore ./src/LupusBytes.Azure.EventHubs.LiveExplorer.Web/*.csproj -o /app

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled
EXPOSE 5000
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["./LupusBytes.Azure.EventHubs.LiveExplorer.Web"]