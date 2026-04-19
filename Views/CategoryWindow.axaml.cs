using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FinanceCrudApp.Models;
using FinanceCrudApp.Repositories;

namespace FinanceCrudApp.Views;

public partial class CategoryWindow : Window
{
    private readonly CategoryRepository _categoryRepository = new();

    private ListBox? _categoriesListBox;
    private TextBox? _nameTextBox;
    private ComboBox? _typeComboBox;
    private TextBox? _descriptionTextBox;
    private CheckBox? _isActiveCheckBox;
    private TextBlock? _messageTextBlock;

    private int _selectedCategoryId = 0;

    public CategoryWindow()
    {
        InitializeComponent();
        BindControls();
        InitializeTypeComboBox();
        LoadCategories();
        ClearForm();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void BindControls()
    {
        _categoriesListBox = this.FindControl<ListBox>("CategoriesListBox");
        _nameTextBox = this.FindControl<TextBox>("NameTextBox");
        _typeComboBox = this.FindControl<ComboBox>("TypeComboBox");
        _descriptionTextBox = this.FindControl<TextBox>("DescriptionTextBox");
        _isActiveCheckBox = this.FindControl<CheckBox>("IsActiveCheckBox");
        _messageTextBlock = this.FindControl<TextBlock>("MessageTextBlock");
    }

    private void InitializeTypeComboBox()
    {
        if (_typeComboBox == null)
            return;

        _typeComboBox.ItemsSource = new List<string> { "Expense", "Income" };
        _typeComboBox.SelectedIndex = 0;
    }

    private void LoadCategories()
    {
        if (_categoriesListBox == null)
            return;

        _categoriesListBox.ItemsSource = _categoryRepository.GetAll();
    }

    private void ClearForm()
    {
        _selectedCategoryId = 0;

        if (_nameTextBox != null)
            _nameTextBox.Text = "";

        if (_typeComboBox != null)
            _typeComboBox.SelectedIndex = 0;

        if (_descriptionTextBox != null)
            _descriptionTextBox.Text = "";

        if (_isActiveCheckBox != null)
            _isActiveCheckBox.IsChecked = true;

        if (_categoriesListBox != null)
            _categoriesListBox.SelectedItem = null;

        SetMessage("Ready to create a new category.");
    }

    private void SetMessage(string text)
    {
        if (_messageTextBlock != null)
            _messageTextBlock.Text = text;
    }

    private void OnCategorySelected(object? sender, SelectionChangedEventArgs e)
    {
        if (_categoriesListBox?.SelectedItem is not Category category)
            return;

        _selectedCategoryId = category.CategoryId;

        if (_nameTextBox != null)
            _nameTextBox.Text = category.Name;

        if (_typeComboBox != null)
            _typeComboBox.SelectedItem = category.CategoryType;

        if (_descriptionTextBox != null)
            _descriptionTextBox.Text = category.Description ?? "";

        if (_isActiveCheckBox != null)
            _isActiveCheckBox.IsChecked = category.IsActive;

        SetMessage($"Editing category ID {_selectedCategoryId}.");
    }

    private void OnRefreshClick(object? sender, RoutedEventArgs e)
    {
        LoadCategories();
        SetMessage("Category list refreshed.");
    }

    private void OnNewClick(object? sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        string name = _nameTextBox?.Text?.Trim() ?? "";
        string type = _typeComboBox?.SelectedItem?.ToString() ?? "Expense";
        string? description = _descriptionTextBox?.Text?.Trim();
        bool isActive = _isActiveCheckBox?.IsChecked ?? true;

        if (string.IsNullOrWhiteSpace(name))
        {
            SetMessage("Name is required.");
            return;
        }

        try
        {
            var category = new Category
            {
                CategoryId = _selectedCategoryId,
                Name = name,
                CategoryType = type,
                Description = string.IsNullOrWhiteSpace(description) ? null : description,
                IsActive = isActive
            };

            if (_selectedCategoryId == 0)
            {
                _categoryRepository.Insert(category);
                SetMessage("Category created successfully.");
            }
            else
            {
                _categoryRepository.Update(category);
                SetMessage("Category updated successfully.");
            }

            LoadCategories();
            ClearForm();
        }
        catch (Exception ex)
        {
            SetMessage($"Error: {ex.Message}");
        }
    }

    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (_selectedCategoryId == 0)
        {
            SetMessage("Please select a category to delete.");
            return;
        }

        try
        {
            _categoryRepository.Delete(_selectedCategoryId);
            LoadCategories();
            ClearForm();
            SetMessage("Category deleted successfully.");
        }
        catch (Exception ex)
        {
            SetMessage($"Error: {ex.Message}");
        }
    }
}
