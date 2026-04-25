# Task Management API

A RESTful API for managing tasks with file attachment support, built with .NET 8 and MSSQL.

---

## Tech Stack

- **Runtime**: .NET 8
- **Database**: Microsoft SQL Server
- **Containerization**: Docker + Docker Compose
- **ORM**: Entity Framework Core 8
- **Documentation**: Swagger / OpenAPI
- **Architecture**: Layered (MODEL → REPOSITORY → BAL → API)

---

## Project Structure

```
Task Management.sln
├── API/          - ASP.NET Core Web API (controllers, startup)
├── BAL/          - Business logic layer (services)
├── MODEL/        - Entities, DTOs, DataContext, ResponseModel
└── REPOSITORY/   - Generic repository, Unit of Work
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Microsoft SQL Server (any edition, including Express)
- SQL Server Management Studio (optional)

---

## Database Setup

### Option A — Run the schema script

1. Open SQL Server Management Studio (or `sqlcmd`)
2. Run the script at `database/schema.sql`

```bash
sqlcmd -S . -U sa -P YourPassword -i "database/schema.sql"
```

This creates the `TaskManagementDB` database and all tables:
- `TaskHeaders`
- `TaskDetails`
- `FileAttachments`
- `SystemLogs`

### Option B — EnsureCreated

`Program.cs` calls `db.Database.EnsureCreated()` on startup. If the database does not exist, EF will create it from the entity models automatically.

---

## Configuration

Edit `API/appsettings.json` to match your SQL Server credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=TaskManagementDB;User ID=sa;Password=YourPassword;TrustServerCertificate=True;"
  },
  "FileStorage": {
    "UploadPath": "Uploads"
  }
}
```

> Uploaded files are stored in the `Uploads/` folder relative to the API project. The folder is created automatically on first upload.

---

## Running the Application

### Option A — Local 

Requires .NET 8 SDK and a running SQL Server instance.

```bash
cd "API"
dotnet run
```

The API will be available at:
- HTTP:  `http://localhost:5123`
- HTTPS: `https://localhost:7004`

Swagger UI opens automatically at `/swagger`.

### Option B — Docker

Requires [Docker Desktop](https://www.docker.com/products/docker-desktop/).  
No SQL Server installation needed — everything runs in containers.

```bash
docker-compose up --build
```

This starts two containers:
- **`taskmanagement-db`** — SQL Server 2022 Express on port `1433`
- **`taskmanagement-api`** — The API on port `8080`

The API waits for the database to be healthy before starting.

Swagger UI: `http://localhost:8080/swagger`

To stop:
```bash
docker-compose down
```

To stop and remove all data volumes:
```bash
docker-compose down -v
```

---

## API Reference

### Task Endpoints — `/api/tasks`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/tasks` | Get all active tasks |
| GET | `/api/tasks/{id}` | Get task by ID (with details and attachments) |
| POST | `/api/tasks` | Create a new task |
| PUT | `/api/tasks/{id}` | Update a task |
| DELETE | `/api/tasks/{id}` | Soft-delete a task |
| POST | `/api/tasks/{id}/details` | Add a detail item to a task |
| PUT | `/api/tasks/details/{detailId}` | Update a detail item |
| DELETE | `/api/tasks/details/{detailId}` | Soft-delete a detail item |

### File Endpoints — `/files`

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/files/upload` | Upload a file (optionally linked to a task) |
| GET | `/files/download/{id}` | Download a file by ID |
| DELETE | `/files/{id}` | Soft-delete a file record |

---

## Sample Requests

### Create a Task

```http
POST /api/tasks
Content-Type: application/json

{
  "taskCode": "TASK-001",
  "title": "Design login page",
  "description": "Create wireframes and implement the login UI",
  "priority": "High",
  "status": "Pending",
  "dueDate": "2026-05-01T00:00:00Z",
  "assignedTo": "john.doe",
  "createdBy": "manager",
  "taskDetails": [
    {
      "itemTitle": "Create wireframe",
      "itemDescription": "Use Figma",
      "remark": ""
    },
    {
      "itemTitle": "Implement HTML/CSS",
      "remark": "Mobile responsive required"
    }
  ]
}
```

**Response `201 Created`:**
```json
{
  "message": "Add Successfully!",
  "status": 0,
  "data": {
    "taskId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "taskCode": "TASK-001"
  }
}
```

### Get a Task

```http
GET /api/tasks/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Response `200 OK`** — returns full task with detail items and file attachments.

### Update a Task

```http
PUT /api/tasks/3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json

{
  "title": "Design login page (revised)",
  "description": "Updated scope",
  "priority": "Medium",
  "status": "InProgress",
  "dueDate": "2026-05-10T00:00:00Z",
  "assignedTo": "jane.doe"
}
```

### Delete a Task

```http
DELETE /api/tasks/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Response `200 OK`** — soft-deletes task and all its detail items and attachments.

### Upload a File

```http
POST /files/upload?taskId=3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: multipart/form-data

file: <binary>
```

- `taskId` is optional. Omit it to upload without linking to a task.
- Max file size: **10 MB**
- Allowed types: JPEG, PNG, GIF, WEBP, PDF, DOC, DOCX, XLS, XLSX, TXT

**Response `201 Created`:**
```json
{
  "message": "Add Successfully!",
  "status": 0,
  "data": {
    "fileId": "...",
    "originalFileName": "report.pdf",
    "contentType": "application/pdf",
    "fileSize": 204800,
    "uploadedAt": "2026-04-25T10:00:00Z"
  }
}
```

### Download a File

```http
GET /files/download/a1b2c3d4-0000-0000-0000-000000000000
```

Returns the file with correct `Content-Type` and `Content-Disposition` headers.

---

## Response Format

All endpoints return a consistent envelope:

```json
{
  "message": "string",
  "status": 0,
  "data": {}
}
```

| `status` value | Meaning | HTTP Code |
|---|---|---|
| `0` | Success | 200 / 201 |
| `1` | Validation / business error | 400 |
| `2` | Server error | 500 |
| `3` | Not found | 404 |

---

## Validation Rules

**Task Header:**
- `TaskCode` — required, max 20 chars, must be unique
- `Title` — required, max 200 chars
- `Priority` — must be `Low`, `Medium`, or `High`
- `Status` — must be `Pending`, `InProgress`, or `Done`
- `CreatedBy` — required, max 100 chars

**Task Detail:**
- `ItemTitle` — required, max 200 chars

**File Upload:**
- File must not be empty
- File name must be valid
- Max size: 10 MB
- Allowed content types only
