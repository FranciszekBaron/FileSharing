# FileSharingServer

A .NET backend application responsible for exposing REST API endpoints and communicating with a database. Application allows to control user files and share them between other users. 
The application uses configuration based on `appsettings.json` and environment variables.


## Technologies
- .NET 9 SDK
- ASP.NET Core Web API
- Entity Framework Core
- Postgresql Server
- Docker

## Configuration 
The application requires the following configuration values:
Database connection string
Application secrets (e.g. JWT secret key)

Sensitive configuration values are not committed to the repository.
A sample configuration file is provided:

## File Management API Endpoints
All endpoints require authentication (JWT Bearer token).
## 📄 Files & Folders
| Method | Endpoint | Description |
|------|--------|------------|
| GET | `/api/fileitem` | Retrieve all files and folders belonging to the authenticated user |
| GET | `/api/fileitem/{fileId}` | Get details of a specific file or folder |
| POST | `/api/fileitem/folder` | Create a new folder |
| POST | `/api/fileitem/uploadFile` | Upload a new file |
| PATCH | `/api/fileitem/{fileId}/rename` | Rename a file or folder |
| PATCH | `/api/fileitem/{fileId}/toggleStarred` | Mark or unmark a file as starred |
