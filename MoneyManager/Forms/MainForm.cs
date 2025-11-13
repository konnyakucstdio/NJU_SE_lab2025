using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MoneyManager.Models;
using MoneyManager.Services;

namespace MoneyManager.Forms
{
    public partial class MainForm : Form
    {
        private TransactionService _transactionService;
        private List<Account> _accounts;

        // UI控件
        private Panel headerPanel;
        private Label lblTotalAmount;
        private FlowLayoutPanel flowLayoutAccounts;
        private FlowLayoutPanel flowLayoutTransactions;
        private Panel panelBudgets;
        private Button btnAddTransaction;
        private Button btnStatistics;
        private Button btnViewAll;

        public MainForm()
        {
            InitializeComponent();
            InitializeServices();
            CreateUI();
            LoadData();
        }

        private void InitializeServices()
        {
            _transactionService = new TransactionService();
        }

        private void CreateUI()
        {
            // 窗体设置
            this.Text = "理财管家";
            this.Size = new Size(400, 800);  // 进一步增加高度
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.Padding = new Padding(0);

            CreateHeader();
            CreateAccountsSection();
            CreateTransactionsSection();
            CreateBudgetSection();
            CreateNavigation();
        }

        private void CreateHeader()
        {
            headerPanel = new Panel
            {
                BackColor = Color.FromArgb(67, 97, 238),
                Size = new Size(400, 180),  // 恢复为原来的高度
                Location = new Point(0, 0)
            };

            // 应用名称
            var lblAppName = new Label
            {
                Text = "💰 理财管家",
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            // 余额标签
            var lblBalanceLabel = new Label
            {
                Text = "总资产 (CNY)",
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 10),
                Location = new Point(20, 70),
                AutoSize = true
            };

            // 总金额
            lblTotalAmount = new Label
            {
                Text = "¥ 0.00",
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 20, FontStyle.Bold),
                Location = new Point(20, 95),
                AutoSize = true
            };

            // 快速操作按钮
            CreateQuickActions();

            headerPanel.Controls.Add(lblAppName);
            headerPanel.Controls.Add(lblBalanceLabel);
            headerPanel.Controls.Add(lblTotalAmount);
            this.Controls.Add(headerPanel);
        }

        private void CreateQuickActions()
        {
            var actionsPanel = new Panel
            {
                Size = new Size(360, 50),  // 减少高度
                Location = new Point(20, 140)  // 向上移动
            };

            var actions = new[]
            {
                new { Text = "记账", Icon = "➕", Handler = (EventHandler)btnAddTransaction_Click },
                new { Text = "报表", Icon = "📊", Handler = (EventHandler)btnStatistics_Click },
                new { Text = "预算", Icon = "🎯", Handler = (EventHandler)btnViewAll_Click },
                new { Text = "扫描", Icon = "📷", Handler = (EventHandler)btnScan_Click }
            };

            int x = 0;
            foreach (var action in actions)
            {
                var actionBtn = new Button
                {
                    Text = $"{action.Icon}\n{action.Text}",
                    Size = new Size(80, 45),  // 减少按钮高度
                    Location = new Point(x, 0),
                    BackColor = Color.Transparent,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("微软雅黑", 9)
                };
                actionBtn.FlatAppearance.BorderSize = 0;
                actionBtn.Click += action.Handler;

                actionsPanel.Controls.Add(actionBtn);
                x += 90;
            }

            headerPanel.Controls.Add(actionsPanel);
        }

        private void CreateAccountsSection()
        {
            var sectionLabel = new Label
            {
                Text = "我的账户",
                Font = new Font("微软雅黑", 12, FontStyle.Bold),
                Location = new Point(20, 200),  // 调整位置
                AutoSize = true
            };

            flowLayoutAccounts = new FlowLayoutPanel
            {
                Location = new Point(20, 230),
                Size = new Size(360, 70),  // 减少高度
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = false  // 禁用滚动，确保所有账户都能显示
            };

            this.Controls.Add(sectionLabel);
            this.Controls.Add(flowLayoutAccounts);
        }

        private void CreateTransactionsSection()
        {
            var sectionLabel = new Label
            {
                Text = "最近交易",
                Font = new Font("微软雅黑", 12, FontStyle.Bold),
                Location = new Point(20, 320),  // 调整位置，增加间距
                AutoSize = true
            };

            btnViewAll = new Button
            {
                Text = "查看全部",
                Size = new Size(80, 25),
                Location = new Point(300, 320),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Font = new Font("微软雅黑", 9)
            };
            btnViewAll.Click += btnViewAll_Click;

            flowLayoutTransactions = new FlowLayoutPanel
            {
                Location = new Point(20, 350),  // 调整位置
                Size = new Size(360, 180),
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };

            this.Controls.Add(sectionLabel);
            this.Controls.Add(btnViewAll);
            this.Controls.Add(flowLayoutTransactions);
        }

