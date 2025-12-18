using FluentAssertions;
using MoneyManager.Models;
using MoneyManager.Services;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MoneyManager.Tests.Integration
{
    [TestFixture]
    public class TransactionServiceFileIntegrationTests
    {
        private string _testFilePath;
        private TransactionService _service1;
        private TransactionService _service2;

        [SetUp]
        public void Setup()
        {
            _testFilePath = $"integration_test_{Guid.NewGuid()}.json";
            // 创建空文件
            File.WriteAllText(_testFilePath, "[]");
        }

        [TearDown]
        public void Teardown()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        private TransactionService CreateTestableService(string filePath)
        {
            var service = new TransactionService();
            SetPrivateFilePath(service, filePath);
            return service;
        }

        private void SetPrivateFilePath(TransactionService service, string filePath)
        {
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
        }

        [Test]
        public void DataPersistence_ShouldPersistBetweenServiceInstances()
        {
            // Arrange - 创建第一个服务实例
            _service1 = CreateTestableService(_testFilePath);

            // 记录初始状态
            var initialCount = _service1.GetTransactions().Count;

            var transaction = new Transaction
            {
                Title = "Integration Test",
                Amount = 123.45m,
                Type = TransactionType.Expense,
                Category = "Test",
                Date = DateTime.Now
            };

            // Act - 通过第一个服务添加数据
            _service1.AddTransaction(transaction);

            // 创建第二个服务实例，应该加载相同的数据
            _service2 = CreateTestableService(_testFilePath);

            // Assert - 验证第二个实例能读取第一个实例保存的数据
            var transactions = _service2.GetTransactions();

            // 检查数量增加了1
            transactions.Should().HaveCount(initialCount + 1);

            // 检查新增的交易存在
            var newTransaction = transactions.FirstOrDefault(t => t.Title == "Integration Test");
            newTransaction.Should().NotBeNull();
            newTransaction.Amount.Should().Be(123.45m);
        }

        [Test]
        public void MultiTransactionPersistence_ShouldMaintainAllData()
        {
            // Arrange
            _service1 = CreateTestableService(_testFilePath);
            var initialCount = _service1.GetTransactions().Count;

            var transactionsToAdd = new[]
            {
                new Transaction { Title = "T1", Amount = 100, Type = TransactionType.Income, Date = DateTime.Now },
                new Transaction { Title = "T2", Amount = 50, Type = TransactionType.Expense, Date = DateTime.Now },
                new Transaction { Title = "T3", Amount = 75, Type = TransactionType.Income, Date = DateTime.Now }
            };

            // Act - 添加多个交易
            foreach (var t in transactionsToAdd)
            {
                _service1.AddTransaction(t);
            }

            // 创建新实例加载数据
            _service2 = CreateTestableService(_testFilePath);

            // Assert - 检查数量增加了3
            var loadedTransactions = _service2.GetTransactions();
            loadedTransactions.Should().HaveCount(initialCount + 3);

            // 检查所有新增交易都存在
            var titles = loadedTransactions.Skip(initialCount).Select(t => t.Title).ToList();
            titles.Should().Contain("T1").And.Contain("T2").And.Contain("T3");
        }

        [Test]
        public void DeleteTransactionPersistence_ShouldReflectInNewInstance()
        {
            // Arrange
            _service1 = CreateTestableService(_testFilePath);
            var initialCount = _service1.GetTransactions().Count;

            var t1 = new Transaction { Title = "Keep", Amount = 100, Type = TransactionType.Income, Date = DateTime.Now };
            var t2 = new Transaction { Title = "Delete", Amount = 50, Type = TransactionType.Expense, Date = DateTime.Now };

            _service1.AddTransaction(t1);
            _service1.AddTransaction(t2);

            // 记录添加后的数量
            var afterAddCount = _service1.GetTransactions().Count;
            var idToDelete = t2.Id;

            // Act - 删除一个交易
            _service1.DeleteTransaction(idToDelete);

            // 创建新实例
            _service2 = CreateTestableService(_testFilePath);

            // Assert - 检查数量减少了1（相比添加后）
            var transactions = _service2.GetTransactions();
            transactions.Should().HaveCount(afterAddCount - 1);

            // 检查删除的交易不存在
            transactions.Should().NotContain(t => t.Title == "Delete");
            // 检查保留的交易存在
            transactions.Should().Contain(t => t.Title == "Keep");
        }

        [Test]
        public void FileCorruption_ShouldHandleGracefully()
        {
            // Arrange - 创建损坏的文件
            File.WriteAllText(_testFilePath, "{invalid json}");

            // Act - 创建服务，应该处理损坏文件
            _service1 = CreateTestableService(_testFilePath);

            // Assert - 应该加载示例数据而不是崩溃
            var transactions = _service1.GetTransactions();
            transactions.Should().NotBeNull();

            // 记录当前数量
            var initialCount = transactions.Count;

            // 可以添加交易
            var transaction = new Transaction
            {
                Title = "After Corruption",
                Amount = 100,
                Type = TransactionType.Income,
                Date = DateTime.Now
            };

            _service1.AddTransaction(transaction);

            // 验证添加成功 - 数量增加了1
            var updatedTransactions = _service1.GetTransactions();
            updatedTransactions.Should().HaveCount(initialCount + 1);
            updatedTransactions.Should().Contain(t => t.Title == "After Corruption");
        }

        [Test]
        public void StatisticsCalculations_ShouldBeConsistentAfterPersistence()
        {
            // Arrange
            _service1 = CreateTestableService(_testFilePath);

            // 记录初始统计
            var initialIncome = _service1.GetTotalIncome();
            var initialExpense = _service1.GetTotalExpense();
            var initialBalance = _service1.GetBalance();

            // 添加测试数据
            var incomeTransaction = new Transaction
            {
                Title = "Test Income",
                Amount = 1500m,
                Type = TransactionType.Income,
                Category = "Test",
                Date = DateTime.Now
            };

            var expenseTransaction = new Transaction
            {
                Title = "Test Expense",
                Amount = 750m,
                Type = TransactionType.Expense,
                Category = "Test",
                Date = DateTime.Now
            };

            _service1.AddTransaction(incomeTransaction);
            _service1.AddTransaction(expenseTransaction);

            // 计算期望的变化
            var expectedIncomeChange = 1500m;
            var expectedExpenseChange = 750m;
            var expectedBalanceChange = 1500m - 750m;

            // Act - 创建新实例验证统计
            _service2 = CreateTestableService(_testFilePath);

            // Assert - 验证统计变化一致
            var newIncome = _service2.GetTotalIncome();
            var newExpense = _service2.GetTotalExpense();
            var newBalance = _service2.GetBalance();

            (newIncome - initialIncome).Should().Be(expectedIncomeChange);
            (newExpense - initialExpense).Should().Be(expectedExpenseChange);
            (newBalance - initialBalance).Should().Be(expectedBalanceChange);
        }
    }
}