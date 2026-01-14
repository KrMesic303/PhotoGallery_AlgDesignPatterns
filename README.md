# Photo Gallery Application - Design Patterns and SOLID Principles

## Overview
### Using Layered and Clean Architecture:
    - Presentation Layer (Web)
        - ASP.NET Core MVC, Razor Views
        - Controllers, ViewModels, Views
    - Application Layer (Orchestration, defining use cases, depending on Domain)
        - Abstractions (Interfaces)
        - DTOs
        - Query / Policy Services
    - Domain Layer (Core business logic)
        - Entities
        - Value Objects
        - Business Rules
    - Infrastructure Layer (Implementats techical details, bridging app with external systems)
        - EF Core DbContext
        - Storage (Local / Cloud-ready)
        - Logging
        - Image Processing

## Patterns

### 1. Strategy Pattern
Encapsulate interchangeable behaviors behind a common interface.

#### 1.1    - IPhotoStorageService
    - Implementations LocalPhotoStorageService and GoogleCloudPhotoStorageService
- Storage backend can change without modifying controllers
- Open/Closed principle
- Cloud/Local switching via DI (appsettings.json)

#### 1.2    - IPhotoUploadPolicy
- Upload rules vary by package plan
- Rules evolve independently of controllers

#### 1.3     - IAuditLogger
- Logging destination can change
- Controllers remain unaware of logging mechanics

### 2. Abstract Factory Pattern
Create families of related objects without specifying concrete classes.

#### 2.1     - IImageProcessorFactory
- Producing multiple Image processors
- Used by ImageProcessingPipeline
    - Multiple processors must be created consistently
    - Upload and download use the same factory
    - Easy to add new filters without changing pipeline code

### 3. Command Pattern
Encapsulate an action as an object (or service).
- Used in controllers to delegate work to Upload policy  service, storage service, image processing pipeline, audit logger.
    - Controllers orchestrate, they do not execute logic
    - Promotes Single Responsibility Principle
    - Makes logic testable and reusable

### 4. Specification Pattern
Encapsulate query rules as composable conditions.
#### 4.1     - PhotoQueryService
Examples:
    - Search by author
    - Search by hashtag
    - Search by size
    - Search by date
    - Search by filters
    - Quick search across all metadata
Why Specification
    - Queries remain readable
    - Conditions are applied incrementally
    - Avoids massive conditional queries

### 5. Factory (Used in Dependency Injection)
Create objects without exposing instantiation logic.
Why:
    - Infrastructure swapping
    - Environment-specific configuration
    - Clean startup composition

### 6. Template Method
Define an algorithm structure while allowing steps to vary.
#### 6.1     - ImageProcessingPipeline.Execute()
Steps:
    1. Load image
    2. Apply processors in order
    3. Save result
Why Template Method
    - Pipeline structure is fixed
    - Processors vary independently

### 7. Decorator - konceptualno
Attach additional responsibilities dynamically.
#### 7.1     - ImageProcessingPipeline + multiple processors
**Each processor:**

    - Modifies the image
    - Passes it forward

**Why Decorator:**

    - Filters stack without inheritance explosion
    - Order matters
    - Behavior composed at runtime

### 8. Repository-Like abstraction
#### 8.1     -IPhotoQueryService
     - Used in EF (as each DbSet is repository)
Although not pure Repository pattern, it:
    - Separates queries from controllers
    - Centralizes read logic

### 9. Presentation pattern: MVC
---
## Design Patterns Table:

| Layer         | Pattern                               |
| ------------- | ------------------------------------- |
| Business      | Strategy, Command                     |
| Data/Service  | Abstract Factory, Specification       |
| Presentation  | Template Method (pipeline), Decorator |
| Cross-cutting | Factory (DI), Strategy                |

## SOLID principles:
- S - LocalPhotoStorageService, AuditLogger...
- O - IStorageService, IAuditLogger...
- L - PhotosController - we can swap IStorageService
- I - IPhotoQueryService - Gallery controller depends only on querying not uploading/deleting (its using only queries here to fetch data)
- D - Used in DI containers from project (Program.cs - builder, ServiceProvider in behind...), 
    - Controllers depend on abstractions rather than on concrete implementations of services
