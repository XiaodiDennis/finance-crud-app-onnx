using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FinanceCrudApp.Models;
using FinanceCrudApp.Repositories;

namespace FinanceCrudApp.Views;

public partial class TransactionWindow : Window
{
    private readonly TransactionRepository _transactionRepository = new();
    private readonly AccountRepository _accountRepository = new();
    private readonly CategoryRepository _categoryRepository = new();
    private readonly MerchantRepository _merchantRepository = new();

    private ListBox? _transactionsListBox;
    private TextBox? _dateTextBox;
    private TextBox? _amountTextBox;
    private ComboBox? _typeComboBox;
    private ComboBox? _accountComboBox;
    private ComboBox? _categoryComboBox;
    private ComboBox? _merchantComboBox;
    private TextBox? _noteTextBox;
    private TextBlock? _messageTextBlock;

    private int _selectedTransactionId = 0;

    public TransactionWindow()
    {
        InitializeComponent();
        BindControls();
        InitializeComboBoxes();
        LoadTransactions();
        ClearForm();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void BindControls()
    {
        _transactionsListBox = this.FindControl<ListBox>("TransactionsListBox");
        _dateTextBox = this.FindControl<TextBox>("DateTextBox");
        _amountTextBox = this.FindControl<TextBox>("AmountTextBox");
        _typeComboBox = this.FindControl<ComboBox>("TypeComboBox");
        _accountComboBox = this.FindControl<ComboBox>("AccountComboBox");
        _categoryComboBox = this.FindControl<ComboBox>("CategoryComboBox");
        _merchantComboBox = this.FindControl<ComboBox>("MerchantComboBox");
        _noteTextBox = this.FindControl<TextBox>("NoteTextBox");
        _messageTextBlock = this.FindControl<TextBlock>("MessageTextBlock");
    }

    private void InitializeComboBoxes()
    {
        if (_typeComboBox != null)
            _typeComboBox.ItemsSource = new List<string> { "Expense", "Income" };

        if (_typeComboBox != null)
            _typeComboBox.SelectedIndex = 0;

        if (_accountComboBox != null)
            _accountComboBox.ItemsSource = _accountRepository.GetAll();

        if (_categoryComboBox != null)
            _categoryComboBox.ItemsSource = _categoryRepository.GetAll();

        if (_merchantComboBox != null)
            _merchantComboBox.ItemsSource = _merchantRepository.GetAll();
    }

    private void LoadTransactions()
    {
        if (_transactionsListBox == null)
            return;

        _transactionsListBox.ItemsSource = _transactionRepository.GetAll();
    }

    private void ClearForm()
    {
        _selectedTransactionId = 0;

        if (_dateTextBox != null)
            _dateTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");

        if (_amountTextBox != null)
            _amountTextBox.Text = "";

        if (_typeComboBox != null)
            _typeComboBox.SelectedIndex = 0;

        if (_accountComboBox != null && _accountComboBox.ItemCount > 0)
            _accountComboBox.SelectedIndex = 0;

        if (_categoryComboBox != null && _categoryComboBox.ItemCount > 0)
            _categoryComboBox.SelectedIndex = 0;

        if (_merchantComboBox != null && _merchantComboBox.ItemCount > 0)
            _merchantComboBox.SelectedIndex = 0;

        if (_noteTextBox != null)
            _noteTextBox.Text = "";

        if (_transactionsListBox != null)
            _transactionsListBox.SelectedItem = null;

        SetMessage("Ready to create a new transaction.");
    }

    private void SetMessage(string text)
    {
        if (_messageTextBlock != null)
            _messageTextBlock.Text = text;
    }

    private void OnTransactionSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (_transactionsListBox?.SelectedItem is not FinanceTransaction transaction)
            return;

        _selectedTransactionId = transaction.TransactionId;

        if (_dateTextBox != null)
            _dateTextBox.Text = transaction.TransactionDate.ToString("yyyy-MM-dd");

        if (_amountTextBox != null)
            _amountTextBox.Text = transaction.Amount.ToString("F2");

        if (_typeComboBox != null)
            _typeComboBox.SelectedItem = transaction.TransactionType;

        if (_accountComboBox?.ItemsSource is IEnumerable<Account> accounts)
            _accountComboBox.SelectedItem = accounts.FirstOrDefault(x => x.AccountId == transaction.AccountId);

        if (_categoryComboBox?.ItemsSource is IEnumerable<Category> categories)
            _categoryComboBox.SelectedItem = categories.FirstOrDefault(x => x.CategoryId == transaction.CategoryId);

        if (_merchantComboBox?.ItemsSource is IEnumerable<Merchant> merchants)
            _merchantComboBox.SelectedItem = merchants.FirstOrDefault(x => x.MerchantId == transaction.MerchantId);

        if (_noteTextBox != null)
            _noteTextBox.Text = transaction.Note ?? "";

        SetMessage($"Editing transaction ID {_selectedTransactionId}.");
    }

    private void OnRefreshClick(object? sender, RoutedEventArgs e)
    {
        LoadTransactions();
        SetMessage("Transaction list refreshed.");
    }

    private void OnNewClick(object? sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        string dateText = _dateTextBox?.Text?.Trim() ?? "";
        string amountText = _amountTextBox?.Text?.Trim() ?? "";
        string type = _typeComboBox?.SelectedItem?.ToString() ?? "Expense";
        string? note = _noteTextBox?.Text?.Trim();

        if (!DateTime.TryParseExact(dateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime transactionDate))
        {
            SetMessage("Date must be in yyyy-MM-dd format.");
            return;
        }

        if (!decimal.TryParse(amountText, out decimal amount))
        {
            SetMessage("Amount must be a valid number.");
            return;
        }

        if (_accountComboBox?.SelectedItem is not Account account)
        {
            SetMessage("Please select an account.");
            return;
        }

        if (_categoryComboBox?.SelectedItem is not Category category)
        {
            SetMessage("Please select a category.");
            return;
        }

        if (_merchantComboBox?.SelectedItem is not Merchant merchant)
        {
            SetMessage("Please select a merchant.");
            return;
        }

        try
        {
            var transaction = new FinanceTransaction
            {
                TransactionId = _selectedTransactionId,
                TransactionDate = transactionDate,
                Amount = amount,
                TransactionType = type,
                AccountId = account.AccountId,
                CategoryId = category.CategoryId,
                MerchantId = merchant.MerchantId,
                Note = string.IsNullOrWhiteSpace(note) ? null : note
            };

            if (_selectedTransactionId == 0)
            {
                _transactionRepository.Insert(transaction);
                SetMessage("Transaction created successfully.");
            }
            else
            {
                _transactionRepository.Update(transaction);
                SetMessage("Transaction updated successfully.");
            }

            LoadTransactions();
            ClearForm();
        }
        catch (Exception ex)
        {
            SetMessage($"Error: {ex.Message}");
        }
    }

    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (_selectedTransactionId == 0)
        {
            SetMessage("Please select a transaction to delete.");
            return;
        }

        try
        {
            _transactionRepository.Delete(_selectedTransactionId);
            LoadTransactions();
            ClearForm();
            SetMessage("Transaction deleted successfully.");
        }
        catch (Exception ex)
        {
            SetMessage($"Error: {ex.Message}");
        }
    }
}
