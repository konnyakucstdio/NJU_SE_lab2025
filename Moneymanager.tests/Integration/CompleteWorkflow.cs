using NUnit.Framework;
using FluentAssertions;
using MoneyManager.Services;
using MoneyManager.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MoneyManager.Tests.Integration
{
    [TestFixture]
    public class CompleteWorkflowIntegrationTests
    {
        private string _testFilePath;
        private TransactionService _transactionService;

        [SetUp]
        public void Setup()
        {
            _testFilePath = $"workflow_test_{Guid.NewGuid()}.json";
            _transactionService = new TransactionService();

            // 设置测试文件路径
            var field = typeof(TransactionService).GetField("_dataFilePath",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(_transactionService, _testFilePath);

            // 初始化空数据文件
            File.WriteAllText(_testFilePath, "[]");

            // 重新加载数据
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
        public void CompleteWorkflow_AddDeleteUpdate_ShouldMaintainConsistency()
        {
            // ========== 阶段1：初始状态 ==========
            // Arrange
            var initialTransactionCount = _transactionService.GetTransactions().Count;
            var initialIncome = _transactionService.GetTotalIncome();
            var initialExpense = _transactionService.GetTotalExpense();
            var initialBalance = _transactionService.GetBalance();

            // ========== 阶段2：添加多笔交易 ==========
            // Act - 添加收入交易
            var salaryTransaction = new Transaction
            {
                Title = "Monthly Salary",
                Amount = 8500.00m,
                Type = TransactionType.Income,
                Category = "工资",
                Date = new DateTime(2024, 1, 15)
            };
            _transactionService.AddTransaction(salaryTransaction);

            var bonusTransaction = new Transaction
            {
                Title = "Yearly Bonus",
                Amount = 5000.00m,
                Type = TransactionType.Income,
                Category = "奖金",
                Date = new DateTime(2024, 1, 20)
            };
            _transactionService.AddTransaction(bonusTransaction);

            // Act - 添加支出交易
            var rentTransaction = new Transaction
            {
                Title = "January Rent",
                Amount = 2500.00m,
                Type = TransactionType.Expense,
                Category = "房租",
                Date = new DateTime(2024, 1, 5)
            };
            _transactionService.AddTransaction(rentTransaction);

            var foodTransaction = new Transaction
            {
                Title = "Groceries",
                Amount = 800.00m,
                Type = TransactionType.Expense,
                Category = "餐饮",
                Date = new DateTime(2024, 1, 10)
            };
            _transactionService.AddTransaction(foodTransaction);

            var entertainmentTransaction = new Transaction
            {
                Title = "Movie Tickets",
                Amount = 150.00m,
                Type = TransactionType.Expense,
                Category = "娱乐",
                Date = new DateTime(2024, 1, 12)
            };
            _transactionService.AddTransaction(entertainmentTransaction);

            // Assert - 验证阶段2结果
            var afterAddCount = _transactionService.GetTransactions().Count;
            var afterAddIncome = _transactionService.GetTotalIncome();
            var afterAddExpense = _transactionService.GetTotalExpense();
            var afterAddBalance = _transactionService.GetBalance();

            afterAddCount.Should().Be(initialTransactionCount + 5);
            afterAddIncome.Should().Be(initialIncome + 8500.00m + 5000.00m);
            afterAddExpense.Should().Be(initialExpense + 2500.00m + 800.00m + 150.00m);
            afterAddBalance.Should().Be(initialBalance + 13500.00m - 3450.00m);

            // ========== 阶段3：删除一笔交易 ==========
            // Act - 删除房租交易
            _transactionService.DeleteTransaction(rentTransaction.Id);

            // Assert - 验证阶段3结果
            var afterDeleteCount = _transactionService.GetTransactions().Count;
            var afterDeleteIncome = _transactionService.GetTotalIncome();
            var afterDeleteExpense = _transactionService.GetTotalExpense();
            var afterDeleteBalance = _transactionService.GetBalance();

            afterDeleteCount.Should().Be(initialTransactionCount + 4);
            afterDeleteExpense.Should().Be(initialExpense + 800.00m + 150.00m); // 2500已删除
            afterDeleteBalance.Should().Be(initialBalance + 13500.00m - 950.00m);

            // ========== 阶段4：验证预算计算 ==========
            // Act - 获取预算信息
            var budgets = _transactionService.GetBudgets();
            var foodBudget = budgets.FirstOrDefault(b => b.Category == "餐饮");
            var entertainmentBudget = budgets.FirstOrDefault(b => b.Category == "娱乐");
            var shoppingBudget = budgets.FirstOrDefault(b => b.Category == "购物");
            var transportBudget = budgets.FirstOrDefault(b => b.Category == "交通");

            // Assert - 验证预算
            foodBudget.Should().NotBeNull();
            foodBudget.UsedAmount.Should().Be(800.00m);
            foodBudget.TotalAmount.Should().Be(1500);
            foodBudget.Remaining.Should().Be(700);

            entertainmentBudget.Should().NotBeNull();
            entertainmentBudget.UsedAmount.Should().Be(150.00m);
            entertainmentBudget.TotalAmount.Should().Be(500);
            entertainmentBudget.Remaining.Should().Be(350);

            shoppingBudget.UsedAmount.Should().Be(0); // 没有购物支出
            transportBudget.UsedAmount.Should().Be(0); // 没有交通支出

            // ========== 阶段5：数据持久化验证 ==========
            // Act - 创建新服务实例加载相同数据
            var newService = new TransactionService();
            var newServiceField = typeof(TransactionService).GetField("_dataFilePath",
                BindingFlags.NonPublic | BindingFlags.Instance);
            newServiceField?.SetValue(newService, _testFilePath);

            // 重新加载数据
            var loadMethod = typeof(TransactionService).GetMethod("LoadTransactions",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (loadMethod != null)
            {
                var transactions = loadMethod.Invoke(newService, null);
                var transactionsField = typeof(TransactionService).GetField("_transactions",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                transactionsField?.SetValue(newService, transactions);
            }

            // Assert - 验证新实例数据一致性
            var persistedCount = newService.GetTransactions().Count;
            var persistedIncome = newService.GetTotalIncome();
            var persistedExpense = newService.GetTotalExpense();
            var persistedBalance = newService.GetBalance();

            persistedCount.Should().Be(afterDeleteCount);
            persistedIncome.Should().Be(afterDeleteIncome);
            persistedExpense.Should().Be(afterDeleteExpense);
            persistedBalance.Should().Be(afterDeleteBalance);
        }

        [Test]
        public void CrossMonthTransactionWorkflow_ShouldHandleDateBoundaries()
        {
            // ========== 场景：跨月份交易处理 ==========
            // Arrange - 12月交易
            var decSalary = new Transaction
            {
                Title = "December Salary",
                Amount = 8000.00m,
                Type = TransactionType.Income,
                Category = "工资",
                Date = new DateTime(2023, 12, 25)
            };

            var decRent = new Transaction
            {
                Title = "December Rent",
                Amount = 2400.00m,
                Type = TransactionType.Expense,
                Category = "房租",
                Date = new DateTime(2023, 12, 1)
            };

            // Arrange - 1月交易
            var janSalary = new Transaction
            {
                Title = "January Salary",
                Amount = 8200.00m,
                Type = TransactionType.Income,
                Category = "工资",
                Date = new DateTime(2024, 1, 25)
            };

            var janFood = new Transaction
            {
                Title = "January Food",
                Amount = 900.00m,
                Type = TransactionType.Expense,
                Category = "餐饮",
                Date = new DateTime(2024, 1, 15)
            };

            // Act - 添加所有交易
            _transactionService.AddTransaction(decSalary);
            _transactionService.AddTransaction(decRent);
            _transactionService.AddTransaction(janSalary);
            _transactionService.AddTransaction(janFood);

            // Assert - 验证总体统计
            var allTransactions = _transactionService.GetTransactions();
            var totalIncome = _transactionService.GetTotalIncome();
            var totalExpense = _transactionService.GetTotalExpense();

            allTransactions.Should().HaveCount(4);
            totalIncome.Should().Be(8000.00m + 8200.00m);
            totalExpense.Should().Be(2400.00m + 900.00m);

            // Assert - 验证按日期排序
            allTransactions.Should().BeInDescendingOrder(t => t.Date);
            allTransactions.First().Title.Should().Be("January Salary"); // 最近的在前
            allTransactions.Last().Title.Should().Be("December Rent");   // 最早的在后
        }

        [Test]
        public void BudgetMonitoringWorkflow_ShouldDetectOverspending()
        {
            // ========== 场景：预算超支监控 ==========
            // Arrange - 设置初始预算和交易
            var budgetCategory = "餐饮";
            var budgetLimit = 1500.00m;

            // Act - 添加多笔餐饮支出
            var transactions = new[]
            {
                new Transaction { Title = "Lunch", Amount = 80.00m, Type = TransactionType.Expense, Category = budgetCategory, Date = DateTime.Now },
                new Transaction { Title = "Dinner", Amount = 120.00m, Type = TransactionType.Expense, Category = budgetCategory, Date = DateTime.Now },
                new Transaction { Title = "Groceries", Amount = 450.00m, Type = TransactionType.Expense, Category = budgetCategory, Date = DateTime.Now },
                new Transaction { Title = "Restaurant", Amount = 300.00m, Type = TransactionType.Expense, Category = budgetCategory, Date = DateTime.Now },
                new Transaction { Title = "Coffee", Amount = 150.00m, Type = TransactionType.Expense, Category = budgetCategory, Date = DateTime.Now }
            };

            foreach (var transaction in transactions)
            {
                _transactionService.AddTransaction(transaction);
            }

            // Act - 获取预算信息
            var budgets = _transactionService.GetBudgets();
            var foodBudget = budgets.FirstOrDefault(b => b.Category == budgetCategory);

            // Assert - 验证预算计算
            foodBudget.Should().NotBeNull();
            var totalSpent = transactions.Sum(t => t.Amount);
            foodBudget.UsedAmount.Should().Be(totalSpent);

            // 验证是否超支
            foodBudget.TotalAmount.Should().Be(budgetLimit);
            foodBudget.Remaining.Should().Be(budgetLimit - totalSpent); // 1500 - 1100 = 400

            // 验证百分比
            var expectedPercentage = (totalSpent / budgetLimit) * 100;
            foodBudget.Percentage.Should().BeApproximately(expectedPercentage, 0.01m);

            // Act - 添加更多支出导致超支
            var extraTransaction = new Transaction
            {
                Title = "Expensive Dinner",
                Amount = 600.00m,
                Type = TransactionType.Expense,
                Category = budgetCategory,
                Date = DateTime.Now
            };
            _transactionService.AddTransaction(extraTransaction);

            // 重新获取预算
            budgets = _transactionService.GetBudgets();
            foodBudget = budgets.FirstOrDefault(b => b.Category == budgetCategory);

            // Assert - 验证超支情况
            var newTotalSpent = totalSpent + 600.00m;
            foodBudget.UsedAmount.Should().Be(newTotalSpent);
            foodBudget.Remaining.Should().Be(budgetLimit - newTotalSpent); // 负数：1500 - 1700 = -200
            foodBudget.Remaining.Should().BeNegative();

            // 验证百分比超过100%
            var newPercentage = (newTotalSpent / budgetLimit) * 100;
            foodBudget.Percentage.Should().BeApproximately(newPercentage, 0.01m);
            foodBudget.Percentage.Should().BeGreaterThan(100);
        }

        [Test]
        public void ErrorRecoveryWorkflow_ShouldHandleSystemFailures()
        {
            // ========== 场景：系统故障恢复流程 ==========
            // Phase 1: 正常操作
            var normalTransaction = new Transaction
            {
                Title = "Normal Transaction",
                Amount = 1000.00m,
                Type = TransactionType.Income,
                Category = "工资",
                Date = DateTime.Now
            };
            _transactionService.AddTransaction(normalTransaction);

            var transactionCountBeforeFailure = _transactionService.GetTransactions().Count;

            // Phase 2: 模拟文件损坏
            File.WriteAllText(_testFilePath, "CORRUPTED_JSON_CONTENT{");

            // Phase 3: 创建新服务实例（应该处理损坏文件）
            var recoveryService = new TransactionService();
            var field = typeof(TransactionService).GetField("_dataFilePath",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(recoveryService, _testFilePath);

            // 重新加载数据 - 应该加载示例数据而不是崩溃
            var method = typeof(TransactionService).GetMethod("LoadTransactions",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
            {
                var transactions = method.Invoke(recoveryService, null);
                var transactionsField = typeof(TransactionService).GetField("_transactions",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                transactionsField?.SetValue(recoveryService, transactions);
            }

            // Assert - 验证系统恢复
            var transactionsAfterRecovery = recoveryService.GetTransactions();
            transactionsAfterRecovery.Should().NotBeNull();

            // 系统应该加载示例数据，而不是崩溃或返回空列表
            transactionsAfterRecovery.Should().NotBeEmpty();

            // Phase 4: 系统恢复正常操作
            var newTransaction = new Transaction
            {
                Title = "Recovery Transaction",
                Amount = 500.00m,
                Type = TransactionType.Expense,
                Category = "测试",
                Date = DateTime.Now
            };

            recoveryService.AddTransaction(newTransaction);

            // Assert - 验证可以继续添加交易
            var finalTransactions = recoveryService.GetTransactions();
            finalTransactions.Should().Contain(t => t.Title == "Recovery Transaction");
        }

        [Test]
        public void ConcurrentOperationWorkflow_ShouldMaintainDataIntegrity()
        {
            // ========== 场景：模拟并发操作 ==========
            // Arrange - 创建多个服务实例（模拟多个用户/线程）
            var service1 = new TransactionService();
            var service2 = new TransactionService();

            var field = typeof(TransactionService).GetField("_dataFilePath",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(service1, _testFilePath);
            field?.SetValue(service2, _testFilePath);

            // 初始化两个服务
            InitService(service1);
            InitService(service2);

            // Act - 服务1添加交易
            var transaction1 = new Transaction
            {
                Title = "From Service 1",
                Amount = 1000.00m,
                Type = TransactionType.Income,
                Category = "服务1",
                Date = DateTime.Now
            };
            service1.AddTransaction(transaction1);

            // Act - 服务2添加交易
            var transaction2 = new Transaction
            {
                Title = "From Service 2",
                Amount = 500.00m,
                Type = TransactionType.Expense,
                Category = "服务2",
                Date = DateTime.Now
            };
            service2.AddTransaction(transaction2);

            // 重新加载服务1查看服务2添加的数据
            InitService(service1);

            // Assert - 验证两个交易都存在
            var service1Transactions = service1.GetTransactions();
            var service2Transactions = service2.GetTransactions();

            service1Transactions.Should().HaveCount(2);
            service2Transactions.Should().HaveCount(2);

            service1Transactions.Select(t => t.Title).Should().Contain("From Service 1").And.Contain("From Service 2");

            // 验证统计正确性
            var service1Income = service1.GetTotalIncome();
            var service1Expense = service1.GetTotalExpense();

            service1Income.Should().Be(1000.00m);
            service1Expense.Should().Be(500.00m);
        }

        private void InitService(TransactionService service)
        {
            var method = typeof(TransactionService).GetMethod("LoadTransactions",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
            {
                var transactions = method.Invoke(service, null);
                var transactionsField = typeof(TransactionService).GetField("_transactions",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                transactionsField?.SetValue(service, transactions);
            }
        }
    }
}