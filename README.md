# Finance CRUD App ONNX

A desktop finance management application built with C#, .NET, Avalonia UI, SQLite, ONNX Runtime, and dashboard analytics.

Repository: https://github.com/XiaodiDennis/finance-crud-app-onnx

## Overview

This project is a course-project desktop application focused on CRUD functionality, database persistence, role-based authorization, ONNX-assisted category suggestion, and dashboard visualization.

The application supports:

- Categories
- Merchants
- Accounts
- Transactions

It also includes:

- login and role-based access control
- ONNX category suggestion inside the transaction form
- dashboard statistics with charts
- seeded sample data for demonstration

## Main Features

### Authorization
- Login screen with username and password
- Two default users:
  - `admin / admin123`
  - `viewer / viewer123`
- Admin users can access management windows
- Viewer users can log in but cannot access CRUD management windows

### CRUD Modules
The application includes full Create, Read, Update, and Delete functionality for:

- Categories
- Merchants
- Accounts
- Transactions

### Database
- Local SQLite database
- Automatic database initialization on startup
- Automatic seeding of sample data
- Relational structure with foreign keys for transactions

### ONNX Integration
The project includes ONNX Runtime integration for category suggestion in the transaction form.

Current ONNX workflow:
- the user enters transaction data
- the user selects merchant, account, transaction type, and amount
- the ONNX model predicts a suggested category
- the application shows the suggestion and confidence-related output
- high-confidence suggestions can be auto-applied

### Dashboard Analytics
The project includes a dashboard window with visual statistics, including:

- summary cards
- expense distribution by category
- monthly net totals
- recent daily expense trend

## Technology Stack

- C#
- .NET 10
- Avalonia UI
- SQLite
- Microsoft.Data.Sqlite
- ONNX Runtime
- LiveChartsCore.SkiaSharpView.Avalonia

## Project Structure

```text
FinanceCrudApp/
├── App.axaml
├── App.axaml.cs
├── Program.cs
├── Data/
├── Helpers/
├── Models/
├── Repositories/
├── Onnx/
├── Views/
└── onnx_models/
