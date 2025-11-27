# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directories for uploaded images
RUN mkdir -p /app/wwwroot/images/listings

# Create wwwroot/libs directory with a .gitkeep file to satisfy ABP check
# ABP verifies that this folder exists and is not empty
# The libs are bundled via ABP bundling system, so we don't need actual files
RUN mkdir -p /app/wwwroot/libs && echo "# This folder is required by ABP Framework but libs are bundled" > /app/wwwroot/libs/.gitkeep

# Set environment variables for STAGING
ENV ASPNETCORE_ENVIRONMENT=Staging
ENV ASPNETCORE_URLS=http://+:8080
ENV Kestrel__Endpoints__Http__Url=http://0.0.0.0:8080

ENTRYPOINT ["dotnet", "cima.Blazor.dll"]
