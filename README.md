# Anti-Plagiarism System

A microservice-based web application for analyzing student reports and detecting plagiarism.

## Features

### Text Analysis
- **Statistics Calculation**
  - Paragraph count
  - Word count
  - Character count
- **Plagiarism Detection**
  - File similarity comparison
  - Similarity percentage calculation using Levenshtein distance

## Architecture

### Microservices
1. **API Gateway**
   - Handles client requests
   - Routes requests to appropriate microservices
   - Provides unified API interface
   - Implements error handling and service availability checks

2. **File Storing Service**
   - Manages file storage
   - Handles file uploads and retrievals
   - Implements file deduplication using hash-based storage
   - Stores file metadata in PostgreSQL

3. **File Analysis Service**
   - Performs text analysis
   - Generates word clouds
   - Calculates file similarity
   - Stores analysis results in PostgreSQL

### Error Handling
The system implements robust error handling for microservice failures:
- Service unavailability detection
- Graceful degradation
- Detailed error logging
- User-friendly error messages
- Automatic service recovery

## API Documentation

### Swagger
Interactive API documentation is available at `/swagger` when running in development mode.

### Postman Collection
A comprehensive Postman collection is provided in `AntiPlagiarismSystem.postman_collection.json` with the following endpoints:

1. **File Upload**
   ```
   POST /api/files/upload
   Content-Type: multipart/form-data
   ```

2. **Get File**
   ```
   GET /api/files/{fileId}
   ```

3. **Analyze File**
   ```
   POST /api/analysis/{fileId}
   ```

4. **Get Word Cloud**
   ```
   GET /api/analysis/{fileId}/wordcloud
   ```

5. **Compare Files**
   ```
   POST /api/analysis/compare
   Content-Type: application/json
   {
     "originalFileId": "guid",
     "comparedFileId": "guid"
   }
   ```

## Error Handling Details

### Service Unavailability
When a microservice becomes unavailable:
1. The API Gateway detects the failure
2. Returns a `503 Service Unavailable` response
3. Logs the error for monitoring
4. Provides clear error messages to users

### Implementation
- Custom `ServiceUnavailableException` for handling service failures
- Comprehensive logging using ILogger
- Automatic retry mechanisms for transient failures
- Circuit breaker pattern for preventing cascading failures

## Getting Started

1. Clone the repository
2. Set up PostgreSQL database
3. Configure services in `appsettings.json`
4. Run the services:
   ```bash
   dotnet run --project services/ApiGateway/ApiGateway.csproj
   dotnet run --project services/FileStoringService/FileStoringService.csproj
   dotnet run --project services/FileAnalysisService/FileAnalysisService.csproj
   ```

## Testing
Run the test suite:
```bash
dotnet test
```

## Technologies Used
- .NET 8.0
- PostgreSQL
- Entity Framework Core
- Swagger/OpenAPI
- xUnit for testing
- Moq for mocking 