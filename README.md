# WeatherWeb API: C# Backend

**WeatherWeb** is a RESTful API built with **.NET 9**. This project was created to learn about how web development backend functions. Explores Minimal APIs, and how system decoupling, performance optimisation, and validation can be applied in web services.

## 🎯 Technical Highlights

* **Layered Separation:** Decoupled the Database (EF Core), the Business Logic (Services), and the API Surface (Mappings).

* **Data Integrity:** Implemented **FluentValidation** to enforce strict guard clauses on incoming data, preventing corrupted state in the persistence layer.

* **Performance-First IO:** Utilised `async/await` and `IQueryable` to ensure database queries are executed efficiently on the SQL engine, minimising memory overhead.

* **Error Handling:** Integrated a **Global Exception Handler** using the `IExceptionHandler` interface to standardise error responses.

---

## 🛠️ Tech Stack

* **Runtime:** .NET 9
  
* **ORM:** Entity Framework Core (SQLite)

* **Validation:** FluentValidation

* **API Pattern:** Minimal APIs

* **Error Handling:** Problem Details via `IExceptionHandler`

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

For manual testing, this project includes a `WeatherWeb.http` file compatible with the **Visual Studio / VS Code REST Client**.

1. **Trust & Verify the Local Certificate**: Ensure your machine trusts the .NET development certificate so the firewall does not block local HTTPS traffic.

```Bash

# Step A: Apply trust
dotnet dev-certs https --trust

# Step B: Verify trust status
dotnet dev-certs https --check --trust
```

2. **Run the API**: Start the project using `dotnet run --project src/WeatherWeb.API`.

3. **The "First Run" Sequence**: Since the database starts empty, you must execute the `POST: Create a New Report` request first to populate the SQLite database with initial data.

4. **Explore Edge Cases**: The `.http` file includes dedicated "Failure" cases. These demonstrate the **FluentValidation** logic and **Global Exception Handling**, showing how the API returns standardised `ProblemDetails` when:
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

## 📂 Project Organization

```text
WeatherWeb/

├── src/

│   └── WeatherWeb.API/

│       ├── Data/            # Persistence Layer (DbContext & Migrations)

│       ├── Models/          # Domain Entities and Data Transfer Objects (DTOs)

│       ├── Services/        # Business Logic (Reporters & Formatters)

│       ├── Mappings/        # API Route Definitions & Request Handling

│       ├── Middleware/      # Global Exception Handling

│       └── Validators/      # FluentValidation Rules

└── tests/                   # Automated Testing Suite (xUnit & Moq)
```

---

