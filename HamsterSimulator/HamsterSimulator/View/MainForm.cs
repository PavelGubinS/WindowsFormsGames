using System;
using System.Drawing;
using System.Windows.Forms;
using HamsterSimulator.Controller;

namespace HamsterSimulator.View
{
    public partial class MainForm : Form
    {
        private GameController _controller;
        private Timer _gameTimer;
        private Timer _animationTimer;
        private DateTime _animationStartTime;
        private bool _isAnimating = false;
        private int _buttonClickCount = 0;

        public MainForm()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            _controller = new GameController(this);

            _gameTimer = new Timer();
            _gameTimer.Interval = 100;
            _gameTimer.Tick += Timer_Tick;
            _gameTimer.Start();

            _animationTimer = new Timer();
            _animationTimer.Interval = 100;
            _animationTimer.Tick += AnimationTimer_Tick;

            btnAction.Click += BtnAction_Click;
            btnLoan.Click += BtnLoan_Click;
            btnMicroLoan.Click += BtnMicroLoan_Click;
            btnRigging.Click += BtnRigging_Click; // новый обработчик
            this.KeyDown += MainForm_KeyDown;
            this.KeyPreview = true;

            UpdateUI();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_isAnimating)
                UpdateUI();
        }

        public void UpdateUI()
        {
            if (lblBalance == null || lblNumbers == null || lblGameOver == null ||
                lblLoanCount == null || lblMicroLoanCount == null || lblPenalty == null ||
                lblRiggingUses == null) return;

            lblBalance.Text = $"Баланс: {_controller.Balance}";
            lblLoanCount.Text = $"Займы: {_controller.LoanCount}/3";
            lblMicroLoanCount.Text = $"Микрозаймы: {_controller.MicroLoanCount}/5";
            lblPenalty.Text = $"Штраф за долги: {_controller.SpinPenalty}";
            lblRiggingUses.Text = $"Подкрутка: {_controller.RiggingUsesLeft}/3";

            if (!_isAnimating && _controller.CurrentNumbers != null)
            {
                lblNumbers.Text = string.Join(" ", _controller.CurrentNumbers);
            }

            if (_controller.IsGameOver)
            {
                lblGameOver.Text = _controller.GameOverMessage;
                lblGameOver.Visible = true;
                btnAction.Enabled = false;
                btnLoan.Enabled = false;
                btnMicroLoan.Enabled = false;
                btnRigging.Enabled = false;
            }
            else
            {
                lblGameOver.Visible = false;

                bool canSpin = !_isAnimating && _controller.Balance >= _controller.TotalSpinCost;
                bool canTakeLoan = !_isAnimating && _controller.LoanCount < 3;
                bool canTakeMicroLoan = !_isAnimating && _controller.MicroLoanCount < 5;
                bool canUseRigging = !_isAnimating && _controller.CanUseRigging;

                btnAction.Enabled = canSpin;
                btnLoan.Enabled = canTakeLoan;
                btnMicroLoan.Enabled = canTakeMicroLoan;
                btnRigging.Enabled = canUseRigging;
            }

            string[] buttonTexts = { "ДЕП", "ДОДЕП", "ЛАСТ ДЕП" };
            btnAction.Text = buttonTexts[_buttonClickCount % 3];
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            Random rand = new Random();
            int[] tempNumbers = new int[7];
            for (int i = 0; i < 7; i++)
                tempNumbers[i] = rand.Next(0, 10);
            lblNumbers.Text = string.Join(" ", tempNumbers);

            if ((DateTime.Now - _animationStartTime).TotalSeconds >= 3)
            {
                _animationTimer.Stop();
                _isAnimating = false;

                _controller.Spin();
                UpdateUI();
            }
        }

        private void BtnAction_Click(object sender, EventArgs e)
        {
            if (_controller.IsGameOver || _isAnimating) return;
            if (_controller.Balance < _controller.TotalSpinCost) return;

            _buttonClickCount++;

            btnAction.Enabled = false;
            btnLoan.Enabled = false;
            btnMicroLoan.Enabled = false;
            btnRigging.Enabled = false;

            _isAnimating = true;
            _animationStartTime = DateTime.Now;
            _animationTimer.Start();

            Random rand = new Random();
            int[] tempNumbers = new int[7];
            for (int i = 0; i < 7; i++)
                tempNumbers[i] = rand.Next(0, 10);
            lblNumbers.Text = string.Join(" ", tempNumbers);
        }

        private void BtnLoan_Click(object sender, EventArgs e)
        {
            if (_controller.IsGameOver || _isAnimating) return;
            _controller.TakeLoan();
            UpdateUI();
        }

        private void BtnMicroLoan_Click(object sender, EventArgs e)
        {
            if (_controller.IsGameOver || _isAnimating) return;
            _controller.TakeMicroLoan();
            UpdateUI();
        }

        // Новый обработчик подкрутки
        private void BtnRigging_Click(object sender, EventArgs e)
        {
            if (_controller.IsGameOver || _isAnimating) return;
            _controller.UseRigging();
            UpdateUI();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                if (_isAnimating)
                {
                    _animationTimer.Stop();
                    _isAnimating = false;
                    UpdateUI();
                }

                if (_controller.IsGameOver)
                {
                    _controller.ResetGame();
                    _buttonClickCount = 0;
                    UpdateUI();
                }
            }
        }

        private void InitializeComponent()
        {
            this.btnAction = new System.Windows.Forms.Button();
            this.btnLoan = new System.Windows.Forms.Button();
            this.btnMicroLoan = new System.Windows.Forms.Button();
            this.btnRigging = new System.Windows.Forms.Button();
            this.lblBalance = new System.Windows.Forms.Label();
            this.lblNumbers = new System.Windows.Forms.Label();
            this.lblGameOver = new System.Windows.Forms.Label();
            this.lblLoanCount = new System.Windows.Forms.Label();
            this.lblMicroLoanCount = new System.Windows.Forms.Label();
            this.lblPenalty = new System.Windows.Forms.Label();
            this.lblRiggingUses = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnAction
            // 
            this.btnAction.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.btnAction.Location = new System.Drawing.Point(589, 329);
            this.btnAction.Name = "btnAction";
            this.btnAction.Size = new System.Drawing.Size(248, 50);
            this.btnAction.TabIndex = 0;
            this.btnAction.Text = "ДЕП";
            this.btnAction.UseVisualStyleBackColor = true;
            // 
            // btnLoan
            // 
            this.btnLoan.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.btnLoan.Location = new System.Drawing.Point(787, 400);
            this.btnLoan.Name = "btnLoan";
            this.btnLoan.Size = new System.Drawing.Size(167, 50);
            this.btnLoan.TabIndex = 1;
            this.btnLoan.Text = "Займ";
            this.btnLoan.UseVisualStyleBackColor = true;
            // 
            // btnMicroLoan
            // 
            this.btnMicroLoan.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.btnMicroLoan.Location = new System.Drawing.Point(458, 400);
            this.btnMicroLoan.Name = "btnMicroLoan";
            this.btnMicroLoan.Size = new System.Drawing.Size(178, 50);
            this.btnMicroLoan.TabIndex = 5;
            this.btnMicroLoan.Text = "Микрозайм";
            this.btnMicroLoan.UseVisualStyleBackColor = true;
            // 
            // btnRigging
            // 
            this.btnRigging.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.btnRigging.Location = new System.Drawing.Point(1327, 12);
            this.btnRigging.Name = "btnRigging";
            this.btnRigging.Size = new System.Drawing.Size(129, 45);
            this.btnRigging.TabIndex = 9;
            this.btnRigging.Text = "Подкрутка";
            this.btnRigging.UseVisualStyleBackColor = true;
            // 
            // lblBalance
            // 
            this.lblBalance.AutoSize = true;
            this.lblBalance.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblBalance.Location = new System.Drawing.Point(650, 60);
            this.lblBalance.Name = "lblBalance";
            this.lblBalance.Size = new System.Drawing.Size(130, 29);
            this.lblBalance.TabIndex = 2;
            this.lblBalance.Text = "Баланс: 0";
            // 
            // lblNumbers
            // 
            this.lblNumbers.AutoSize = true;
            this.lblNumbers.Font = new System.Drawing.Font("Courier New", 26F, System.Drawing.FontStyle.Bold);
            this.lblNumbers.Location = new System.Drawing.Point(540, 258);
            this.lblNumbers.Name = "lblNumbers";
            this.lblNumbers.Size = new System.Drawing.Size(360, 50);
            this.lblNumbers.TabIndex = 3;
            this.lblNumbers.Text = "0 0 0 0 0 0 0";
            // 
            // lblGameOver
            // 
            this.lblGameOver.AutoSize = true;
            this.lblGameOver.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold);
            this.lblGameOver.ForeColor = System.Drawing.Color.Red;
            this.lblGameOver.Location = new System.Drawing.Point(520, 550);
            this.lblGameOver.Name = "lblGameOver";
            this.lblGameOver.Size = new System.Drawing.Size(329, 31);
            this.lblGameOver.TabIndex = 4;
            this.lblGameOver.Text = "Ты всё слил в нулину...";
            this.lblGameOver.Visible = false;
            // 
            // lblLoanCount
            // 
            this.lblLoanCount.AutoSize = true;
            this.lblLoanCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblLoanCount.Location = new System.Drawing.Point(650, 100);
            this.lblLoanCount.Name = "lblLoanCount";
            this.lblLoanCount.Size = new System.Drawing.Size(116, 25);
            this.lblLoanCount.TabIndex = 6;
            this.lblLoanCount.Text = "Займы: 0/3";
            // 
            // lblMicroLoanCount
            // 
            this.lblMicroLoanCount.AutoSize = true;
            this.lblMicroLoanCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblMicroLoanCount.Location = new System.Drawing.Point(650, 130);
            this.lblMicroLoanCount.Name = "lblMicroLoanCount";
            this.lblMicroLoanCount.Size = new System.Drawing.Size(173, 25);
            this.lblMicroLoanCount.TabIndex = 7;
            this.lblMicroLoanCount.Text = "Микрозаймы: 0/5";
            // 
            // lblPenalty
            // 
            this.lblPenalty.AutoSize = true;
            this.lblPenalty.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblPenalty.ForeColor = System.Drawing.Color.DarkRed;
            this.lblPenalty.Location = new System.Drawing.Point(650, 160);
            this.lblPenalty.Name = "lblPenalty";
            this.lblPenalty.Size = new System.Drawing.Size(187, 25);
            this.lblPenalty.TabIndex = 8;
            this.lblPenalty.Text = "Штраф за долги: 0";
            // 
            // lblRiggingUses
            // 
            this.lblRiggingUses.AutoSize = true;
            this.lblRiggingUses.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblRiggingUses.Location = new System.Drawing.Point(1323, 60);
            this.lblRiggingUses.Name = "lblRiggingUses";
            this.lblRiggingUses.Size = new System.Drawing.Size(133, 20);
            this.lblRiggingUses.TabIndex = 10;
            this.lblRiggingUses.Text = "Подкрутка: 3/3";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1468, 707);
            this.Controls.Add(this.lblRiggingUses);
            this.Controls.Add(this.btnRigging);
            this.Controls.Add(this.lblPenalty);
            this.Controls.Add(this.lblMicroLoanCount);
            this.Controls.Add(this.lblLoanCount);
            this.Controls.Add(this.btnMicroLoan);
            this.Controls.Add(this.lblGameOver);
            this.Controls.Add(this.lblNumbers);
            this.Controls.Add(this.lblBalance);
            this.Controls.Add(this.btnLoan);
            this.Controls.Add(this.btnAction);
            this.Name = "MainForm";
            this.Text = "Симулятор Лудика";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Button btnAction;
        private System.Windows.Forms.Button btnLoan;
        private System.Windows.Forms.Button btnMicroLoan;
        private System.Windows.Forms.Button btnRigging;
        private System.Windows.Forms.Label lblBalance;
        private System.Windows.Forms.Label lblNumbers;
        private System.Windows.Forms.Label lblGameOver;
        private System.Windows.Forms.Label lblLoanCount;
        private System.Windows.Forms.Label lblMicroLoanCount;
        private System.Windows.Forms.Label lblPenalty;
        private System.Windows.Forms.Label lblRiggingUses;
    }
}