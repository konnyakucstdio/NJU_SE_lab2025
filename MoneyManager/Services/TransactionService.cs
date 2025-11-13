using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using MoneyManager.Models;

namespace MoneyManager.Services
{
    public class TransactionService
    {
        private readonly string _dataFilePath = "transactions.json";
        private List<Transaction> _transactions;

        public TransactionService()
        {
            _transactions = LoadTransactions();
        }

        public List<Transaction> GetTransactions()
        {
            return _transactions.OrderByDescending(t => t.Date).ToList();
        }

        public void AddTransaction(Transaction transaction)
        {
            transaction.Id = _transactions.Count > 0 ? _transactions.Max(t => t.Id) + 1 : 1;
            _transactions.Add(transaction);
            SaveTransactions();
        }

        public void DeleteTransaction(int id)
        {
            var transaction = _transactions.FirstOrDefault(t => t.Id == id);
            if (transaction != null)
            {
                _transactions.Remove(transaction);
                SaveTransactions();
            }
        }

        public decimal GetTotalIncome()
        {
            return _transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        }

        public decimal GetTotalExpense()
        {
            return _transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        }

        public decimal GetBalance()
        {
            return GetTotalIncome() - GetTotalExpense();
        }

        public List<Budget> GetBudgets()
        {
            var expensesByCategory = _transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            // 使用兼容的方法替代 GetValueOrDefault
            return new List<Budget>
            {
                new Budget {
                    Category = "餐饮",
                    TotalAmount = 1500,
                    UsedAmount = GetDictionaryValue(expensesByCategory, "餐饮")
                },
                new Budget {
                    Category = "购物",
                    TotalAmount = 1000,
                    UsedAmount = GetDictionaryValue(expensesByCategory, "购物")
                },
                new Budget {
                    Category = "娱乐",
                    TotalAmount = 500,
                    UsedAmount = GetDictionaryValue(expensesByCategory, "娱乐")
                },
                new Budget {
                    Category = "交通",
                    TotalAmount = 300,
                    UsedAmount = GetDictionaryValue(expensesByCategory, "交通")
                }
            };
        }

        // 替代 GetValueOrDefault 的兼容方法
        private decimal GetDictionaryValue(Dictionary<string, decimal> dictionary, string key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : 0;
        }

        private List<Transaction> LoadTransactions()
        {
            if (!File.Exists(_dataFilePath))
                return GetSampleData();

            try
            {
                var json = File.ReadAllText(_dataFilePath);
                return JsonSerializer.Deserialize<List<Transaction>>(json) ?? new List<Transaction>();
            }
            catch
            {
                return GetSampleData();
            }
        }

        private void SaveTransactions()
        {
            try
            {
                var json = JsonSerializer.Serialize(_transactions, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存数据时出错: {ex.Message}");
            }
        }

        private List<Transaction> GetSampleData()
        {
            return new List<Transaction>
            {
                new Transaction {
                    Id = 1,
                    Title = "午餐",
                    Amount = 42.50m,
                    Type = TransactionType.Expense,
                    Category = "餐饮",
                    Date = DateTime.Today
                },
                new Transaction {
                    Id = 2,
                    Title = "工资收入",
                    Amount = 15800.00m,
                    Type = TransactionType.Income,
                    Category = "工资",
                    Date = DateTime.Today.AddDays(-1)
                },
                new Transaction {
                    Id = 3,
                    Title = "网购商品",
                    Amount = 328.00m,
                    Type = TransactionType.Expense,
                    Category = "购物",
                    Date = DateTime.Today.AddDays(-2)
                },
                new Transaction {
                    Id = 4,
                    Title = "电影票",
                    Amount = 85.00m,
                    Type = TransactionType.Expense,
                    Category = "娱乐",
                    Date = DateTime.Today.AddDays(-3)
                }
            };
        }
    }
}