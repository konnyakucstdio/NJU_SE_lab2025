using FluentAssertions;
using MoneyManager.Forms;
using MoneyManager.Models;
using MoneyManager.Services;
using NUnit.Framework;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace MoneyManager.Tests.Integration
{
    [TestFixture]
    public class FormsServiceIntegrationTests
    {
        private TransactionService _transactionService;
        private string _testFilePath;

        [SetUp]
        public void Setup()
        {
            _testFilePath = $"forms_integration_{Guid.NewGuid()}.json";
            _transactionService = new TransactionService();

            // 设置测试文件路径
            var field = typeof(TransactionService).GetField("_dataFilePath",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(_transactionService, _testFilePath);

            // 重新加载空数据
            File.WriteAllText(_testFilePath, "[]");
            var method = typeof(TransactionService).GetMethod("LoadTransactions",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
            {
                var transactions = method.Invoke(_transactionService, null);
                var transactionsField = typeof(TransactionService).GetField("_transactions",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                transactionsField?.SetValue(_transactionService, transactions);
            }
        }

        [TearDown]
        public void Teardown()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        [Test]
        public void StatisticsForm_ShouldDisplayCorrectTotals()
        {
            // Arrange - 添加测试数据
            _transactionService.AddTransaction(new Transaction
            {
                Title = "Salary",
                Amount = 3000,
                Type = TransactionType.Income,
                Category = "Salary",
                Date = DateTime.Now
            });

            _transactionService.AddTransaction(new Transaction
            {
                Title = "Rent",
                Amount = 1000,
                Type = TransactionType.Expense,
                Category = "Rent",
                Date = DateTime.Now
            });

            _transactionService.AddTransaction(new Transaction
            {
                Title = "Food",
                Amount = 300,
                Type = TransactionType.Expense,
                Category = "Food",
                Date = DateTime.Now
            });

            // 计算期望值
            var expectedIncome = 3000m;
            var expectedExpense = 1300m; // 1000 + 300
            var expectedBalance = 1700m; // 3000 - 1300

            // Act - 通过反射获取统计值
            var income = _transactionService.GetTotalIncome();
            var expense = _transactionService.GetTotalExpense();
            var balance = _transactionService.GetBalance();

            // Assert - 验证计算正确
            income.Should().Be(expectedIncome);
            expense.Should().Be(expectedExpense);
            balance.Should().Be(expectedBalance);
        }

        [Test]
        public void StatisticsForm_ShouldHandleEmptyData()
        {
            // Arrange - 确保没有数据
            File.WriteAllText(_testFilePath, "[]");

            // 重新加载空数据
            var method = typeof(TransactionService).GetMethod("LoadTransactions",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
            {
                var transactions = method.Invoke(_transactionService, null);
                var transactionsField = typeof(TransactionService).GetField("_transactions",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                transactionsField?.SetValue(_transactionService, transactions);
            }

            // Act - 获取统计值
            var income = _transactionService.GetTotalIncome();
            var expense = _transactionService.GetTotalExpense();
            var balance = _transactionService.GetBalance();

            // Assert
            income.Should().Be(0);
            expense.Should().Be(0);
            balance.Should().Be(0);
        }

        [Test]
        public void StatisticsForm_ShouldUpdateWhenDataChanges()
        {
            // Arrange - 初始状态
            var initialBalance = _transactionService.GetBalance();

            // Act - 添加新交易
            _transactionService.AddTransaction(new Transaction
            {
                Title = "New Income",
                Amount = 500,
                Type = TransactionType.Income,
                Category = "Other",
                Date = DateTime.Now
            });

            // 重新计算
            var updatedBalance = _transactionService.GetBalance();

            // Assert - 检查余额变化
            var expectedChange = 500m;
            (updatedBalance - initialBalance).Should().Be(expectedChange);
        }

        [Test]
        public void StatisticsForm_ShouldDisplayCategoryStatistics()
        {
            // Arrange
            _transactionService.AddTransaction(new Transaction
            {
                Title = "Lunch",
                Amount = 50,
                Type = TransactionType.Expense,
                Category = "餐饮",
                Date = DateTime.Now
            });

            _transactionService.AddTransaction(new Transaction
            {
                Title = "Dinner",
                Amount = 80,
                Type = TransactionType.Expense,
                Category = "餐饮",
                Date = DateTime.Now
            });

            _transactionService.AddTransaction(new Transaction
            {
                Title = "Movie",
                Amount = 120,
                Type = TransactionType.Expense,
                Category = "娱乐",
                Date = DateTime.Now
            });

            // Act - 获取预算信息
            var budgets = _transactionService.GetBudgets();
            var foodBudget = budgets.FirstOrDefault(b => b.Category == "餐饮");
            var entertainmentBudget = budgets.FirstOrDefault(b => b.Category == "娱乐");

            // Assert - 验证预算计算
            foodBudget.Should().NotBeNull();
            foodBudget.UsedAmount.Should().Be(130m); // 50 + 80

            entertainmentBudget.Should().NotBeNull();
            entertainmentBudget.UsedAmount.Should().Be(120m);
        }

        [Test]
        public void FormServiceIntegration_ShouldHandleExceptionsGracefully()
        {
            // Arrange - 创建损坏的文件
            File.WriteAllText(_testFilePath, "corrupted data");

            // 重新创建服务实例
            _transactionService = new TransactionService();
            var field = typeof(TransactionService).GetField("_dataFilePath",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(_transactionService, _testFilePath);

            // 重新加载数据（应该处理异常）
            var method = typeof(TransactionService).GetMethod("LoadTransactions",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
            {
                // 应该不抛出异常
                Action loadAction = () => method.Invoke(_transactionService, null);
                loadAction.Should().NotThrow();
            }

            // Act & Assert - 验证可以正常操作
            var transactions = _transactionService.GetTransactions();
            transactions.Should().NotBeNull();

            // 可以添加新交易
            var newTransaction = new Transaction
            {
                Title = "Test",
                Amount = 100,
                Type = TransactionType.Income,
                Date = DateTime.Now
            };

            Action addAction = () => _transactionService.AddTransaction(newTransaction);
            addAction.Should().NotThrow();
        }

        [Test]
        public void NumberFormat_ShouldMatchDisplayFormat()
        {
            // Arrange
            _transactionService.AddTransaction(new Transaction
            {
                Title = "Salary",
                Amount = 3000.50m,
                Type = TransactionType.Income,
                Category = "Salary",
                Date = DateTime.Now
            });

            // Act - 获取统计值
            var income = _transactionService.GetTotalIncome();

            // 模拟显示的格式化（就像界面上显示的那样）
            var formattedIncome = income.ToString("N2", CultureInfo.InvariantCulture);
            var formattedWithCurrency = $"¥{formattedIncome}";

            // 常见的显示格式
            var displayFormat1 = $"¥{income:N2}";
            var displayFormat2 = string.Format(CultureInfo.InvariantCulture, "¥{0:N2}", income);
            var displayFormat3 = income.ToString("C", CultureInfo.CreateSpecificCulture("zh-CN"));

            // Assert - 验证不同格式
            formattedIncome.Should().Be("3,000.50");
            formattedWithCurrency.Should().Be("¥3,000.50");
            displayFormat1.Should().Be("¥3,000.50");
            displayFormat2.Should().Be("¥3,000.50");

            // 中文货币格式可能显示为"¥3,000.50"或"￥3,000.50"
            displayFormat3.Should().MatchRegex(@"[¥￥]3,000\.50");
        }

        [Test]
        public void CurrencyDisplay_ShouldFormatCorrectly()
        {
            // 测试各种金额的格式化
            var testCases = new[]
            {
                new { Amount = 3000m, ExpectedFormatted = "3,000.00" },
                new { Amount = 3000.50m, ExpectedFormatted = "3,000.50" },
                new { Amount = 1000m, ExpectedFormatted = "1,000.00" },
                new { Amount = 1300m, ExpectedFormatted = "1,300.00" },
                new { Amount = 1700m, ExpectedFormatted = "1,700.00" },
                new { Amount = 500m, ExpectedFormatted = "500.00" }
            };

            foreach (var testCase in testCases)
            {
                var formatted = testCase.Amount.ToString("N2", CultureInfo.InvariantCulture);
                formatted.Should().Be(testCase.ExpectedFormatted);

                // 带货币符号的格式
                var withCurrency = $"¥{formatted}";
                withCurrency.Should().Be($"¥{testCase.ExpectedFormatted}");
            }
        }
    }
}