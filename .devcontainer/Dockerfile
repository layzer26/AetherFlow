# Microsofts .NET 8 sdk image
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Working directory inside container 
WORKDIR /app

COPY . .

# Restore dependencies 
RUN dotnet Restore

# Default command: open shell 
CMD ["bash"]