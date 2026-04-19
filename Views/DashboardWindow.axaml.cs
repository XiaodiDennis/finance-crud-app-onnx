using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FinanceCrudApp.Models;
using FinanceCrudApp.Repositories;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace FinanceCrudApp.Views;

public partial class DashboardWindow : Window
{
    private readonly TransactionRepository _transactionRepository = new();

    private TextBlock? _categoryCountTextBlock;
    private TextBlock? _merchantCountTextBlock;
    private TextBlock? _accountCountTextBlock;
    private TextBlock? _transactionCountTextBlock;
    private TextBlock? _totalIncomeTextBlock;
    private TextBlock? _totalExpenseTextBlock;
    private TextBlock? _netBalanceTextBlock;

    private ContentControl? _expenseChartHost;
    private ContentControl? _monthlyChartHost;
    private ContentControl? _dailyChartHost;

    public DashboardWindow()
    {
        InitializeComponent();
        BindControls();
        LoadSummary();
        BuildExpenseChart();
        BuildMonthlyChart();
        BuildDailyChart();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void BindControls()
    {
        _categoryCountTextBlock = this.FindControl<TextBlock>("CategoryCountTextBlock");
        _merchantCountTextBlock = this.FindControl<TextBlock>("MerchantCountTextBlock");
        _accountCountTextBlock = this.FindControl<TextBlock>("AccountCountTextBlock");
        _transactionCountTextBlock = this.FindControl<TextBlock>("TransactionCountTextBlock");
        _totalIncomeTextBlock = this.FindControl<TextBlock>("TotalIncomeTextBlock");
        _totalExpenseTextBlock = this.FindControl<TextBlock>("TotalExpenseTextBlock");
        _netBalanceTextBlock = this.FindControl<TextBlock>("NetBalanceTextBlock");

        _expenseChartHost = this.FindControl<ContentControl>("ExpenseChartHost");
        _monthlyChartHost = this.FindControl<ContentControl>("MonthlyChartHost");
        _dailyChartHost = this.FindControl<ContentControl>("DailyChartHost");
    }

    private void LoadSummary()
    {
        DashboardSummary summary = _transactionRepository.GetDashboardSummary();

        if (_categoryCountTextBlock != null)
            _categoryCountTextBlock.Text = summary.CategoryCount.ToString();

        if (_merchantCountTextBlock != null)
            _merchantCountTextBlock.Text = summary.MerchantCount.ToString();

        if (_accountCountTextBlock != null)
            _accountCountTextBlock.Text = summary.AccountCount.ToString();

        if (_transactionCountTextBlock != null)
            _transactionCountTextBlock.Text = summary.TransactionCount.ToString();

        if (_totalIncomeTextBlock != null)
            _totalIncomeTextBlock.Text = summary.TotalIncome.ToString("F2");

        if (_totalExpenseTextBlock != null)
            _totalExpenseTextBlock.Text = summary.TotalExpense.ToString("F2");

        if (_netBalanceTextBlock != null)
            _netBalanceTextBlock.Text = summary.NetBalance.ToString("F2");
    }

    private void BuildExpenseChart()
    {
        if (_expenseChartHost == null)
            return;

        var expenseItems = _transactionRepository.GetExpenseTotalsByCategory();

        var series = expenseItems
            .Select(item => new PieSeries<double>
            {
                Name = item.Label,
                Values = new[] { (double)item.Value }
            })
            .ToArray();

        var chart = new PieChart
        {
            Series = series,
            LegendPosition = LegendPosition.Right
        };

        _expenseChartHost.Content = chart;
    }

    private void BuildMonthlyChart()
    {
        if (_monthlyChartHost == null)
            return;

        var monthlyItems = _transactionRepository.GetNetTotalsByMonth();
        var labels = monthlyItems.Select(x => x.Label).ToArray();
        var values = monthlyItems.Select(x => (double)x.Value).ToArray();

        var chart = new CartesianChart
        {
            Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "Net Total",
                    Values = values,
                    Fill = new SolidColorPaint(SKColors.SteelBlue)
                }
            },
            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = labels
                }
            },
            YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Net Amount"
                }
            },
            LegendPosition = LegendPosition.Hidden
        };

        _monthlyChartHost.Content = chart;
    }

    private void BuildDailyChart()
    {
        if (_dailyChartHost == null)
            return;

        var dailyItems = _transactionRepository.GetDailyExpenseTotalsLast30Days();
        var labels = dailyItems.Select(x => x.Label.Length >= 10 ? x.Label.Substring(5, 5) : x.Label).ToArray();
        var values = dailyItems.Select(x => (double)x.Value).ToArray();

        var chart = new CartesianChart
        {
            Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = "Daily Expense",
                    Values = values,
                    Stroke = new SolidColorPaint(SKColors.RoyalBlue, 3),
                    Fill = new SolidColorPaint(new SKColor(59, 130, 246, 60)),
                    GeometrySize = 8
                }
            },
            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = labels
                }
            },
            YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Expense"
                }
            },
            LegendPosition = LegendPosition.Hidden
        };

        _dailyChartHost.Content = chart;
    }
}
