using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MoneyManager.Models;
using MoneyManager.Services;

namespace MoneyManager.Forms
{
    public partial class StatisticsForm : Form
    {
        private TransactionService _transactionService;

        private Label lblTotalIncome;
        private Label lblTotalExpense;
        private Label lblBalance;
        private FlowLayoutPanel flowLayoutCategories;
        private Panel chartPanel;

        public StatisticsForm(TransactionService transactionService)
        {
            _transactionService = transactionService;
            InitializeComponent();
            CreateUI();
            LoadStatistics();
        }

        private void CreateUI()
        {
            this.Text = "收支统计";
            this.Size = new Size(400, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.Padding = new Padding(10);

            CreateStatsCards();
            CreateCategoryStats();
            CreateChart();
            CreateCloseButton();
        }

        private void CreateStatsCards()
        {
            var statsPanel = new Panel
            {
                Size = new Size(380, 120),
                Location = new Point(10, 10)
            };

            // 总收入卡片
            var incomeCard = CreateStatCard("总收入", new Point(0, 0), Color.Green);
            lblTotalIncome = new Label
            {
                Text = "¥0.00",
                ForeColor = Color.Green,
                Font = new Font("微软雅黑", 14, FontStyle.Bold),
                Location = new Point(10, 40),
                AutoSize = true
            };
            incomeCard.Controls.Add(lblTotalIncome);

            // 总支出卡片
            var expenseCard = CreateStatCard("总支出", new Point(130, 0), Color.Red);
            lblTotalExpense = new Label
            {
                Text = "¥0.00",
                ForeColor = Color.Red,
                Font = new Font("微软雅黑", 14, FontStyle.Bold),
                Location = new Point(10, 40),
                AutoSize = true
            };
            expenseCard.Controls.Add(lblTotalExpense);

            // 结余卡片
            var balanceCard = CreateStatCard("结余", new Point(260, 0), Color.Blue);
            lblBalance = new Label
            {
                Text = "¥0.00",
                ForeColor = Color.Blue,
                Font = new Font("微软雅黑", 14, FontStyle.Bold),
                Location = new Point(10, 40),
                AutoSize = true
            };
            balanceCard.Controls.Add(lblBalance);

            statsPanel.Controls.Add(incomeCard);
            statsPanel.Controls.Add(expenseCard);
            statsPanel.Controls.Add(balanceCard);

            this.Controls.Add(statsPanel);
        }

        private Panel CreateStatCard(string title, Point location, Color color)
        {
            var panel = new Panel
            {
                Size = new Size(120, 100),
                Location = location,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var titleLabel = new Label
            {
                Text = title,
                Location = new Point(10, 15),
                AutoSize = true,
                Font = new Font("微软雅黑", 10)
            };

            panel.Controls.Add(titleLabel);
            return panel;
        }

        private void CreateCategoryStats()
        {
            var categoryLabel = new Label
            {
                Text = "支出分类统计",
                Font = new Font("微软雅黑", 12, FontStyle.Bold),
                Location = new Point(10, 150),
                AutoSize = true
            };

            flowLayoutCategories = new FlowLayoutPanel
            {
                Location = new Point(10, 180),
                Size = new Size(380, 200),
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true
            };

            this.Controls.Add(categoryLabel);
            this.Controls.Add(flowLayoutCategories);
        }

        private void CreateChart()
        {
            var chartLabel = new Label
            {
                Text = "月度趋势",
                Font = new Font("微软雅黑", 12, FontStyle.Bold),
                Location = new Point(10, 400),
                AutoSize = true
            };

            chartPanel = new Panel
            {
                Location = new Point(10, 430),
                Size = new Size(380, 100),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            this.Controls.Add(chartLabel);
            this.Controls.Add(chartPanel);
        }

        private void CreateCloseButton()
        {
            var btnClose = new Button
            {
                Text = "关闭",
                Location = new Point(150, 550),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(67, 97, 238),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 10),
                DialogResult = DialogResult.OK
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(btnClose);
        }

        private void LoadStatistics()
        {
            var totalIncome = _transactionService.GetTotalIncome();
            var totalExpense = _transactionService.GetTotalExpense();
            var balance = _transactionService.GetBalance();

            // 更新统计卡片
            lblTotalIncome.Text = $"¥{totalIncome:N2}";
            lblTotalExpense.Text = $"¥{totalExpense:N2}";
            lblBalance.Text = $"¥{balance:N2}";

            // 加载分类统计
            LoadCategoryStatistics();

            // 加载图表
            LoadChart();
        }

        private void LoadCategoryStatistics()
        {
            flowLayoutCategories.Controls.Clear();

            var transactions = _transactionService.GetTransactions();
            var expenseByCategory = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Amount)
                .ToList();

            var totalExpense = expenseByCategory.Sum(x => x.Amount);

            foreach (var category in expenseByCategory)
            {
                var categoryItem = CreateCategoryItem(category.Category, category.Amount, totalExpense);
                flowLayoutCategories.Controls.Add(categoryItem);
            }

            if (!expenseByCategory.Any())
            {
                var emptyLabel = new Label
                {
                    Text = "暂无支出数据",
                    ForeColor = Color.Gray,
                    Font = new Font("微软雅黑", 10),
                    Location = new Point(120, 80),
                    AutoSize = true
                };
                flowLayoutCategories.Controls.Add(emptyLabel);
            }
        }

        private Panel CreateCategoryItem(string category, decimal amount, decimal totalExpense)
        {
            var panel = new Panel
            {
                Size = new Size(360, 40),
                Margin = new Padding(0, 0, 0, 5)
            };

            var percentage = totalExpense > 0 ? (amount / totalExpense) * 100 : 0;

            var categoryLabel = new Label
            {
                Text = category,
                Location = new Point(10, 12),
                AutoSize = true,
                Font = new Font("微软雅黑", 10)
            };

            var amountLabel = new Label
            {
                Text = $"¥{amount:N2}",
                Location = new Point(200, 12),
                AutoSize = true,
                Font = new Font("微软雅黑", 10, FontStyle.Bold)
            };

            var percentageLabel = new Label
            {
                Text = $"{percentage:F1}%",
                Location = new Point(280, 12),
                AutoSize = true,
                Font = new Font("微软雅黑", 9),
                ForeColor = Color.Gray
            };

            // 进度条
            var progressBar = new Panel
            {
                Size = new Size(360, 6),
                Location = new Point(0, 34),
                BackColor = Color.LightGray
            };

            var progress = new Panel
            {
                Size = new Size((int)(360 * (percentage / 100)), 6),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(67, 97, 238)
            };

            progressBar.Controls.Add(progress);

            panel.Controls.Add(categoryLabel);
            panel.Controls.Add(amountLabel);
            panel.Controls.Add(percentageLabel);
            panel.Controls.Add(progressBar);

            return panel;
        }

        private void LoadChart()
        {
            chartPanel.Controls.Clear();

            // 简单的柱状图模拟
            var transactions = _transactionService.GetTransactions();
            var monthlyData = transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    Expense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                })
                .OrderBy(x => x.Month)
                .Take(6) // 最近6个月
                .ToList();

            if (!monthlyData.Any())
            {
                var emptyLabel = new Label
                {
                    Text = "暂无数据",
                    ForeColor = Color.Gray,
                    Font = new Font("微软雅黑", 10),
                    Location = new Point(150, 40),
                    AutoSize = true
                };
                chartPanel.Controls.Add(emptyLabel);
                return;
            }

            // 简单的柱状图绘制
            int barWidth = 30;
            int spacing = 10;
            int startX = 20;
            int baseY = 80;
            decimal maxAmount = Math.Max(monthlyData.Max(x => x.Income), monthlyData.Max(x => x.Expense));

            for (int i = 0; i < monthlyData.Count; i++)
            {
                var data = monthlyData[i];
                int x = startX + i * (barWidth * 2 + spacing);

                // 收入柱
                if (data.Income > 0)
                {
                    int incomeHeight = maxAmount > 0 ? (int)((data.Income / maxAmount) * 60) : 0;
                    var incomeBar = new Panel
                    {
                        BackColor = Color.Green,
                        Location = new Point(x, baseY - incomeHeight),
                        Size = new Size(barWidth, incomeHeight)
                    };
                    chartPanel.Controls.Add(incomeBar);
                }

                // 支出柱
                if (data.Expense > 0)
                {
                    int expenseHeight = maxAmount > 0 ? (int)((data.Expense / maxAmount) * 60) : 0;
                    var expenseBar = new Panel
                    {
                        BackColor = Color.Red,
                        Location = new Point(x + barWidth, baseY - expenseHeight),
                        Size = new Size(barWidth, expenseHeight)
                    };
                    chartPanel.Controls.Add(expenseBar);
                }

                // 月份标签
                var monthLabel = new Label
                {
                    Text = data.Month.ToString("MM"),
                    Location = new Point(x + barWidth - 10, baseY + 5),
                    AutoSize = true,
                    Font = new Font("微软雅黑", 8)
                };
                chartPanel.Controls.Add(monthLabel);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // StatisticsForm
            // 
            this.ClientSize = new System.Drawing.Size(400, 600);
            this.Name = "StatisticsForm";
            this.ResumeLayout(false);
        }
    }
}