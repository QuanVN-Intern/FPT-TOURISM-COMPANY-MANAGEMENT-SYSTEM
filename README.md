# FPT Tourism Company Management System

A comprehensive desktop application designed to streamline the operations of a travel company. The system supports managing tours, customers, bookings, employees, and resource allocation in an efficient and centralized way.

---

## Features

### Authentication & Security

* Role-based login system for staff management
* Secure password handling and reset functionality
* Account management with role permissions (Admin, Staff, etc.)

### Tour Management

* Create, update, and manage tour itineraries
* Organize destinations and travel schedules
* Assign vehicles and drivers to specific tours

### Customer & Booking

* Store and manage customer information
* Handle tour bookings efficiently
* Track payment status and booking history

### Business Intelligence

* Dashboard for statistics and KPIs
* Reports on revenue, bookings, and tour performance

---

## Tech Stack

* Framework: .NET (WPF - Windows Presentation Foundation)
* Language: C#
* Architecture: Layered Architecture (Models, DAL, BLL, UI)
* Data Access: ADO.NET
* Database: SQL Server
* Configuration: JSON (appsettings.json)

---

## Project Structure

```text
├── BLL/             # Business Logic Layer
├── DAL/             # Data Access Layer
├── Models/          # Entities & Data Models
├── Views/           # WPF UI (XAML)
├── App.xaml         # Entry Point
└── appsettings.json # Configuration (Connection String)
```

---

## Setup & Installation

### 1. Database Setup

* Run the SQL script:
  TravelCompanyDB (1).sql
* This will create database schema and initial data

### 2. Configuration

* Open appsettings.json
* Update connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnectionString": "your_sql_server_here"
  }
}
```

### 3. Run Project

* Open .sln file in Visual Studio
* Restore NuGet packages
* Press F5 to build and run

---

## Instructor

Vu Van Huy

---

## Project Members of group 21

* HE181274 – Tran Nguyen Anh Hao
* HE182357 – Vu Ngoc Quan
* HE186656 – Nguyen Son Nam

---

## Notes

* Developed as part of PRN212 Course – FPT University
* Desktop-based system focusing on real-world business workflow
* Designed using OOP principles and layered architecture
