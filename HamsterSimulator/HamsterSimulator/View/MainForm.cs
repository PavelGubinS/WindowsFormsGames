using System;
using System.Drawing;
using System.Windows.Forms;
using HamsterSimulator.Model;

namespace HamsterSimulator.View
{
    public partial class MainForm : Form
    {
        private GameState _gameState;
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
            _gameState = new GameState();

            _gameTimer = new Timer();
            _gameTimer.Interval = 100;
            _gameTimer.Tick += Timer_Tick;
            _gameTimer.Start();

            _animationTimer = new Timer();
            _animationTimer.Interval = 100;
            _animationTimer.Tick += AnimationTimer_Tick;

            btnAction.Click += BtnAction_Click;
            btnLoan.Click += BtnLoan_Click;
            this.KeyDown += MainForm_KeyDown;
            this.KeyPreview = true;

            UpdateUI();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_isAnimating)
                UpdateUI();
        }

        private void UpdateUI()
        {
            if (lblBalance == null || lblNumbers == null || lblGameOver == null) return;

            lblBalance.Text = $"Баланс: {_gameState.Balance}";

            if (!_isAnimating && _gameState.CurrentNumbers != null)
            {
                lblNumbers.Text = string.Join(" ", _gameState.CurrentNumbers);
            }

            if (_gameState.IsGameOver)
            {
                lblGameOver.Text = _gameState.GameOverMessage;
                lblGameOver.Visible = true;
                btnAction.Enabled = false;
                btnLoan.Enabled = false;
            }
            else
            {
                lblGameOver.Visible = false;
                // Кнопка ДЕП доступна, если есть минимум 10 монет и не идёт анимация
                btnAction.Enabled = !_isAnimating && _gameState.Balance >= 10;
                // Кнопка займа доступна, если не идёт анимация и можно взять займ (модель сама проверит LoanCount)
                btnLoan.Enabled = !_isAnimating;
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

            // Анимация длится 3 секунды
            if ((DateTime.Now - _animationStartTime).TotalSeconds >= 3)
            {
                _animationTimer.Stop();
                _isAnimating = false;

                // После анимации выполняем настоящий спин
                _gameState.Spin();
                UpdateUI(); // обновим интерфейс (включая баланс и цифры)
            }
        }

        private void BtnAction_Click(object sender, EventArgs e)
        {
            if (_gameState.IsGameOver || _isAnimating) return;
            if (_gameState.Balance < 10) return; // дополнительная проверка

            _buttonClickCount++;

            // Блокируем кнопки на время анимации
            btnAction.Enabled = false;
            btnLoan.Enabled = false;

            _isAnimating = true;
            _animationStartTime = DateTime.Now;
            _animationTimer.Start();

            // Сразу показываем случайные цифры
            Random rand = new Random();
            int[] tempNumbers = new int[7];
            for (int i = 0; i < 7; i++)
                tempNumbers[i] = rand.Next(0, 10);
            lblNumbers.Text = string.Join(" ", tempNumbers);
        }

        private void BtnLoan_Click(object sender, EventArgs e)
        {
            if (_gameState.IsGameOver || _isAnimating) return;

            _gameState.TakeLoan();
            UpdateUI(); // обновим баланс и доступность кнопок
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                if (_isAnimating)
                {
                    _animationTimer.Stop();
                    _isAnimating = false;
                }

                if (_gameState.IsGameOver)
                {
                    _gameState.ResetGame();
                    _buttonClickCount = 0;
                    UpdateUI();
                }
                else
                {
                    // Если игра не окончена, просто обновим UI (может быть, баланс поменялся)
                    UpdateUI();
                }
            }
        }

        private void InitializeComponent()
        {
            this.btnAction = new System.Windows.Forms.Button();
            this.btnLoan = new System.Windows.Forms.Button();
            this.lblBalance = new System.Windows.Forms.Label();
            this.lblNumbers = new System.Windows.Forms.Label();
            this.lblGameOver = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnAction
            // 
            this.btnAction.Location = new System.Drawing.Point(468, 341);
            this.btnAction.Name = "btnAction";
            this.btnAction.Size = new System.Drawing.Size(179, 88);
            this.btnAction.TabIndex = 0;
            this.btnAction.Text = "ДЕП";
            this.btnAction.UseVisualStyleBackColor = true;
            // 
            // btnLoan
            // 
            this.btnLoan.Location = new System.Drawing.Point(764, 341);
            this.btnLoan.Name = "btnLoan";
            this.btnLoan.Size = new System.Drawing.Size(178, 88);
            this.btnLoan.TabIndex = 1;
            this.btnLoan.Text = "Займ";
            this.btnLoan.UseVisualStyleBackColor = true;
            // 
            // lblBalance
            // 
            this.lblBalance.AutoSize = true;
            this.lblBalance.Font = new System.Drawing.Font("Microsoft Sans Serif", 35F);
            this.lblBalance.Location = new System.Drawing.Point(541, 140);
            this.lblBalance.Name = "lblBalance";
            this.lblBalance.Size = new System.Drawing.Size(296, 67);
            this.lblBalance.TabIndex = 2;
            this.lblBalance.Text = "Баланс: 0";
            // 
            // lblNumbers
            // 
            this.lblNumbers.AutoSize = true;
            this.lblNumbers.Font = new System.Drawing.Font("Courier New", 40F, System.Drawing.FontStyle.Bold);
            this.lblNumbers.Location = new System.Drawing.Point(437, 241);
            this.lblNumbers.Name = "lblNumbers";
            this.lblNumbers.Size = new System.Drawing.Size(552, 76);
            this.lblNumbers.TabIndex = 3;
            this.lblNumbers.Text = "0 0 0 0 0 0 0";
            // 
            // lblGameOver
            // 
            this.lblGameOver.AutoSize = true;
            this.lblGameOver.Font = new System.Drawing.Font("Microsoft Sans Serif", 25F, System.Drawing.FontStyle.Bold);
            this.lblGameOver.ForeColor = System.Drawing.Color.Red;
            this.lblGameOver.Location = new System.Drawing.Point(460, 471);
            this.lblGameOver.Name = "lblGameOver";
            this.lblGameOver.Size = new System.Drawing.Size(494, 48);
            this.lblGameOver.TabIndex = 4;
            this.lblGameOver.Text = "Ты всё слил в нулину...";
            this.lblGameOver.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1468, 707);
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
        private System.Windows.Forms.Label lblBalance;
        private System.Windows.Forms.Label lblNumbers;
        private System.Windows.Forms.Label lblGameOver;
    }
}