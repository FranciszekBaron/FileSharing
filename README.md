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
### Files & Folders
| Method | Endpoint | Description |
|------|--------|------------|
| GET | `/api/fileitem` | Retrieve all files and folders belonging to the authenticated user |
| GET | `/api/fileitem/{fileId}` | Get details of a specific file or folder |
| POST | `/api/fileitem/folder` | Create a new folder |
| POST | `/api/fileitem/uploadFile` | Upload a new file |
| PATCH | `/api/fileitem/{fileId}/rename` | Rename a file or folder |
| PATCH | `/api/fileitem/{fileId}/toggleStarred` | Mark or unmark a file as starred |
### File Download
| Method | Endpoint | Description |
|------|--------|------------|
| GET |	`/api/fileitem/{fileId}/downloadFile`| Download a file

### Sharing & Permissions
| Method | Endpoint                                 | Description                                           |
| ------ | ---------------------------------------- | ----------------------------------------------------- |
| POST   | `/api/fileitem/{fileId}/share`           | Share a file with another user                        |
| GET    | `/api/fileitem/{fileId}/usersWithAccess` | Get list of users who have access to a file           |
| GET    | `/api/fileitem/allSharedUser`            | Get all users with whom the current user shares files |

### Delete & Restore
| Method | Endpoint                                 | Description                                |
| ------ | ---------------------------------------- | ------------------------------------------ |
| DELETE | `/api/fileitem/{fileId}/softDelete`      | Soft-delete a file (can be restored later) |
| PATCH  | `/api/fileitem/{fileId}/restore`         | Restore a previously soft-deleted file     |
| DELETE | `/api/fileitem/{fileId}/permanentDelete` | Permanently delete a file                  |


## API Documentation
Full API documentation is available via Swagger UI:
/swagger

## What I Learned


### Background Jobs & Dependency Injection
BackgroundService in ASP.NET Core to implement recurring tasks. Key things that I learned:
- Understanding how to create scoped services inside BackgroundService, which is a singleton, using IServiceScopeFactory.
- Managing long-running tasks gracefully: I implemented CancellationToken to allow background jobs to terminate cleanly when the application shuts down.
- Ensuring proper resource disposal and dependency management in background jobs.

### Program.cs & Application Pipeline
I gained deeper insight into ASP.NET Core application startup:
- How the pipeline is configured: middleware ordering, request handling, and response flow.
- How dependency injection scopes are set up and used across controllers and services.
- How to structure Program.cs cleanly to separate configuration, services, and middleware registration.
- Best practices for registering services (singleton, scoped, transient) depending on their lifetime and usage.
- Understanding the flow from HTTP request → middleware → controller → service → database, which made the architecture much clearer.


## Technical Decisions & Challenges

### Secure Client Storage & Authentication
In this project, I improved how user data is stored on the client side.
Originally, user information was stored in `localStorage`, which is vulnerable to XSS attacks. I changed it to **secure HTTP-only cookies**, set by the backend, ensuring that sensitive data is not accessible from JavaScript.

Key improvements include:
- All user data is now retrieved only through the `/me` endpoint, instead of relying on client-side storage.
- Cookies are configured with security flags (`HttpOnly`, `Secure`, `SameSite`) to mitigate XSS and CSRF vulnerabilities.
- This approach enforces better session management and reduces the attack surface for authentication-related exploits.

### Permanent File Deletion & Nested Files
One of the main challenges was implementing **permanent deletion entirely on the backend**, including nested files inside folders, while keeping the system consistent and reliable.

Permanent deletion is implemented in **two separate methods**:
- **Endpoint-triggered deletion**: validates user permissions and provides immediate feedback to the client.  
- **Background job deletion**: processes files asynchronously, recursively removes folder contents, and deletes physical files from storage, handling errors gracefully.

This separation ensures that:
- User-initiated deletions are safe, predictable, and fail fast if something is wrong.
- Automated cleanup can operate efficiently on multiple files and nested folders without breaking the system or leaving orphaned data.





