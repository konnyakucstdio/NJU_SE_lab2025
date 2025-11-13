using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MoneyManager.Services;

namespace MoneyManager.Forms
{
    public partial class AllTransactionsForm : Form
    {
        private TransactionService _transactionService;
        private FlowLayoutPanel flowLayoutAllTransactions;

        public AllTransactionsForm(TransactionService transactionService)
        {
            _transactionService = transactionService;
            InitializeComponent();
            CreateUI();
            LoadAllTransactions();
        }

        private void CreateUI()
        {
            this.Text = "所有交易记录";
            this.Size = new Size(400, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.Padding = new Padding(10);

            var titleLabel = new Label
            {
                Text = "所有交易记录",
                Font = new Font("微软雅黑", 14, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            flowLayoutAllTransactions = new FlowLayoutPanel
            {
                Location = new Point(10, 50),
                Size = new Size(380, 500),
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true
            };

            var btnClose = new Button
            {
                Text = "关闭",
                Location = new Point(150, 560),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(67, 97, 238),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 10)
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(titleLabel);
            this.Controls.Add(flowLayoutAllTransactions);
            this.Controls.Add(btnClose);
        }

        private void LoadAllTransactions()
        {
            flowLayoutAllTransactions.Controls.Clear();

            var transactions = _transactionService.GetTransactions();

            if (!transactions.Any())
            {
                var emptyLabel = new Label
                {
                    Text = "暂无交易记录",
                    ForeColor = Color.Gray,
                    Font = new Font("微软雅黑", 12),
                    Location = new Point(120, 200),
                    AutoSize = true
                };
                flowLayoutAllTransactions.Controls.Add(emptyLabel);
                return;
            }

            foreach (var transaction in transactions)
            {
                var transactionItem = CreateTransactionItem(transaction);
                flowLayoutAllTransactions.Controls.Add(transactionItem);
            }
        }

        private Panel CreateTransactionItem(Models.Transaction transaction)
        {
            var panel = new Panel
            {
                Size = new Size(360, 60),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            var iconLabel = new Label
            {
                Text = transaction.Type == Models.TransactionType.Income ? "📥" : "📤",
                Font = new Font("微软雅黑", 14),
                Location = new Point(10, 20),
                AutoSize = true
            };

            var titleLabel = new Label
            {
                Text = transaction.Title,
                Font = new Font("微软雅黑", 10, FontStyle.Bold),
                Location = new Point(50, 10),
                AutoSize = true
            };

            var infoLabel = new Label
            {
                Text = $"{transaction.Category} • {transaction.Date:yyyy-MM-dd HH:mm}",
                Font = new Font("微软雅黑", 8),
                ForeColor = Color.Gray,
                Location = new Point(50, 30),
                AutoSize = true
            };

            var amountLabel = new Label
            {
                Text = $"{(transaction.Type == Models.TransactionType.Income ? "+" : "-")} ¥{transaction.Amount:N2}",
                Font = new Font("微软雅黑", 11, FontStyle.Bold),
                Location = new Point(250, 20),
                AutoSize = true,
                ForeColor = transaction.Type == Models.TransactionType.Income ? Color.Green : Color.Red
            };

            // 删除按钮
            var btnDelete = new Button
            {
                Text = "删除",
                Size = new Size(50, 25),
                Location = new Point(300, 5),
                BackColor = Color.LightCoral,
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 8),
                Tag = transaction.Id
            };
            btnDelete.Click += (s, e) =>
            {
                var result = MessageBox.Show("确定要删除这条记录吗？", "确认删除",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _transactionService.DeleteTransaction(transaction.Id);
                    LoadAllTransactions(); // 刷新列表
                }
            };

            panel.Controls.Add(iconLabel);
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(infoLabel);
            panel.Controls.Add(amountLabel);
            panel.Controls.Add(btnDelete);

            return panel;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AllTransactionsForm
            // 
            this.ClientSize = new System.Drawing.Size(400, 600);
            this.Name = "AllTransactionsForm";
            this.Load += new System.EventHandler(this.AllTransactionsForm_Load);
            this.ResumeLayout(false);

        }

        private void AllTransactionsForm_Load(object sender, EventArgs e)
        {

        }
    }
}