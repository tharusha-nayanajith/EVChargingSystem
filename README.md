# ⚡ EV Charging System

A modular EV charging management system built with **.NET 9** and **MongoDB**, designed for scalability, clean architecture, and team-based development.

## 🚀 Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Quick Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/tharusha-nayanajith/EVChargingSystem.git
   cd EVChargingSystem
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Run the application**

4. **Verify setup**
   - API: [https://localhost:7261](https://localhost:7261)  
   - MongoDB status :https://localhost:7261/api/test/mongodb-status

---

## 🛠️ Development

### Adding New Features

Follow **Clean Architecture** structure:

1. **Domain Layer (Entities)**
   ```csharp
   // src/EVChargingSystem.Domain/Entities/YourEntity.cs
   public class YourEntity
   {
       public string Id { get; set; }
       public string Name { get; set; }
   }
   ```

2. **Infrastructure Layer (Repository)**
   ```csharp
   // src/EVChargingSystem.Infrastructure/Persistence/YourRepository.cs
   public class YourRepository : BaseRepository<YourEntity>
   {
       public YourRepository(IMongoDatabase database) 
           : base(database, "yourCollectionName") { }
   }
   ```

3. **Application Layer (Services)**
   ```csharp
   // src/EVChargingSystem.Application/Services/YourService.cs
   public class YourService : IYourService
   {
       private readonly YourRepository _repository;
       public YourService(YourRepository repository) => _repository = repository;
   }
   ```

4. **API Layer (Controllers)**
   ```csharp
   // src/EVChargingSystem.Api/Controllers/YourController.cs
   [ApiController]
   [Route("api/[controller]")]
   public class YourController : ControllerBase
   {
       private readonly IYourService _yourService;
       public YourController(IYourService yourService) => _yourService = yourService;
   }
   ```

---

## 📋 API Endpoints

| Method | Endpoint                          | Description              | Owner |
|--------|-----------------------------------|--------------------------|-------|
| GET    | `/api/test/mongodb-status`        | Check DB connection      | -     |
| ...    | More endpoints coming soon        | By feature owners        |       |


---

## 🤝 Development Workflow

1. **Create feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Commit changes**
   ```bash
   git add .
   git commit -m "Add feature description"
   ```

3. **Push & open PR**
   ```bash
   git push origin feature/your-feature-name
   ```
---
