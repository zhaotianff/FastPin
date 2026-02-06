# FastPin

FastPin is a modern WPF application that allows you to quickly pin text, images, and files from your clipboard for easy access and organization.

## Features

### Core Functionality
- **Clipboard Monitoring**: Automatically detects clipboard changes and allows pinning of content
- **Multi-Type Support**: Pin text, images, and files
- **SQLite Database**: All data is persisted in a local SQLite database
- **MVVM Architecture**: Clean separation of concerns using the Model-View-ViewModel pattern

### Organization
- **Tagging**: Add custom tags to pinned items for better organization
- **Search**: Quickly find items by searching text content, file names, or tags
- **Grouping**: View items grouped by date (Today, Yesterday, This Week, This Month, etc.)
- **Flexible View**: Toggle between grouped and ungrouped views

### File Handling
- **Link Mode**: Store only the file path without copying the file
- **Cache Mode**: Save a copy of the file within the application's data structure
- **Toggle Option**: Switch between link and cache modes for each file item

## Technology Stack

- **Framework**: WPF (.NET 8.0)
- **Architecture**: MVVM (Model-View-ViewModel)
- **Database**: SQLite with Entity Framework Core
- **ORM Approach**: Model First with Code First migrations
- **UI Design**: Modern Fluent Design principles

## Data Storage

All application data is stored in:
```
%APPDATA%\FastPin\fastpin.db
```

The database includes:
- Pinned items (text content, image data, file paths, cached file data)
- Tags and tag associations
- Metadata (creation dates, modification dates)

## Usage

1. **Launch the Application**: The clipboard monitoring starts automatically
2. **Pin Items**:
   - Copy text, images, or files to clipboard
   - Items are automatically detected (or use the Pin buttons in the toolbar)
3. **Organize**:
   - Add tags to items for categorization
   - Use the search box to filter items
   - Toggle "Group by Date" to organize items chronologically
4. **Manage Files**:
   - For file items, use the "Cache file locally" checkbox to switch between link and cache modes
5. **Delete**: Click the Delete button on any item to remove it

## Building

```bash
cd src/FastPin
dotnet restore
dotnet build
dotnet run
```

## Requirements

- .NET 8.0 SDK or later
- Windows OS (for WPF support)

## Architecture

### Models
- `PinnedItem`: Represents a pinned item with support for text, image, or file content
- `Tag`: Represents a tag that can be applied to items
- `ItemTag`: Junction table for many-to-many relationship between items and tags

### ViewModels
- `MainViewModel`: Main application logic, clipboard monitoring, and data operations
- `PinnedItemViewModel`: Wrapper for individual pinned items with UI-friendly properties
- `ItemGroup`: Groups items by date for organized display

### Services
- `ClipboardMonitorService`: Monitors Windows clipboard for changes

### Data Layer
- `FastPinDbContext`: Entity Framework Core DbContext for SQLite database operations

## License

MIT License
