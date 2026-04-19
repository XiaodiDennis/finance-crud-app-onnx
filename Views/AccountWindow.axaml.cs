using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FinanceCrudApp.Models;
using FinanceCrudApp.Repositories;

namespace FinanceCrudApp.Views;

public partial class AccountWindow : Window
{
    private readonly AccountRepository _accountRepository = new();

    private ListBox? _accountsListBox;
    private TextBox? _nameTextBox;
    private ComboBox? _typeComboBox;
    private TextBox? _balanceTextBox;
    private CheckBox? _isActiveCheckBox;
    private TextBlock? _messageTextBlock;

    private int _selectedAccountId = 0;

    public AccountWindow()
    {
        InitializeComponent();
        BindControls();
        InitializeTypeComboBox();
        LoadAccounts();
        ClearForm();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void BindControls()
    {
        _accountsListBox = this.FindControl<ListBox>("AccountsListBox");
        _nameTextBox = this.FindControl<TextBox>("NameTextBox");
        _typeComboBox = this.FindControl<ComboBox>("TypeComboBox");
        _balanceTextBox = this.FindControl<TextBox>("BalanceTextBox");
        _isActiveCheckBox = this.FindControl<CheckBox>("IsActiveCheckBox");
        _messageTextBlock = this.FindControl<TextBlock>("MessageTextBlock");
    }

    private void InitializeTypeComboBox()
    {
        if (_typeComboBox == null)
            return;

        _typeComboBox.ItemsSource = new List<string>
        {
            "Cash",
            "Bank Card",
            "Savings",
            "E-Wallet"
        };

        _typeComboBox.SelectedIndex = 0;
    }

    private void LoadAccounts()
    {
        if (_accountsListBox == null)
            return;

        _accountsListBox.ItemsSource = _accountRepository.GetAll();
    }

    private void ClearForm()
    {
        _selectedAccountId = 0;

        if (_nameTextBox != null)
            _nameTextBox.Text = "";

        if (_typeComboBox != null)
            _typeComboBox.SelectedIndex = 0;

        if (_balanceTextBox != null)
            _balanceTextBox.Text = "";

        if (_isActiveCheckBox != null)
            _isActiveCheckBox.IsChecked = true;

        if (_accountsListBox != null)
            _accountsListBox.SelectedItem = null;

        SetMessage("Ready to create a new account.");
    }

    private void SetMessage(string text)
    {
        if (_messageTextBlock != null)
            _messageTextBlock.Text = text;
    }

    private void OnAccountSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (_accountsListBox?.SelectedItem is not Account account)
            return;

        _selectedAccountId = account.AccountId;

        if (_nameTextBox != null)
            _nameTextBox.Text = account.Name;

        if (_typeComboBox != null)
            _typeComboBox.SelectedItem = account.AccountType;

        if (_balanceTextBox != null)
            _balanceTextBox.Text = account.CurrentBalance.ToString("F2");

        if (_isActiveCheckBox != null)
            _isActiveCheckBox.IsChecked = account.IsActive;

        SetMessage($"Editing account ID {_selectedAccountId}.");
    }

    private void OnRefreshClick(object? sender, RoutedEventArgs e)
    {
        LoadAccounts();
        SetMessage("Account list refreshed.");
    }

    private void OnNewClick(object? sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        string name = _nameTextBox?.Text?.Trim() ?? "";
        string type = _typeComboBox?.SelectedItem?.ToString() ?? "Cash";
        string balanceText = _balanceTextBox?.Text?.Trim() ?? "";
        bool isActive = _isActiveCheckBox?.IsChecked ?? true;

        if (string.IsNullOrWhiteSpace(name))
        {
            SetMessage("Name is required.");
            return;
        }

        if (!decimal.TryParse(balanceText, out decimal balance))
        {
            SetMessage("Balance must be a valid number.");
            return;
        }

        try
        {
            var account = new Account
            {
                AccountId = _selectedAccountId,
                Name = name,
                AccountType = type,
                CurrentBalance = balance,
                IsActive = isActive
            };

            if (_selectedAccountId == 0)
            {
                _accountRepository.Insert(account);
                SetMessage("Account created successfully.");
            }
            else
            {
                _accountRepository.Update(account);
                SetMessage("Account updated successfully.");
            }

            LoadAccounts();
            ClearForm();
        }
        catch (Exception ex)
        {
            SetMessage($"Error: {ex.Message}");
        }
    }

    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (_selectedAccountId == 0)
        {
            SetMessage("Please select an account to delete.");
            return;
        }

        try
        {
            _accountRepository.Delete(_selectedAccountId);
            LoadAccounts();
            ClearForm();
            SetMessage("Account deleted successfully.");
        }
        catch (Exception ex)
        {
            SetMessage($"Error: {ex.Message}");
        }
    }
}
