# =========================================================
# Stage 1: BUILD - uses the full .NET SDK (compilers, NuGet)
# =========================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy ONLY the project file first and restore NuGet packages.
# This layer is cached and only re-runs when the .csproj changes.
COPY CustomerService/CustomerService.csproj CustomerService/
RUN dotnet restore CustomerService/CustomerService.csproj

# Now copy the rest of the source code and publish a Release build.
COPY CustomerService/ CustomerService/
RUN dotnet publish CustomerService/CustomerService.csproj -c Release -o /app/publish --no-restore

# =========================================================
# Stage 2: RUNTIME - small image with only the ASP.NET runtime
# =========================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Take only the compiled output from the build stage.
COPY --from=build /app/publish .

# ASP.NET Core in this image listens on port 8080 by default.
EXPOSE 8080

# Run as the built-in non-root user instead of root.
USER $APP_UID

ENTRYPOINT ["dotnet", "CustomerService.dll"]
