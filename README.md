# Document Generator Solution

A full-stack document generation system with dynamic templates, PDF generation, and comprehensive authentication.

## Technology Stack

### Backend
- .NET 9
- ASP.NET Core Web API
- Entity Framework Core (Code First)
- PostgreSQL
- JWT Authentication
- Clean Architecture
- Repository Pattern
- Unit of Work Pattern
- FluentValidation
- AutoMapper
- Serilog
- Swagger
- Playwright for PDF Generation

### Frontend
- Angular 20
- Standalone Components
- Angular Material
- Reactive Forms
- Signals
- Route Guards
- JWT Authentication
- Dynamic Forms

## Solution Structure

```
DocumentGenerator.sln
├── backend/
│   ├── DocumentGenerator.API
│   ├── DocumentGenerator.Business
│   ├── DocumentGenerator.DAL
│   └── DocumentGenerator.Contracts
└── frontend/
    └── document-generator-ui
```

## Features

- **Authentication System**: Custom JWT-based authentication with roles and claims
- **Template Management**: Create and manage HTML templates with placeholders
- **Dynamic Forms**: Automatically generated forms based on template fields
- **Lookup Management**: Reusable dropdown lists
- **PDF Generation**: High-quality A4 PDFs with Playwright
- **Image Management**: Upload and embed images in templates
- **Document History**: Track all generated documents

## Getting Started

### Prerequisites
- .NET 9 SDK
- Node.js 20+
- PostgreSQL
- Angular CLI

### Backend Setup

1. Navigate to backend directory:
```bash
cd backend
```

2. Update connection string in `DocumentGenerator.API/appsettings.json`

3. Run migrations:
```bash
cd DocumentGenerator.API
dotnet ef database update
```

4. Run the API:
```bash
dotnet run
```

The API will be available at `https://localhost:7001`

### Frontend Setup

1. Navigate to frontend directory:
```bash
cd frontend/document-generator-ui
```

2. Install dependencies:
```bash
npm install
```

3. Run the application:
```bash
npm start
```

The application will be available at `http://localhost:4200`

## Default Credentials

- **Email**: admin@local.com
- **Password**: Admin@123

## API Documentation

Swagger UI is available at: `https://localhost:7001/swagger`

## License

MIT
