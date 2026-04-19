using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FinanceCrudApp.Models;
using FinanceCrudApp.Repositories;

namespace FinanceCrudApp.Views;

public partial class MerchantWindow : Window
{
    private readonly MerchantRepository _merchantRepository = new();

    private ListBox? _merchantsListBox;
    private TextBox? _nameTextBox;
    private TextBox? _descriptionTextBox;
    private CheckBox? _isActiveCheckBox;
    private TextBlock? _messageTextBlock;

    private int _selectedMerchantId = 0;

    public MerchantWindow()
    {
        InitializeComponent();
        BindControls();
        LoadMerchants();
        ClearForm();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void BindControls()
    {
        _merchantsListBox = this.FindControl<ListBox>("MerchantsListBox");
        _nameTextBox = this.FindControl<TextBox>("NameTextBox");
        _descriptionTextBox = this.FindControl<TextBox>("DescriptionTextBox");
        _isActiveCheckBox = this.FindControl<CheckBox>("IsActiveCheckBox");
        _messageTextBlock = this.FindControl<TextBlock>("MessageTextBlock");
    }

    private void LoadMerchants()
    {
        if (_merchantsListBox == null)
            return;

        _merchantsListBox.ItemsSource = _merchantRepository.GetAll();
    }

    private void ClearForm()
    {
        _selectedMerchantId = 0;

        if (_nameTextBox != null)
            _nameTextBox.Text = "";

        if (_descriptionTextBox != null)
            _descriptionTextBox.Text = "";

        if (_isActiveCheckBox != null)
            _isActiveCheckBox.IsChecked = true;

        if (_merchantsListBox != null)
            _merchantsListBox.SelectedItem = null;

        SetMessage("Ready to create a new merchant.");
    }

    private void SetMessage(string text)
    {
        if (_messageTextBlock != null)
            _messageTextBlock.Text = text;
    }

    private void OnMerchantSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (_merchantsListBox?.SelectedItem is not Merchant merchant)
            return;

        _selectedMerchantId = merchant.MerchantId;

        if (_nameTextBox != null)
            _nameTextBox.Text = merchant.Name;

        if (_descriptionTextBox != null)
            _descriptionTextBox.Text = merchant.Description ?? "";

        if (_isActiveCheckBox != null)
            _isActiveCheckBox.IsChecked = merchant.IsActive;

        SetMessage($"Editing merchant ID {_selectedMerchantId}.");
    }

    private void OnRefreshClick(object? sender, RoutedEventArgs e)
    {
        LoadMerchants();
        SetMessage("Merchant list refreshed.");
    }

    private void OnNewClick(object? sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        string name = _nameTextBox?.Text?.Trim() ?? "";
        string? description = _descriptionTextBox?.Text?.Trim();
        bool isActive = _isActiveCheckBox?.IsChecked ?? true;

        if (string.IsNullOrWhiteSpace(name))
        {
            SetMessage("Name is required.");
            return;
        }

        try
        {
            var merchant = new Merchant
            {
                MerchantId = _selectedMerchantId,
                Name = name,
                Description = string.IsNullOrWhiteSpace(description) ? null : description,
                IsActive = isActive
            };

            if (_selectedMerchantId == 0)
            {
                _merchantRepository.Insert(merchant);
                SetMessage("Merchant created successfully.");
            }
            else
            {
                _merchantRepository.Update(merchant);
                SetMessage("Merchant updated successfully.");
            }

            LoadMerchants();
            ClearForm();
        }
        catch (Exception ex)
        {
            SetMessage($"Error: {ex.Message}");
        }
    }

    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (_selectedMerchantId == 0)
        {
            SetMessage("Please select a merchant to delete.");
            return;
        }

        try
        {
            _merchantRepository.Delete(_selectedMerchantId);
            LoadMerchants();
            ClearForm();
            SetMessage("Merchant deleted successfully.");
        }
        catch (Exception ex)
        {
            SetMessage($"Error: {ex.Message}");
        }
    }
}
