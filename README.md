# ICMS — Integrated Church Management System

> A full-stack church and district management platform built with **ASP.NET Core** (Clean Architecture) and **Angular 18**. Designed to manage members, teams, attendance, payments, transfers, and clearances across churches and districts.

---

## 🏗️ Tech Stack

| Layer | Technology |
|---|---|
| **Backend** | ASP.NET Core 10, Clean Architecture |
| **Frontend** | Angular 22, NgRx Signals, TailwindCSS |
| **Database** | SQL Server + Entity Framework Core |
| **Auth** | ASP.NET Identity + JWT + Refresh Tokens |
| **Image Storage** | Cloudinary |

---

## 📁 Project Structure

```
ICMS/
├── Icms.Api/                  → REST API (controllers, middleware, auth policies)
├── Icms.Application/          → Use cases, interfaces, DTOs, validators
├── Icms.Domain/               → Entities and enums
├── Icms.Infrastructure/       → EF Core, repositories, services, migrations
└── icms-web/                  → Angular 22 frontend
    └── src/app/features/
        ├── auth/              → Login
        ├── church/            → Church dashboard, members, teams, clearance
        ├── district/          → District dashboard, churches, employees, ministers
        ├── member/            → Member portal, profile, teams, attendance
        ├── landing/           → Public landing page
        └── shared/            → Shared components, layout, pipes, UI
```

---

## ✨ Features

### 👥 Member Management
- Register, view, edit, and manage church members
- Member profile with personal details
- Member transfer between churches with clearance workflow

### ⛪ Church Management
- Church dashboard with overview stats
- Manage departments, employees, accounts
- Team creation and management
- Team attendance tracking and payments

### 🏢 District Management
- District-level overview of all churches
- Manage district employees and ministers
- Church accounts and financial overview

### 🔐 Authentication & Authorization
- Role-based access (District Admin, Church Admin, Member)
- JWT authentication with refresh tokens
- Account lockout protection

### 📋 Clearance & Transfers
- Member transfer request workflow
- Clearance approval process between churches

---

## 🚀 Getting Started (Fresh Setup)

### Prerequisites

| Tool | Version |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0+ |
| [Node.js](https://nodejs.org/) | 18+ |
| [Angular CLI](https://angular.io/cli) | 22+ |
| SQL Server | LocalDB or full |

```bash
# Verify installations
dotnet --version
node --version
ng version
```

---

### Step 1 — Clone the repo

```bash
git clone https://github.com/ijoollee-biyyaa/ICMS.git
cd ICMS
```

---

### Step 2 — Configure environment files

These files are in `.gitignore` — you must create them from the examples:

**Backend:**
```bash
copy appsettings.Development.Example.json Icms.Api/appsettings.Development.json
```
Then open `appsettings.Development.json` and fill in:
- `ConnectionStrings:DefaultConnection` — your SQL Server connection string
- `Jwt:Secret` — any long random string (32+ chars)
- `Cloudinary` — your Cloudinary cloud name and upload preset

**Frontend:**
```bash
copy icms-web/src/environments/environment.development.example.ts icms-web/src/environments/environment.development.ts
```
Then open `environment.development.ts` and fill in:
- `apiBase` — your API URL (default: `http://localhost:5171`)
- `cloudinary.cloudName` — your Cloudinary cloud name
- `cloudinary.uploadPreset` — your Cloudinary upload preset

---

### Step 3 — Backend setup

```bash
# Restore packages
dotnet restore

# Apply all migrations to create the database
dotnet ef database update --project Icms.Infrastructure --startup-project Icms.Api

# Run the API (default: https://localhost:7171 / http://localhost:5171)
dotnet run --project Icms.Api
```

> API documentation available at: `http://localhost:5171/scalar`

---

### Step 4 — Frontend setup

```bash
cd icms-web

# Install dependencies
npm install

# Start the dev server (default: http://localhost:4200)
ng serve
```

---

## 🌐 Running Both Together

| Service | URL |
|---|---|
| Angular Frontend | `http://localhost:4200` |
| ASP.NET Core API | `http://localhost:5171` |
| API Docs (Scalar) | `http://localhost:5171/scalar` |

---

## 🔐 Default Credentials

After running migrations, seed data creates default accounts. Check `DataSeeder.cs` for default credentials.

> ⚠️ Change default passwords immediately in production!

---

## 📦 Key Packages

**Backend:**
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` — Identity
- `Microsoft.EntityFrameworkCore.SqlServer` — EF Core
- `FluentValidation` — Request validation
- `CloudinaryDotNet` — Image uploads
- `Scalar.AspNetCore` — API documentation

**Frontend:**
- `@ngrx/signals` — State management
- `TailwindCSS` — Styling
- `Angular Material` — UI components

---

## 🤝 Contributing

1. Fork the repo
2. Create a feature branch: `git switch -c feat/your-feature`
3. Commit your changes: `git commit -m "feat: add your feature"`
4. Push: `git push origin feat/your-feature`
5. Open a Pull Request

---

## 📄 License

This project is private and owned by [@ijoollee-biyyaa](https://github.com/ijoollee-biyyaa).
