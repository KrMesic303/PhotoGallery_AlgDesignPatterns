# PhotoGallery Application

# 0.0.11
**Added**
	- Decorator patter
	- Refactored handlers to use it

# 0.0.11
**Added**
	- Command pattern (CQRS)
	- Refactored Controllers

# 0.0.10
**Changed**
	- Controllers are independent of DbContext class, they use services

# 0.0.9
**Added**
	- Pagination support on Gallery main page (10 photos per page)
	- Paged result abstraction (PagedResult<T>)
	- Page navigation UI with responsive behavior (side navigation on large screens, bottom on small screens)
	- Google Cloud Storage implementation for photo and thumbnail storage
	- Support for external authentication providers:
		- Google Login
		- GitHub Login

**Changed**
	- Gallery index query refactored to support paging
	- Storage implementation switched via DI (Local - Cloud)
	- Navigation layout updated to support pagination UX

# 0.0.8
**Added**
	- Photo filters persistence (PhotoFilter entity)
	- Support for multiple image filters:
		- Resize
		- Format
		- Sepia
		- Blur
	- Filter metadata saved per photo
	- Advanced search extended to filter by applied image filters
	- Quick search across:
		- Author email
		- Description
		- Hashtags
		- Applied filters (type and value)

**Changed**
	- Upload pipeline extended to support multiple processors
	- Image processing implemented using Abstract Factory pattern
	- Search logic updated to include filter-based criteria
	- Query services refactored for extensibility and performance

# 0.0.7
**Added**
	- Full Admin area with role-based authorization
	- Admin tabs:
	- Users management
	- Photos management
	- Audit logs
	- Statistics
	- Advanced search
	- Bulk delete functionality for photos
	- Admin-only photo deletion
	- Admin photo edit access
	- Admin statistics dashboard

**Changed**
	- Search page restricted to administrators
	- UI actions (Edit/Delete) conditionally rendered based on role and ownership
	- Gallery search logic reused for Admin advanced search

# 0.0.6
**Added**
	- Download option for images

# 0.0.5
**Added**
	- Thumbnails and DTO models
	- Gallery page and controller

# 0.0.4
**Added**
	- Audit logging abstraction and implementation
	- Audit log model
	- Comments with Design Patterns on classes where they are used/shown

# 0.0.3
**Added**
	- Storage abstraction
	- Local Storage service implementation
	- Photo, Hastah, PhotoHastag models
	- Photos Upload page and controller
	- Upload Policy

# 0.0.2
**Added**
	- Identity db context and initial models
	- PackageModel
	- ApplicationUser

**Changed**
	- ApplicationUser extends IdentityUser
	- PackagePlan models part of Identity db context
	- Profile page extended
	- Navigation updated

# 0.0.1
**Added**
	- initial projects structure and nugets
	- initial model for packages plans