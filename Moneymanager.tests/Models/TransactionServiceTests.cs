using NUnit.Framework;
using FluentAssertions;
using MoneyManager.Services;
using MoneyManager.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MoneyManager.Tests.Services
{
    [TestFixture]
    public class TransactionServiceTests
    {
        private string _testFilePath;
        private TransactionService _service;

        [SetUp]
        public void Setup()
        {
            // 创建唯一的测试文件名，确保每个测试都是干净的
            _testFilePath = $"test_transactions_{Guid.NewGuid()}.json";

            // 确保文件是空的
            File.WriteAllText(_testFilePath, "[]");

            _service = CreateTestableService(_testFilePath);
        }

        [TearDown]
        public void Teardown()
        {
            // 清理测试文件
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        private TransactionService CreateTestableService(string filePath)
        {
            // 创建服务实例
            var service = new TransactionService();

            // 使用反射设置私有字段 _dataFilePath
            var fieldInfo = typeof(TransactionService).GetField("_dataFilePath",
                BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo?.SetValue(service, filePath);

            // 重新加载交易数据
            var methodInfo = typeof(TransactionService).GetMethod("LoadTransactions",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo != null)
            {
                var transactions = methodInfo.Invoke(service, null);
                var transactionsField = typeof(TransactionService).GetField("_transactions",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                transactionsField?.SetValue(service, transactions);
            }

            return service;
        }

        [Test]
        public void AddTransaction_FirstTransaction_IdShouldBe1()
        {
            // Arrange
            var transaction = new Transaction
            {
                Title = "Test Transaction",
                Amount = 100,
                Type = TransactionType.Income,
                Category = "Test",
                Date = DateTime.Now
            };

            // Act
            _service.AddTransaction(transaction);

            // Assert - 检查ID是否正确分配
            transaction.Id.Should().Be(1);
        }

        [Test]
        public void AddTransaction_MultipleTransactions_IdsShouldIncrement()
        {
            // Arrange
            var transaction1 = new Transaction { Title = "Test 1", Amount = 100, Type = TransactionType.Income, Category = "Test", Date = DateTime.Now };
            var transaction2 = new Transaction { Title = "Test 2", Amount = 200, Type = TransactionType.Expense, Category = "Test", Date = DateTime.Now };

            // Act
            _service.AddTransaction(transaction1);
            _service.AddTransaction(transaction2);

            // Assert - 检查ID递增
            var transactions = _service.GetTransactions();
            var ids = transactions.Select(t => t.Id).OrderBy(id => id).ToList();
            ids.Should().BeEquivalentTo(new[] { 1, 2 });
        }

        [Test]
        public void AddTransaction_ShouldIncreaseTransactionCount()
        {
            // Arrange
            var initialCount = _service.GetTransactions().Count();
            var transaction = new Transaction
            {
                Title = "Test",
                Amount = 100,
                Type = TransactionType.Income,
                Category = "Test",
                Date = DateTime.Now
            };

            // Act
            _service.AddTransaction(transaction);

            // Assert - 检查数量变化
            var finalCount = _service.GetTransactions().Count();
            finalCount.Should().Be(initialCount + 1);
        }

        [Test]
        public void GetTotalIncome_WithIncomeTransactions_ReturnsCorrectSum()
        {
            // Arrange
            var initialIncome = _service.GetTotalIncome();
            var incomeTransaction = new Transaction
            {
                Title = "Salary",
                Amount = 1000,
                Type = TransactionType.Income,
                Category = "Salary",
                Date = DateTime.Now
            };
            _service.AddTransaction(incomeTransaction);

            // Act
            var totalIncome = _service.GetTotalIncome();

            // Assert - 检查收入增加量
            totalIncome.Should().Be(initialIncome + 1000);
        }

        [Test]
        public void GetTotalIncome_WithMixedTransactions_OnlyCountsIncome()
        {
            // Arrange
            var initialIncome = _service.GetTotalIncome();
            var incomeAmount = 1000m;
            var expenseAmount = 500m;

            var incomeTransaction = new Transaction
            {
                Title = "Salary",
                Amount = incomeAmount,
                Type = TransactionType.Income,
                Category = "Salary",
                Date = DateTime.Now
            };

            var expenseTransaction = new Transaction
            {
                Title = "Food",
                Amount = expenseAmount,
                Type = TransactionType.Expense,
                Category = "Food",
                Date = DateTime.Now
            };

            _service.AddTransaction(incomeTransaction);
            _service.AddTransaction(expenseTransaction);

            // Act
            var totalIncome = _service.GetTotalIncome();

            // Assert - 收入只应该增加incomeAmount
            totalIncome.Should().Be(initialIncome + incomeAmount);
        }

        [Test]
        public void GetTotalExpense_WithExpenseTransactions_ReturnsCorrectSum()
        {
            // Arrange
            var initialExpense = _service.GetTotalExpense();
            var expenseTransaction = new Transaction
            {
                Title = "Food",
                Amount = 300,
                Type = TransactionType.Expense,
                Category = "Food",
                Date = DateTime.Now
            };
            _service.AddTransaction(expenseTransaction);

            // Act
            var totalExpense = _service.GetTotalExpense();

            // Assert - 检查支出增加量
            totalExpense.Should().Be(initialExpense + 300);
        }

        [Test]
        public void GetTotalExpense_WithMixedTransactions_OnlyCountsExpense()
        {
            // Arrange
            var initialExpense = _service.GetTotalExpense();
            var expenseAmount = 300m;
            var incomeAmount = 1000m;

            var expenseTransaction = new Transaction
            {
                Title = "Food",
                Amount = expenseAmount,
                Type = TransactionType.Expense,
                Category = "Food",
                Date = DateTime.Now
            };

            var incomeTransaction = new Transaction
            {
                Title = "Salary",
                Amount = incomeAmount,
                Type = TransactionType.Income,
                Category = "Salary",
                Date = DateTime.Now
            };

            _service.AddTransaction(expenseTransaction);
            _service.AddTransaction(incomeTransaction);

            // Act
            var totalExpense = _service.GetTotalExpense();

            // Assert - 支出只应该增加expenseAmount
            totalExpense.Should().Be(initialExpense + expenseAmount);
        }

        [Test]
        public void GetBalance_CalculatesCorrectlyFromIncomeAndExpense()
        {
            // Arrange
            var initialBalance = _service.GetBalance();
            var incomeTransaction = new Transaction
            {
                Title = "Salary",
                Amount = 2000,
                Type = TransactionType.Income,
                Category = "Salary",
                Date = DateTime.Now
            };

            var expenseTransaction1 = new Transaction
            {
                Title = "Rent",
                Amount = 800,
                Type = TransactionType.Expense,
                Category = "Rent",
                Date = DateTime.Now
            };

            var expenseTransaction2 = new Transaction
            {
                Title = "Utilities",
                Amount = 300,
                Type = TransactionType.Expense,
                Category = "Utilities",
                Date = DateTime.Now
            };

            _service.AddTransaction(incomeTransaction);
            _service.AddTransaction(expenseTransaction1);
            _service.AddTransaction(expenseTransaction2);

            // Act
            var balance = _service.GetBalance();

            // Assert - 检查余额变化
            var expectedChange = 2000 - 800 - 300;
            balance.Should().Be(initialBalance + expectedChange);
        }

        [Test]
        public void DeleteTransaction_ExistingId_ShouldRemoveTransaction()
        {
            // Arrange
            var transaction1 = new Transaction { Title = "Keep Me", Amount = 100, Type = TransactionType.Income, Date = DateTime.Now };
            var transaction2 = new Transaction { Title = "Delete Me", Amount = 200, Type = TransactionType.Expense, Date = DateTime.Now };

            _service.AddTransaction(transaction1);
            _service.AddTransaction(transaction2);
            var initialCount = _service.GetTransactions().Count;

            // Act
            _service.DeleteTransaction(transaction2.Id);

            // Assert - 检查数量减少
            var finalCount = _service.GetTransactions().Count;
            finalCount.Should().Be(initialCount - 1);

            // 检查特定交易被删除
            var remainingIds = _service.GetTransactions().Select(t => t.Id).ToList();
            remainingIds.Should().NotContain(transaction2.Id);
        }

        [Test]
        public void DeleteTransaction_NonExistingId_ShouldDoNothing()
        {
            // Arrange
            var initialTransactions = _service.GetTransactions().ToList();
            var initialCount = initialTransactions.Count;

            // Act - 删除不存在的ID
            _service.DeleteTransaction(999);

            // Assert - 数量应该不变
            var finalTransactions = _service.GetTransactions().ToList();
            finalTransactions.Should().HaveCount(initialCount);
            finalTransactions.Should().BeEquivalentTo(initialTransactions);
        }

        [Test]
        public void GetBudgets_WithExpenses_ShouldCalculateCorrectUsedAmount()
        {
            // Arrange - 先清空可能存在的测试文件数据
            File.WriteAllText(_testFilePath, "[]");
            _service = CreateTestableService(_testFilePath);

            var foodTransaction = new Transaction
            {
                Title = "Lunch",
                Amount = 120.50m,
                Type = TransactionType.Expense,
                Category = "餐饮",
                Date = DateTime.Now
            };

            var shoppingTransaction = new Transaction
            {
                Title = "Clothes",
                Amount = 250.75m,
                Type = TransactionType.Expense,
                Category = "购物",
                Date = DateTime.Now
            };

            _service.AddTransaction(foodTransaction);
            _service.AddTransaction(shoppingTransaction);

            // Act
            var budgets = _service.GetBudgets();
            var foodBudget = budgets.FirstOrDefault(b => b.Category == "餐饮");
            var shoppingBudget = budgets.FirstOrDefault(b => b.Category == "购物");

            // Assert - 检查预算使用金额
            foodBudget.Should().NotBeNull();
            foodBudget.UsedAmount.Should().Be(120.50m);

            shoppingBudget.Should().NotBeNull();
            shoppingBudget.UsedAmount.Should().Be(250.75m);
        }

        [Test]
        public void GetBudgets_WithNoExpenses_ShouldReturnZeroUsedAmount()
        {
            // Arrange - 确保是空数据
            File.WriteAllText(_testFilePath, "[]");
            _service = CreateTestableService(_testFilePath);

            // Act
            var budgets = _service.GetBudgets();

            // Assert - 所有预算使用金额应该是0
            budgets.Should().NotBeNull();
            budgets.Should().HaveCount(4); // 餐饮、购物、娱乐、交通

            foreach (var budget in budgets)
            {
                budget.UsedAmount.Should().Be(0);
            }
        }

        [Test]
        public void GetTransactions_ShouldReturnOrderedByDateDescending()
        {
            // Arrange
            var olderTransaction = new Transaction
            {
                Title = "Older",
                Amount = 100,
                Type = TransactionType.Income,
                Date = DateTime.Now.AddDays(-2)
            };

            var newerTransaction = new Transaction
            {
                Title = "Newer",
                Amount = 200,
                Type = TransactionType.Expense,
                Date = DateTime.Now
            };

            // 故意先添加新的，再添加旧的
            _service.AddTransaction(newerTransaction);
            _service.AddTransaction(olderTransaction);

            // Act
            var transactions = _service.GetTransactions();

            // Assert - 检查排序
            transactions.Should().HaveCount(2);
            transactions.Should().BeInDescendingOrder(t => t.Date);
            transactions.First().Title.Should().Be("Newer");
            transactions.Last().Title.Should().Be("Older");
        }

        [Test]
        public void LoadTransactions_WhenFileCorrupted_ShouldReturnSampleData()
        {
            // Arrange - 创建损坏的JSON文件
            File.WriteAllText(_testFilePath, "invalid json content");
            _service = CreateTestableService(_testFilePath);

            // Act
            var transactions = _service.GetTransactions();

            // Assert - 应该返回示例数据
            transactions.Should().NotBeNull();
            transactions.Should().NotBeEmpty();

            // 示例数据应该有特定的交易
            var sampleTitles = transactions.Select(t => t.Title).ToList();
            sampleTitles.Should().Contain("午餐");
            sampleTitles.Should().Contain("工资收入");
        }

        [Test]
        public void LoadTransactions_WhenFileEmpty_ShouldReturnEmptyList()
        {
            // Arrange - 创建空文件
            File.WriteAllText(_testFilePath, "[]");
            _service = CreateTestableService(_testFilePath);

            // Act
            var transactions = _service.GetTransactions();

            // Assert
            transactions.Should().NotBeNull();
            transactions.Should().BeEmpty();
        }
    }
}