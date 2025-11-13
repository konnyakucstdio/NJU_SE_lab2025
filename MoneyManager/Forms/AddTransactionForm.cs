using System;
using System.Drawing;
using System.Windows.Forms;
using MoneyManager.Models;

namespace MoneyManager.Forms
{
    public partial class AddTransactionForm : Form
    {
        public Transaction NewTransaction { get; private set; }

        private ComboBox cmbType;
        private TextBox txtTitle;
        private NumericUpDown numAmount;
        private ComboBox cmbCategory;
        private DateTimePicker dtpDate;
        private TextBox txtDescription;
        private Button btnSave;
        private Button btnCancel;

        public AddTransactionForm()
        {
            InitializeComponent();
            CreateForm();
        }

        private void CreateForm()
        {
            this.Text = "添加交易";
            this.Size = new Size(350, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 类型选择
            var lblType = new Label
            {
                Text = "类型:",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("微软雅黑", 10)
            };

            cmbType = new ComboBox
            {
                Location = new Point(100, 20),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("微软雅黑", 10)
            };
            cmbType.Items.AddRange(new[] { "收入", "支出" });
            cmbType.SelectedIndex = 1;

            // 标题输入
            var lblTitle = new Label
            {
                Text = "标题:",
                Location = new Point(20, 60),
                AutoSize = true,
                Font = new Font("微软雅黑", 10)
            };

            txtTitle = new TextBox
            {
                Location = new Point(100, 60),
                Size = new Size(200, 25),
                Font = new Font("微软雅黑", 10)
            };

            // 金额输入
            var lblAmount = new Label
            {
                Text = "金额:",
                Location = new Point(20, 100),
                AutoSize = true,
                Font = new Font("微软雅黑", 10)
            };

            numAmount = new NumericUpDown
            {
                Location = new Point(100, 100),
                Size = new Size(200, 25),
                DecimalPlaces = 2,
                Maximum = 1000000,
                Font = new Font("微软雅黑", 10)
            };

            // 分类选择
            var lblCategory = new Label
            {
                Text = "分类:",
                Location = new Point(20, 140),
                AutoSize = true,
                Font = new Font("微软雅黑", 10)
            };

            cmbCategory = new ComboBox
            {
                Location = new Point(100, 140),
                Size = new Size(200, 25),
                Font = new Font("微软雅黑", 10)
            };
            cmbCategory.Items.AddRange(new[] { "餐饮", "购物", "交通", "娱乐", "工资", "投资", "医疗", "教育", "其他" });
            cmbCategory.SelectedIndex = 0;

            // 日期选择
            var lblDate = new Label
            {
                Text = "日期:",
                Location = new Point(20, 180),
                AutoSize = true,
                Font = new Font("微软雅黑", 10)
            };

            dtpDate = new DateTimePicker
            {
                Location = new Point(100, 180),
                Size = new Size(200, 25),
                Value = DateTime.Now,
                Font = new Font("微软雅黑", 10)
            };

            // 描述输入
            var lblDescription = new Label
            {
                Text = "描述:",
                Location = new Point(20, 220),
                AutoSize = true,
                Font = new Font("微软雅黑", 10)
            };

            txtDescription = new TextBox
            {
                Location = new Point(100, 220),
                Size = new Size(200, 60),
                Multiline = true,
                Font = new Font("微软雅黑", 10)
            };

            // 按钮
            btnSave = new Button
            {
                Text = "保存",
                Location = new Point(80, 300),
                Size = new Size(80, 35),
                BackColor = Color.FromArgb(67, 97, 238),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 10),
                DialogResult = DialogResult.OK
            };
            btnSave.Click += btnSave_Click;

            btnCancel = new Button
            {
                Text = "取消",
                Location = new Point(180, 300),
                Size = new Size(80, 35),
                BackColor = Color.LightGray,
                Font = new Font("微软雅黑", 10),
                DialogResult = DialogResult.Cancel
            };

            // 添加控件到窗体
            this.Controls.AddRange(new Control[]
            {
                lblType, cmbType,
                lblTitle, txtTitle,
                lblAmount, numAmount,
                lblCategory, cmbCategory,
                lblDate, dtpDate,
                lblDescription, txtDescription,
                btnSave, btnCancel
            });

            // 设置AcceptButton和CancelButton
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTitle.Text))
            {
                MessageBox.Show("请输入交易标题！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTitle.Focus();
                return;
            }

            if (numAmount.Value <= 0)
            {
                MessageBox.Show("请输入有效的金额！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numAmount.Focus();
                return;
            }

            NewTransaction = new Transaction
            {
                Title = txtTitle.Text,
                Amount = numAmount.Value,
                Type = cmbType.SelectedIndex == 0 ? TransactionType.Income : TransactionType.Expense,
                Category = cmbCategory.Text,
                Date = dtpDate.Value,
                Description = txtDescription.Text
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AddTransactionForm
            // 
            this.ClientSize = new System.Drawing.Size(350, 400);
            this.Name = "AddTransactionForm";
            this.Load += new System.EventHandler(this.AddTransactionForm_Load);
            this.ResumeLayout(false);

        }

        private void AddTransactionForm_Load(object sender, EventArgs e)
        {

        }
    }
}