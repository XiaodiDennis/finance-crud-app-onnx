using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FinanceCrudApp.Helpers;
using FinanceCrudApp.Models;
using FinanceCrudApp.Repositories;

namespace FinanceCrudApp.Views;

public partial class LoginWindow : Window
{
    private TextBox? _usernameTextBox;
    private TextBox? _passwordTextBox;
    private TextBlock? _errorTextBlock;

    public LoginWindow()
    {
        InitializeComponent();

        _usernameTextBox = this.FindControl<TextBox>("UsernameTextBox");
        _passwordTextBox = this.FindControl<TextBox>("PasswordTextBox");
        _errorTextBlock = this.FindControl<TextBlock>("ErrorTextBlock");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnLoginClick(object? sender, RoutedEventArgs e)
    {
        string username = _usernameTextBox?.Text?.Trim() ?? "";
        string password = _passwordTextBox?.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            if (_errorTextBlock != null)
                _errorTextBlock.Text = "Please enter both username and password.";
            return;
        }

        var userRepository = new UserRepository();
        User? user = userRepository.GetByUsername(username);

        if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash))
        {
            if (_errorTextBlock != null)
                _errorTextBlock.Text = "Invalid username or password.";
            return;
        }

        var mainWindow = new MainWindow(user);

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = mainWindow;
        }

        mainWindow.Show();
        Close();
    }
}
