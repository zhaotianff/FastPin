# FastPin User Guide

## Getting Started

### First Launch
When you first launch FastPin, the application will:
1. Create a database file in `%APPDATA%\FastPin\fastpin.db`
2. Start monitoring your clipboard automatically
3. Display an empty main window ready to receive pinned items

### Understanding the Interface

#### Header
- **Title Bar**: Shows "FastPin - Pin Anything"
- Blue header with modern design

#### Toolbar
The toolbar contains:
- **Search Box**: Filter items by text, filename, or tags
- **Pin Text Button**: Manually pin text from clipboard
- **Pin Image Button**: Manually pin images from clipboard  
- **Pin File Button**: Manually pin files from clipboard
- **Group by Date Checkbox**: Toggle between grouped and flat view

#### Main Content Area
Displays your pinned items with:
- Item type badge (Text/Image/File)
- Date information
- Content preview
- Tags
- Delete button

#### Status Bar
Shows:
- Total item count
- Clipboard monitoring status

## How to Use

### Pinning Items

#### Automatic Pinning (Recommended)
1. Simply copy any text, image, or file to your clipboard
2. FastPin will automatically detect it
3. The item will be added to your collection

#### Manual Pinning
1. Copy content to clipboard
2. Click the appropriate Pin button in toolbar
3. Item will be added

### Managing Items

#### Searching Items
1. Type in the search box
2. Results update in real-time
3. Press Escape or click X to clear search

#### Deleting Items
1. Find the item you want to remove
2. Click the red "Delete" button
3. Confirm deletion in the dialog

### File Handling

When you pin a file, you have two options:

#### Link Mode (Default)
- Only the file path is stored
- No additional disk space used
- File must remain in original location

#### Cache Mode
- Complete copy of file is stored in database
- Uses additional disk space
- File remains accessible even if original is moved/deleted

**To toggle**: Check/uncheck the "Cache file locally" checkbox on any file item

### Viewing Options

#### Grouped View (Default)
Items are organized into date groups:
- **Today**: Items pinned today
- **Yesterday**: Items from yesterday
- **This Week**: Items from last 7 days
- **This Month**: Items from last 30 days
- **[Month Year]**: Items from specific months
- **[Year]**: Items from specific years

#### Ungrouped View
All items in a flat list, sorted by date (newest first)

**To toggle**: Check/uncheck "Group by Date" in toolbar

## Tips and Tricks

- Use Link mode for large files to save database space
- Regularly delete items you no longer need
- Use tags and search to organize large collections
- Keep FastPin running in background for automatic capture

## Data Location

All data stored in:
```
%APPDATA%\FastPin\fastpin.db
```
