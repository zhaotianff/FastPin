using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FastPin.Commands;
using FastPin.Data;
using FastPin.Models;
using Microsoft.EntityFrameworkCore;

namespace FastPin.ViewModels
{
    /// <summary>
    /// ViewModel for Tag Management window
    /// </summary>
    public class TagManagementViewModel : ViewModelBase
    {
        private readonly FastPinDbContext _dbContext;
        private TagViewModel? _selectedTag;
        private string _editingTagName = string.Empty;
        private string? _editingTagClass;
        private string? _editingTagColor;
        private bool _isEditingExisting;

        public TagManagementViewModel()
        {
            _dbContext = new FastPinDbContext();

            // Initialize commands
            SaveTagCommand = new RelayCommand(SaveTag, CanSaveTag);
            NewTagCommand = new RelayCommand(NewTag);
            DeleteTagCommand = new RelayCommand<TagViewModel>(DeleteTag, tag => tag != null);

            LoadTags();
        }

        public ObservableCollection<TagViewModel> Tags { get; } = new ObservableCollection<TagViewModel>();

        public TagViewModel? SelectedTag
        {
            get => _selectedTag;
            set
            {
                if (SetProperty(ref _selectedTag, value))
                {
                    LoadTagForEditing();
                }
            }
        }

        public string EditingTagName
        {
            get => _editingTagName;
            set => SetProperty(ref _editingTagName, value);
        }

        public string? EditingTagClass
        {
            get => _editingTagClass;
            set => SetProperty(ref _editingTagClass, value);
        }

        public string? EditingTagColor
        {
            get => _editingTagColor;
            set => SetProperty(ref _editingTagColor, value);
        }

        public ICommand SaveTagCommand { get; }
        public ICommand NewTagCommand { get; }
        public ICommand DeleteTagCommand { get; }

        private void LoadTags()
        {
            Tags.Clear();
            var tags = _dbContext.Tags
                .OrderBy(t => t.Name)
                .ToList();

            foreach (var tag in tags)
            {
                Tags.Add(new TagViewModel(tag));
            }
        }

        private void LoadTagForEditing()
        {
            if (SelectedTag == null)
            {
                EditingTagName = string.Empty;
                EditingTagClass = null;
                EditingTagColor = null;
                _isEditingExisting = false;
            }
            else
            {
                EditingTagName = SelectedTag.Name;
                EditingTagClass = SelectedTag.Class;
                EditingTagColor = SelectedTag.Color;
                _isEditingExisting = true;
            }
        }

        private bool CanSaveTag()
        {
            return !string.IsNullOrWhiteSpace(EditingTagName);
        }

        private void SaveTag()
        {
            try
            {
                var tagName = EditingTagName.Trim();

                if (_isEditingExisting && SelectedTag != null)
                {
                    // Update existing tag
                    var tag = _dbContext.Tags.FirstOrDefault(t => t.Id == SelectedTag.Id);
                    if (tag != null)
                    {
                        // Check if name is being changed and if it conflicts
                        if (tag.Name != tagName)
                        {
                            var existingTag = _dbContext.Tags.FirstOrDefault(t => t.Name == tagName && t.Id != tag.Id);
                            if (existingTag != null)
                            {
                                MessageBox.Show("A tag with this name already exists.", "Duplicate Tag", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }

                        tag.Name = tagName;
                        tag.Class = string.IsNullOrWhiteSpace(EditingTagClass) ? null : EditingTagClass.Trim();
                        tag.Color = string.IsNullOrWhiteSpace(EditingTagColor) ? null : EditingTagColor.Trim();
                        _dbContext.SaveChanges();

                        // Update ViewModel
                        SelectedTag.Name = tag.Name;
                        SelectedTag.Class = tag.Class;
                        SelectedTag.Color = tag.Color;
                    }
                }
                else
                {
                    // Create new tag
                    var existingTag = _dbContext.Tags.FirstOrDefault(t => t.Name == tagName);
                    if (existingTag != null)
                    {
                        MessageBox.Show("A tag with this name already exists.", "Duplicate Tag", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var newTag = new Tag
                    {
                        Name = tagName,
                        Class = string.IsNullOrWhiteSpace(EditingTagClass) ? null : EditingTagClass.Trim(),
                        Color = string.IsNullOrWhiteSpace(EditingTagColor) ? null : EditingTagColor.Trim()
                    };

                    _dbContext.Tags.Add(newTag);
                    _dbContext.SaveChanges();

                    // Add to list
                    var tagViewModel = new TagViewModel(newTag);
                    Tags.Add(tagViewModel);
                    SelectedTag = tagViewModel;
                }

                MessageBox.Show("Tag saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving tag: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewTag()
        {
            SelectedTag = null;
            EditingTagName = string.Empty;
            EditingTagClass = null;
            EditingTagColor = "#0078D4"; // Default color
            _isEditingExisting = false;
        }

        private void DeleteTag(TagViewModel? tag)
        {
            if (tag == null)
                return;

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the tag '{tag.Name}'? This will remove it from all pinned items.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var dbTag = _dbContext.Tags.FirstOrDefault(t => t.Id == tag.Id);
                    if (dbTag != null)
                    {
                        _dbContext.Tags.Remove(dbTag);
                        _dbContext.SaveChanges();
                        Tags.Remove(tag);

                        if (SelectedTag == tag)
                        {
                            SelectedTag = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting tag: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
