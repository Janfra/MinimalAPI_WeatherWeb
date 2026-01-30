# WeatherWeb API: High-Performance C# Backend

**WeatherWeb** is a robust, production-ready RESTful API built with \*\*.NET 9\*\*. This project serves as a technical demonstration of how architectural principles from AAA game development—such as system decoupling, performance optimization, and rigorous validation—translate into enterprise-grade web services.

## 🎯 Technical Highlights

This project demonstrates a "Clean Architecture" approach to web development, focusing on maintainability and the **Uniform Interface** constraint of REST:

* **Layered Separation:** Decoupled the Database (EF Core), the Business Logic (Services), and the API Surface (Mappings).

* **Data Integrity:** Implemented **FluentValidation** to enforce strict guard clauses on incoming data, preventing corrupted state in the persistence layer.

* **Performance-First IO:** Utilized `async/await` and `IQueryable` to ensure database queries are executed efficiently on the SQL engine, minimizing memory overhead.

* **Fault Tolerance:** Integrated a **Global Exception Handler** using the `IExceptionHandler` interface to standardize error responses (RFC 7231) and ensure system stability.

## 🛠️ Tech Stack

* **Runtime:** .NET 9 (CoreCLR)

* **ORM:** Entity Framework Core (SQLite)

* **Validation:** FluentValidation

* **API Pattern:** Minimal APIs (high-performance routing)

* **Standardization:** OpenAPI / Problem Details for standard error reporting

---

## 📂 Project Organization

The folder structure follows industry-standard conventions to ensure a clear "blueprint" of the application:

```text

WeatherWeb/

├── src/

│   └── WeatherWeb.API/

│       ├── Data/            # Persistence Layer (DbContext \& Migrations)

│       ├── Models/          # Domain Entities and Data Transfer Objects (DTOs)

│       ├── Services/        # Business Logic \& Data Processing

│       ├── Mappings/        # API Route Definitions \& Request Handling

│       ├── Middleware/      # Global Safety Nets (Exception Handling)

│       └── Validators/      # Declarative Validation Rules

└── tests/                   # Automated Testing Suite (xUnit \& Moq)

```

---

## 🚦 Getting Started

### Prerequisites

* .NET 9 SDK

* `dotnet-ef` tool (`dotnet tool install --global dotnet-ef`)

### Local Setup

1. **Initialize Database:**

```bash

dotnet ef database update

```

2. **Run Application:**

```bash

dotnet run --project src/WeatherWeb.API

```

---

## 🧪 Quick Verification (REST Client)

For rapid testing without external tools, this project includes a `WeatherWeb.http` file compatible with the **Visual Studio / VS Code REST Client**.

1. **Trust \& Verify the Local Certificate**: Ensure your machine trusts the .NET development certificate so the firewall does not block local HTTPS traffic.

```Bash

# Step A: Apply trust
dotnet dev-certs https --trust

# Step B: Verify trust status
dotnet dev-certs https --check --trust
```

2. **Run the API**: Start the project using `dotnet run --project src/WeatherWeb.API`.

3. **The "First Run" Sequence**: Since the database starts empty, you must execute the `POST: Create a New Report` request first to populate the SQLite database.

4. **Explore Edge Cases**: The `.http` file includes dedicated "Failure" cases. These demonstrate the **FluentValidation** logic and **Global Exception Handling**, showing how the API returns standardized `ProblemDetails` when:

    * Temperatures are out of physical bounds (e.g., 150°C).

    * Required fields like `Location` are missing.

    * The client requests a resource that does not exist (404).

---

## 📡 API Capabilities

The API provides a full suite of CRUD operations, leveraging DTOs to prevent over-posting and protect the internal data model:

| Method | Endpoint | Purpose |
| :--- | :--- | :--- |
| **GET** | `/weather/hot` | Logic-driven filtering for temperatures $\ge 30°C$. |
| **GET** | `/weather/filter` | Flexible query-string filtering (e.g., `?minHumidity=90.0`). |
| **GET** | `/weather/{location}` | Route-parameter based lookup for specific regional data. |
| **POST** | `/weather/reports` | Resource creation utilizing **FluentValidation** and **DTOs**. |
| **PUT** | `/weather/reports/{id}` | Idempotent updates to existing weather records by unique ID. |
| **DELETE** | `/weather/reports/{id}` | Secure removal of records from the SQLite persistence layer. |

---

## 🧠 From Game Dev to Enterprise C#

Coming from a background in **Game Development (C++/C#)**, I approach web services with a unique perspective on system design:

* **Stateless vs. State-based:** While games manage complex frame-by-frame state, I've designed this API to be entirely stateless, ensuring it can scale horizontally in a cloud environment.

* **System Decoupling:** Just as one would decouple a Physics System from a Rendering System, I have isolated the `WeatherReporter` logic from the `WeatherDbContext`, making the code testable and modular.

* **Resource Management:** My experience with memory-constrained environments leads me to prioritize efficient data transfer and non-blocking asynchronous operations.


