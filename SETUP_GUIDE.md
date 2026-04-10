# IAMS Setup & Login Guide

## Quick Start

### 1. Database Setup
```bash
cd AssetManagement.Server

# Restore packages
dotnet restore

# Apply migrations
dotnet ef database update --project ../AssetManagement.Infrastructure
```

### 2. Seed Data
Chạy lệnh để tạo dữ liệu test:

```bash
# Chờ backend khởi động xong, sau đó chạy command này trong terminal mới:
curl -X POST http://localhost:5006/api/dev/seed-data
```

hoặc vào `http://localhost:5006/swagger` click POST `/api/dev/seed-data`

### 3. Run Application
```bash
# Terminal 1: Backend
cd AssetManagement.Server
dotnet run

# Backend khởi động tại http://localhost:5006
# Frontend tự động khởi động tại http://localhost:5174
```

---

## Login Credentials

Sau khi seed data, dùng các account sau để test:

### Admin Account
- **Email**: admin@example.com
- **Password**: admin123
- **Role**: ADMIN

### Manager Account
- **Email**: manager@example.com
- **Password**: manager123
- **Role**: MANAGER

### Employee Account
- **Email**: employee@example.com
- **Password**: emp123
- **Role**: EMPLOYEE

### IT Admin Account
- **Email**: itadmin@example.com
- **Password**: itadmin123
- **Role**: ITAdmin (dùng để tạo user mới)

---

## Features by Role

### ADMIN
- Xem tất cả assets
- Xem dashboard (KPI, charts, reports)
- Quản lý users & departments

### MANAGER
- Xem assets
- Điều phối assignments
- Duyệt asset requests từ nhân viên
- Xem reports theo department

### EMPLOYEE
- Yêu cầu assets
- Xem assets được gán cho mình
- Báo cáo hỏng hóc

### ITAdmin
- Tạo user mới
- Quản lý roles

---

## API Endpoints

### Auth
- `POST /api/v1/auth` - Login
- `POST /api/v1/auth/create` - Create user (Require ITAdmin role)

### Assets
- `GET /api/assets` - Get all assets (có pagination)
- `GET /api/assets/{id}` - Get asset detail
- `POST /api/assets` - Create asset
- `PUT /api/assets/{id}` - Update asset
- `DELETE /api/assets/{id}` - Delete asset (soft delete)

### Dashboard
- `GET /api/dashboard/kpi` - KPI metrics
- `GET /api/dashboard/asset-status` - Asset status summary
- `GET /api/dashboard/repair-spending` - Repair spending by month
- `GET /api/dashboard/activity-trend` - Asset activity trend
- `GET /api/dashboard/assets-by-department` - Assets grouped by department
- `GET /api/dashboard/top-assets` - Top used assets

### Dev (Development only)
- `POST /api/dev/seed-data` - Insert seed data

---

## Database Connection
```
Host: localhost
Port: 5432
Database: asset
Username: postgres
Password: 123
```

---

## Troubleshooting

### Frontend không kết nối tới backend
- Check xem backend chạy tại `http://localhost:5006`
- Check browser DevTools -> Network tab
- Xem proxy config trong `vite.config.js`

### Database connection error
- Chắc chắn PostgreSQL đang chạy
- Check connection string trong `appsettings.json`

### Migration error
- Xóa migration files không cần thiết
- Chạy lại: `dotnet ef database update`

---