        private void CreateBudgetSection()
        {
            var sectionLabel = new Label
            {
                Text = "本月预算",
                Font = new Font("微软雅黑", 12, FontStyle.Bold),
                Location = new Point(20, 550),
                AutoSize = true
            };

            panelBudgets = new Panel
            {
                Location = new Point(20, 580),
                Size = new Size(360, 110), // 稍微增加高度
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10, 8, 10, 8) // 调整内边距
            };

            this.Controls.Add(sectionLabel);
            this.Controls.Add(panelBudgets);
        }

        private void CreateNavigation()
        {
            var navPanel = new Panel
            {
                BackColor = Color.White,
                Location = new Point(0, 700),  // 调整位置，确保不与预算重叠
                Size = new Size(400, 60),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnAddTransaction = new Button
            {
                Text = "记账",
                Size = new Size(80, 40),
                Location = new Point(20, 10),
                BackColor = Color.FromArgb(67, 97, 238),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10)
            };
            btnAddTransaction.Click += btnAddTransaction_Click;

            btnStatistics = new Button
            {
                Text = "统计",
                Size = new Size(80, 40),
                Location = new Point(120, 10),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10)
            };
            btnStatistics.Click += btnStatistics_Click;

            var btnAccounts = new Button
            {
                Text = "账户",
                Size = new Size(80, 40),
                Location = new Point(220, 10),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10)
            };
            btnAccounts.Click += btnAccounts_Click;

            navPanel.Controls.Add(btnAddTransaction);
            navPanel.Controls.Add(btnStatistics);
            navPanel.Controls.Add(btnAccounts);
            this.Controls.Add(navPanel);
        }

        private void LoadData()
        {
            // 初始化账户数据
            _accounts = new List<Account>
            {
                new Account { Name = "支付宝", Balance = 12458.60m, Icon = "💰", Color = "#4f5bd5" },
                new Account { Name = "微信", Balance = 3245.80m, Icon = "💬", Color = "#1aad19" },
                new Account { Name = "工商银行", Balance = 65328.42m, Icon = "🏦", Color = "#c30d23" }
            };

            UpdateDashboard();
        }

        private void UpdateDashboard()
        {
            // 更新总资产
            var totalBalance = _accounts.Sum(a => a.Balance) + _transactionService.GetBalance();
            lblTotalAmount.Text = $"¥ {totalBalance:N2}";

            // 更新账户显示
            UpdateAccountsDisplay();

            // 更新交易列表
            UpdateTransactionsDisplay();

            // 更新预算显示
            UpdateBudgetsDisplay();
        }

        private void UpdateAccountsDisplay()
        {
            flowLayoutAccounts.Controls.Clear();

            foreach (var account in _accounts)
            {
                var accountCard = CreateAccountCard(account);
                flowLayoutAccounts.Controls.Add(accountCard);
            }
        }

        private Panel CreateAccountCard(Account account)
        {
            var panel = new Panel
            {
                Size = new Size(100, 60),  // 减小卡片大小
                BackColor = Color.FromArgb(248, 249, 250),
                Margin = new Padding(5),  // 减小外边距
                Padding = new Padding(5)  // 减小内边距
            };

            var iconLabel = new Label
            {
                Text = account.Icon,
                Font = new Font("微软雅黑", 14),
                Location = new Point(35, 5),  // 调整位置
                AutoSize = true
            };

            var nameLabel = new Label
            {
                Text = account.Name,
                Font = new Font("微软雅黑", 8),
                Location = new Point(25, 30),  // 调整位置
                AutoSize = true
            };

            var balanceLabel = new Label
            {
                Text = $"¥{account.Balance:N0}",
                Font = new Font("微软雅黑", 9, FontStyle.Bold),
                Location = new Point(20, 45),  // 调整位置
                AutoSize = true
            };

            panel.Controls.Add(iconLabel);
            panel.Controls.Add(nameLabel);
            panel.Controls.Add(balanceLabel);

            return panel;
        }

        private void UpdateTransactionsDisplay()
        {
            flowLayoutTransactions.Controls.Clear();

            var transactions = _transactionService.GetTransactions().Take(5);

            foreach (var transaction in transactions)
            {
                var transactionItem = CreateTransactionItem(transaction);
                flowLayoutTransactions.Controls.Add(transactionItem);
            }

            if (!transactions.Any())
            {
                var emptyLabel = new Label
                {
                    Text = "暂无交易记录",
                    ForeColor = Color.Gray,
                    Font = new Font("微软雅黑", 10),
                    Location = new Point(120, 80),
                    AutoSize = true
                };
                flowLayoutTransactions.Controls.Add(emptyLabel);
            }
        }

        private Panel CreateTransactionItem(Transaction transaction)
        {
            var panel = new Panel
            {
                Size = new Size(340, 50),
                Margin = new Padding(5),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            var iconLabel = new Label
            {
                Text = transaction.Type == TransactionType.Income ? "📥" : "📤",
                Font = new Font("微软雅黑", 12),
                Location = new Point(10, 15),
                AutoSize = true
            };

            var titleLabel = new Label
            {
                Text = transaction.Title,
                Font = new Font("微软雅黑", 9, FontStyle.Bold),
                Location = new Point(50, 10),
                AutoSize = true
            };

            var infoLabel = new Label
            {
                Text = $"{transaction.Category} • {transaction.Date:MM-dd HH:mm}",
                Font = new Font("微软雅黑", 8),
                ForeColor = Color.Gray,
                Location = new Point(50, 25),
                AutoSize = true
            };

            var amountLabel = new Label
            {
                Text = $"{(transaction.Type == TransactionType.Income ? "+" : "-")} ¥{transaction.Amount:N2}",
                Font = new Font("微软雅黑", 9, FontStyle.Bold),
                Location = new Point(250, 15),
                AutoSize = true,
                ForeColor = transaction.Type == TransactionType.Income ? Color.Green : Color.Red
            };

            // 添加点击事件
            panel.Click += (s, e) => ShowTransactionDetails(transaction);

            panel.Controls.Add(iconLabel);
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(infoLabel);
            panel.Controls.Add(amountLabel);

            return panel;
        }

        private void UpdateBudgetsDisplay()
        {
            panelBudgets.Controls.Clear();

            var budgets = _transactionService.GetBudgets();
            int y = 5; // 从更小的位置开始

            foreach (var budget in budgets)
            {
                var budgetItem = CreateBudgetItem(budget, y);
                panelBudgets.Controls.Add(budgetItem);
                y += 26; // 增加间距
            }
        }

        private Panel CreateBudgetItem(Budget budget, int y)
        {
            var panel = new Panel
            {
                Size = new Size(340, 22), // 稍微增加高度
                Location = new Point(0, y) // 从左侧开始
            };

            var categoryLabel = new Label
            {
                Text = budget.Category,
                Font = new Font("微软雅黑", 9),
                Location = new Point(0, 3), // 调整垂直位置
                AutoSize = true
            };

            var amountLabel = new Label
            {
                Text = $"¥{budget.UsedAmount:N0} / ¥{budget.TotalAmount:N0}",
                Font = new Font("微软雅黑", 9),
                Location = new Point(180, 3), // 调整位置
                AutoSize = true
            };

            var progressBar = new Panel
            {
                Size = new Size(340, 8), // 进度条宽度与面板相同
                Location = new Point(0, 16), // 调整位置
                BackColor = Color.LightGray
            };

            // 确保百分比不超过100%
            var percentage = Math.Min(budget.Percentage, 100);
            var progress = new Panel
            {
                Size = new Size((int)(340 * (percentage / 100)), 8),
                Location = new Point(0, 0),
                BackColor = percentage > 80 ? Color.Red : percentage > 50 ? Color.Orange : Color.Green
            };

            progressBar.Controls.Add(progress);

            panel.Controls.Add(categoryLabel);
            panel.Controls.Add(amountLabel);
            panel.Controls.Add(progressBar);

            return panel;
        }

        // 事件处理方法
        private void btnAddTransaction_Click(object sender, EventArgs e)
        {
            var addForm = new AddTransactionForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                _transactionService.AddTransaction(addForm.NewTransaction);
                UpdateDashboard();
            }
        }

        private void btnStatistics_Click(object sender, EventArgs e)
        {
            var statsForm = new StatisticsForm(_transactionService);
            statsForm.ShowDialog();
        }

        private void btnViewAll_Click(object sender, EventArgs e)
        {
            var allTransactionsForm = new AllTransactionsForm(_transactionService);
            allTransactionsForm.ShowDialog();
            UpdateDashboard();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            MessageBox.Show("扫描功能开发中...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnAccounts_Click(object sender, EventArgs e)
        {
            MessageBox.Show("账户管理功能开发中...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowTransactionDetails(Transaction transaction)
        {
            var details = $"标题: {transaction.Title}\n" +
                         $"金额: ¥{transaction.Amount:N2}\n" +
                         $"类型: {(transaction.Type == TransactionType.Income ? "收入" : "支出")}\n" +
                         $"分类: {transaction.Category}\n" +
                         $"日期: {transaction.Date:yyyy-MM-dd HH:mm}\n" +
                         $"账户: {transaction.Account}";

            var result = MessageBox.Show(details + "\n\n是否要删除这条记录？", "交易详情",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                _transactionService.DeleteTransaction(transaction.Id);
                UpdateDashboard();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(400, 800);  // 更新窗体大小
            this.Name = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 可以在这里添加初始化代码
        }
    }
}