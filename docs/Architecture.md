# FastPin - Technical Architecture

## Overview

FastPin is a WPF desktop application built with the MVVM pattern, using SQLite for data persistence and Entity Framework Core for ORM.

## Technology Stack

- **Framework**: WPF (Windows Presentation Foundation)
- **Platform**: .NET 10.0
- **UI Pattern**: MVVM (Model-View-ViewModel)
- **Database**: SQLite
- **ORM**: Entity Framework Core 9.0
- **Approach**: Model First with Code First migrations

## Project Structure

```
FastPin/
├── src/FastPin/
│   ├── Models/           # Data models (Entity classes)
│   ├── ViewModels/       # View models (MVVM)
│   ├── Views/            # (MainWindow is in root for now)
│   ├── Data/             # DbContext and database configuration
│   ├── Services/         # Business logic services
│   ├── Commands/         # ICommand implementations
│   ├── Converters/       # Value converters for XAML bindings
│   ├── App.xaml          # Application resources
│   └── MainWindow.xaml   # Main UI
├── docs/                 # Documentation
└── README.md
```

## Architecture Layers

### 1. Data Layer (Models + Data)

#### Models
- **PinnedItem**: Core entity representing pinned content
  - Properties: Id, Type, TextContent, ImageData, FilePath, CachedFileData, FileName, IsCached, CreatedDate, ModifiedDate
  - Supports: Text, Image, and File types
  
- **Tag**: Represents organizational tags
  - Properties: Id, Name, Color
  
- **ItemTag**: Junction table for many-to-many relationship
  - Properties: Id, PinnedItemId, TagId

#### Data Access
- **FastPinDbContext**: EF Core DbContext
  - Connection: SQLite database at `%APPDATA%\FastPin\fastpin.db`
  - Relationships: Configured with Fluent API
  - Auto-creation: Database.EnsureCreated()

### 2. Business Logic Layer (Services)

- **ClipboardMonitorService**
  - Monitors Windows clipboard using Win32 API
  - Raises events on clipboard changes
  - Uses `AddClipboardFormatListener` Win32 API

### 3. Presentation Layer (ViewModels)

#### ViewModelBase
- Base class for all ViewModels
- Implements `INotifyPropertyChanged`
- Provides `OnPropertyChanged` and `SetProperty` helpers

#### MainViewModel
- Main application ViewModel
- Manages collection of pinned items
- Handles:
  - Clipboard monitoring
  - Item creation (text, image, file)
  - Item deletion
  - Tag management
  - Search/filtering
  - Grouping logic
  
#### PinnedItemViewModel
- Wrapper for PinnedItem model
- Provides UI-friendly properties
- Converts image data to BitmapImage
- Manages tag collection

#### ItemGroup
- Groups items by date
- Used for date-based grouping display

### 4. UI Layer (Views)

#### MainWindow.xaml
- Main application window
- Features:
  - Header with branding
  - Toolbar with search and action buttons
  - Content area with grouped/ungrouped views
  - Status bar

#### XAML Resources
- Modern color scheme (Fluent Design)
- Custom button styles
- Value converters for conditional rendering

### 5. Supporting Components

#### Commands (Commands/)
- **RelayCommand**: Generic ICommand implementation
  - Supports both parameterized and parameterless actions
  - Integrates with CommandManager for CanExecute updates

#### Converters (Converters/)
- **BooleanToVisibilityConverter**: Boolean to Visibility
- **InverseBooleanToVisibilityConverter**: Inverted boolean to Visibility
- **ItemTypeToVisibilityConverter**: ItemType enum to Visibility

## Data Flow

### Pinning an Item

```
Clipboard Change
    ↓
ClipboardMonitorService (Event)
    ↓
MainViewModel.OnClipboardChanged()
    ↓
Determine Type (Text/Image/File)
    ↓
Create PinnedItem Entity
    ↓
DbContext.Add() + SaveChanges()
    ↓
Create PinnedItemViewModel
    ↓
Add to Items Collection
    ↓
UI Updates (via INotifyPropertyChanged)
```

### Grouping Items

```
LoadItems() called
    ↓
Query DbContext with filters
    ↓
If GroupByDate == true:
    Group by date (GetDateGroup)
    Create ItemGroup objects
    Populate GroupedItems collection
Else:
    Populate Items collection
    ↓
UI binds to appropriate collection
```

## Database Schema

### PinnedItems Table
- Id (INTEGER, PK)
- Type (INTEGER) - Enum: 0=Text, 1=Image, 2=File
- TextContent (TEXT, nullable)
- ImageData (BLOB, nullable)
- FilePath (TEXT, nullable)
- CachedFileData (BLOB, nullable)
- FileName (TEXT, nullable)
- IsCached (INTEGER/BOOL)
- CreatedDate (TEXT/DATETIME)
- ModifiedDate (TEXT/DATETIME)

### Tags Table
- Id (INTEGER, PK)
- Name (TEXT, unique)
- Color (TEXT, nullable)

### ItemTags Table
- Id (INTEGER, PK)
- PinnedItemId (INTEGER, FK)
- TagId (INTEGER, FK)
- Unique constraint on (PinnedItemId, TagId)

## Key Design Decisions

### 1. Model First Approach
- Models defined as C# classes
- EF Core generates database schema
- Migrations can be added as needed

### 2. SQLite Choice
- Lightweight, no server required
- File-based, easy backup
- Full-featured SQL database
- Cross-platform compatible

### 3. MVVM Pattern
- Clean separation of concerns
- Testable business logic
- Reusable ViewModels
- Data binding for reactive UI

### 4. File Handling Strategy
- **Link Mode**: Minimal storage, path dependency
- **Cache Mode**: Self-contained, higher storage
- User choice per item
- Default to Link mode for efficiency

### 5. Date Grouping
- Smart grouping (Today, Yesterday, etc.)
- Improves organization for large collections
- Optional (can toggle to flat view)

## Performance Considerations

1. **Database Queries**
   - Use Include() for eager loading of related entities
   - Filter in database with Where() before ToList()
   
2. **Image Handling**
   - Store as byte arrays in database
   - Convert to BitmapImage with caching
   - Freeze bitmaps for thread safety

3. **Clipboard Monitoring**
   - Event-driven, not polling
   - Minimal CPU usage
   - Win32 API integration

4. **UI Updates**
   - ObservableCollection for automatic UI refresh
   - INotifyPropertyChanged for property updates
   - Minimal full refreshes

## Security Considerations

- Database stored in user's AppData (protected by Windows ACLs)
- No network communication
- No sensitive data encryption (should be added if needed)
- Clipboard data handled in-process only

## Extensibility

The architecture supports future enhancements:
- Additional item types (e.g., links, code snippets)
- Cloud sync (add sync service layer)
- Export/Import (add serialization logic)
- Plugins (add plugin infrastructure)
- Multiple windows (already MVVM-ready)

## Testing Strategy

Future testing can include:
- Unit tests for ViewModels (test business logic)
- Integration tests for Data layer (test EF queries)
- UI tests with WPF UI automation
- Mock ClipboardService for automated tests
