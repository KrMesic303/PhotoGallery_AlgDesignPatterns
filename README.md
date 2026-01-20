# Photo Gallery Application - Design Patterns and SOLID Principles

## Overview
### Using Layered and Clean Architecture:
    - Presentation Layer (Web)
        - ASP.NET Core MVC, Razor Views
        - Controllers are HTTP adapters
        -ViewModels and Views
    - Application Layer (Orchestration, defining use cases, depending on Domain)
        - Use cases (CQRS: Commands/Queries + Handlers)
        - Abstractions (interfaces) for persistence, storage, policies, transforms, events
        - DTOs and result models
    - Domain Layer (Core business logic)
        - Entities and business concepts (Photo, Hashtag, PackagePlan...)
        - Domain state
    - Infrastructure Layer (Implementats techical details, bridging app with external systems)
        - EF Core DbContext and repository implementations
        - Storage (Local and Google Cloud Storage)
        - Image Processing (ImageSharp) + pipelines
        - Auditing + metrics, event publisher, query services

### Dependency Rule:
    - Web -> Aplpication -> Domain
    - Infrastructure implements Application abstractions and depends on Application/Domain
    - Domain has no dependency on Application or Interface

## Patterns (GoF - 8)

### 1. Strategy Pattern
Encapsulate interchangeable behavior behind a stable interface.
#### Where used:
    - IphotoStorageService
        - LocalPhotoStorageService
        - GcsPhotoStorageService
    - IIMageProcessor
        - ResizeImageProcessor, SepiaImageProcessor, BlurImageProcessor, FormatImageProcessor
    - Policy/services:
        - IPhotoUploadPolicy
        - IUploadQuotaService
Benefit: Behaviors evolve or swap without changing controllers/use cases.

### 2. Factory Method
Create objects through a method so creation logic stays centralized and extensible.
#### Where used:
    - IImageProcessorFactory / ImageProcessorFactory.Create(ImageProcessingOptionsDto options)
    - Builds a list of IImageProcessor based on options
Benefit: Adding new processors does not require changing the pipeline or use cases.

### 3. Chain of Responsibility
Pass a request through a chain of handlers where each performs work and forwards to the next.
#### Where used:
    - ImageProcessingPipeline
        - Executes a sequential chain of IImageProcessor transformations
Benefit: Image transformations are composable and order-dependent without large conditional logic.

### 4. Command
Encapsulate a request as an object, separating invocation from execution.
#### Where used:
    - Commands + Handlers (write-side)
        - UploadPhotoCommand -> IUploadPhotoHandler
        - EditPhotoMetadataCommand -> IEditPhotoMetadataHandler
        - DeletePhotoCommand -> IDeletePhotoHandler
        - ChangePackageCommand -> IChangePackageHandler
        - BulkDeletePhotosCommand -> IBulkDeletePhotosHandler
    - Query objects are also used for read flows where appropriate
        - DownloadPhotoQuery -> IDownloadPhotoHandler
        - GetEditPhotoQuery -> IGetEditPhotoHandler
        - GetPhotoFileQuery -> IGetPhotoFileHandler
Benefit: Controllers stay thin, logic is testable, reusable, and composable.

### 5. Decorator
Attach responsibilities dynamically by wrapping objects with the same interface.
#### Where used:
    - Audited handler wrappers (cross-cutting)
    - AuditedUploadPhotoHandler : IUploadPhotoHandler
    - AuditedEditPhotoMetadataHandler : IEditPhotoMetadataHandler
    - AuditedDeletePhotoHandler : IDeletePhotoHandler
    - AuditedDownloadPhotoHandler : IDownloadPhotoHandler
    - AuditedBulkDeletePhotosHandler : IBulkDeletePhotosHandler
    - AuditedAdminDeletePhotoHandler : IAdminDeletePhotoHandler
    - AuditedChangePackageHandler : IChangePackageHandler
Benefit: Auditing remains consistent without polluting core use-case handlers.

### 6. Template Method
Define an algorithm skeleton in a base class and allow subclasses to customize steps.
#### Where used:
    - ImageTransformTemplate (base workflow)
        - loads image -> applies pipeline -> encodes output -> optional thumbnail
    - StorageImageTransformTemplate (thumbnail enabled)
    - DownloadImageTransformTemplate (thumbnail disabled)
    - Used through IImageTransformService (ImageSharpTransformService delegates to templates)
Benefit: Upload/store and download transforms share a standardized workflow with controlled variation.

### 7. Observer
Define a one-to-many dependency so observers automatically react to events.
#### Where used:
    - Domain events + publisher + handlers
        - IEventPublisher implemented by InProcessEventPublisher
        - Events:
            - PhotoUploadedEvent
            - PhotoDownloadedEvent
            - PhotoDeletedEvent
            - PackageChangedEvent
        - Observers:
            - PhotoMetricEventHandler : IDomainEventHandler<TEvent>
Benefit: Secondary concerns (metrics) react to system activity without coupling to controllers.

### 8. Abstract Factory
Create families of related objects without specifying concrete classes.
#### Where used:
    - IStorageProviderFactory
        - LocalStorageProviderFactory
        - GcsStorageProviderFactory
    - Provider selection:
        - StorageProviderFactorySelector selects the concrete factory based on config
    - Product creation:
        - IPhotoStorageService resolved via IStorageProviderFactory.CreatePhotoStorageService()
Benefit: Adding a new storage provider becomes an additive change: new factory + new product(s), no rewiring in controllers.

---
### Additional non-GoF patterns:
    - Repository:
        - IPhotoRepository, IHashtagRepository
    - CQRS (native):
        - Write-side: commands + handlers
        - Read-side: query services/handlers (Admin/Profile/Gallery)
    - MVC (Presentation pattern):
        - Controllers, views, view models (ASP.NET Core MVC)

---

## SOLID principles:
S — Single Responsibility
    - Handlers own use-case orchestration (upload/edit/delete)
    - Storage services own persistence of binary objects
    - Query services/handlers own read models
O — Open/Closed
    - Add a new storage provider via new factory + new service
    - Add a new image processor without changing the pipeline
L — Liskov Substitution
    - Any IPhotoStorageService implementation can replace another without changing use cases
I — Interface Segregation
    - Read and write concerns are separated (query services vs command handlers)
D — Dependency Inversion
    - Web depends on Application abstractions (handlers/services)
    - Infrastructure implements abstractions, wired via DI in the composition root

---

### Configuration Notes:
Storage provider selection is set in configuration:
    - Storage:Provider = "Local" or "Gcs"
    - GCS Settings:
        - Storage:Gcs:BucketName
        - Storage:Gcs:UploadsPrefix (default: "uploads")

