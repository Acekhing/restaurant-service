FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG API_PROJECT
WORKDIR /src

COPY School.Management.Peters.sln ./
COPY src/Admin.API/Admin.API.csproj src/Admin.API/
COPY src/Student.API/Student.API.csproj src/Student.API/
COPY src/Teachers.API/Teachers.API.csproj src/Teachers.API/
COPY src/Admin.Data/Admin.Data.csproj src/Admin.Data/
COPY src/Teachers.Data/Teachers.Data.csproj src/Teachers.Data/
COPY src/Students.Data/Students.Data.csproj src/Students.Data/
COPY src/Data.Packages.Management/Data.Packages.Management.csproj src/Data.Packages.Management/
COPY src/Shared.Constants/Shared.Constants.csproj src/Shared.Constants/
COPY src/Shared.Auth/Shared.Auth.csproj src/Shared.Auth/
COPY src/Shared.Observability/Shared.Observability.csproj src/Shared.Observability/
COPY src/Shared.Billing/Shared.Billing.csproj src/Shared.Billing/

RUN dotnet restore

COPY src/ src/
RUN dotnet publish "src/${API_PROJECT}/${API_PROJECT}.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet"]
