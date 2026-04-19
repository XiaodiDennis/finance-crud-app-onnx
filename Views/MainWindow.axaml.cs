using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FinanceCrudApp.Models;
using FinanceCrudApp.Onnx;

namespace FinanceCrudApp.Views;

public partial class MainWindow : Window
{
    private User? _currentUser;

    private TextBlock? _welcomeTextBlock;
    private TextBlock? _roleTextBlock;
    private TextBlock? _onnxStatusTextBlock;
    private Button? _dashboardButton;
    private Button? _categoryButton;
    private Button? _merchantButton;
    private Button? _accountButton;
    private Button? _transactionButton;

    public MainWindow()
    {
        InitializeComponent();
        BindControls();
        ApplyUserState();
        ApplyOnnxStatus();
    }

    public MainWindow(User user) : this()
    {
        _currentUser = user;
        ApplyUserState();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void BindControls()
    {
        _welcomeTextBlock = this.FindControl<TextBlock>("WelcomeTextBlock");
        _roleTextBlock = this.FindControl<TextBlock>("RoleTextBlock");
        _onnxStatusTextBlock = this.FindControl<TextBlock>("OnnxStatusTextBlock");
        _dashboardButton = this.FindControl<Button>("DashboardButton");
        _categoryButton = this.FindControl<Button>("CategoryButton");
        _merchantButton = this.FindControl<Button>("MerchantButton");
        _accountButton = this.FindControl<Button>("AccountButton");
        _transactionButton = this.FindControl<Button>("TransactionButton");
    }

    private void ApplyUserState()
    {
        if (_currentUser == null)
        {
            if (_welcomeTextBlock != null)
                _welcomeTextBlock.Text = "Welcome";

            if (_roleTextBlock != null)
                _roleTextBlock.Text = "Role: not loaded";

            if (_dashboardButton != null)
                _dashboardButton.IsEnabled = false;

            if (_categoryButton != null)
                _categoryButton.IsEnabled = false;

            if (_merchantButton != null)
                _merchantButton.IsEnabled = false;

            if (_accountButton != null)
                _accountButton.IsEnabled = false;

            if (_transactionButton != null)
                _transactionButton.IsEnabled = false;

            return;
        }

        bool isAdmin = _currentUser.Role == "admin";

        if (_welcomeTextBlock != null)
            _welcomeTextBlock.Text = $"Welcome, {_currentUser.Username}";

        if (_roleTextBlock != null)
            _roleTextBlock.Text = $"Role: {_currentUser.Role}";

        if (_dashboardButton != null)
            _dashboardButton.IsEnabled = true;

        if (_categoryButton != null)
            _categoryButton.IsEnabled = isAdmin;

        if (_merchantButton != null)
            _merchantButton.IsEnabled = isAdmin;

        if (_accountButton != null)
            _accountButton.IsEnabled = isAdmin;

        if (_transactionButton != null)
            _transactionButton.IsEnabled = isAdmin;
    }

    private void ApplyOnnxStatus()
    {
        if (_onnxStatusTextBlock != null)
            _onnxStatusTextBlock.Text = OnnxRuntimeState.ModelService.StatusMessage;
    }

    private void OnOpenDashboardClick(object? sender, RoutedEventArgs e)
    {
        var dashboardWindow = new DashboardWindow();
        dashboardWindow.Show();
    }

    private void OnOpenCategoryManagementClick(object? sender, RoutedEventArgs e)
    {
        var categoryWindow = new CategoryWindow();
        categoryWindow.Show();
    }

    private void OnOpenMerchantManagementClick(object? sender, RoutedEventArgs e)
    {
        var merchantWindow = new MerchantWindow();
        merchantWindow.Show();
    }

    private void OnOpenAccountManagementClick(object? sender, RoutedEventArgs e)
    {
        var accountWindow = new AccountWindow();
        accountWindow.Show();
    }

    private void OnOpenTransactionManagementClick(object? sender, RoutedEventArgs e)
    {
        var transactionWindow = new TransactionWindow();
        transactionWindow.Show();
    }

    private void OnLogoutClick(object? sender, RoutedEventArgs e)
    {
        var loginWindow = new LoginWindow();

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = loginWindow;
        }

        loginWindow.Show();
        Close();
    }
}
