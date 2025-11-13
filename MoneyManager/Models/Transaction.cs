using System;

namespace MoneyManager.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Category { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Account { get; set; }
    }

    public enum TransactionType
    {
        Income,
        Expense
    }

    public class Budget
    {
        public string Category { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal UsedAmount { get; set; }

        // 添加缺失的计算属性
        public decimal Remaining
        {
            get { return TotalAmount - UsedAmount; }
        }

        public decimal Percentage
        {
            get { return TotalAmount > 0 ? (UsedAmount / TotalAmount) * 100 : 0; }
        }
    }

    public class Account
    {
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
    }
}