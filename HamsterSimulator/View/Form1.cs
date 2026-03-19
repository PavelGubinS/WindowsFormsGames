using System;
using System.Drawing;
using System.Windows.Forms;
using HamsterSimulator.Model; // Ссылка на модель

namespace HamsterSimulator.View
{
    public partial class MainForm : Form
    {
        // Модель игры (не знает о форме)
        private GameState _gameState;

        // Контроллер (обработка действий пользователя) встроен в форму,
        // но логика перенаправляется в модель.
        private Timer _gameTimer;
        private int _buttonClickCount = 0; // Для смены текста кнопки

        public MainForm()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            _gameState = new GameState();

            // Настройка таймера для перерисовки (хотя у нас статичная игра, можно реже)
            _gameTimer = new Timer();
            _gameTimer.Interval = 100; // 100 мс
            _gameTimer.Tick += Timer_Tick;
            _gameTimer.Start();

            // Подписка на события кнопок
            btnAction.Click += BtnAction_Click;
            btnLoan.Click += BtnLoan_Click;
            this.KeyDown += MainForm_KeyDown;
            this.KeyPreview = true; // Чтобы форма ловила клавиши, даже если фокус на кнопке

            UpdateUI();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Таймер просто перерисовывает форму (в нашем случае обновляет текстовые поля)
            // Можно было бы вызывать Invalidate(), но проще обновить текст.
            UpdateUI();
        }

        /// <summary>
        /// Обновление всех элементов UI на основе состояния модели.
        /// </summary>
        private void UpdateUI()
        {
            // Баланс
            lblBalance.Text = $"Баланс: {_gameState.Balance}";

            // Цифры
            if (_gameState.CurrentNumbers != null)
            {
                lblNumbers.Text = string.Join(" ", _gameState.CurrentNumbers);
            }

            // Конец игры
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
                btnAction.Enabled = true;
                btnLoan.Enabled = true;
            }

            // Меняем текст кнопки по кругу
            string[] buttonTexts = { "ДЕП", "ДОДЕП", "ЛАСТ ДЕП" };
            btnAction.Text = buttonTexts[_buttonClickCount % 3];
        }

        // ---- Контроллер: Обработка действий пользователя ----

        private void BtnAction_Click(object sender, EventArgs e)
        {
            if (_gameState.IsGameOver) return;

            // Увеличиваем счетчик для смены текста
            _buttonClickCount++;

            // Вызываем метод модели
            _gameState.Spin();

            // Обновляем экран
            UpdateUI();
        }

        private void BtnLoan_Click(object sender, EventArgs e)
        {
            if (_gameState.IsGameOver) return;

            _gameState.TakeLoan();
            UpdateUI();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Перезапуск по пробелу, если игра окончена
            if (e.KeyCode == Keys.Space && _gameState.IsGameOver)
            {
                _gameState.ResetGame();
                _buttonClickCount = 0; // Сброс счетчика текста кнопки
                UpdateUI();
            }
        }

        // Не забудьте вызвать InitializeComponent в конструкторе
        private void InitializeComponent()
        {
            // Этот метод создается автоматически дизайнером форм.
            // Просто убедись, что у тебя есть кнопки с нужными именами.
            // Если нет, создай их вручную в коде или через дизайнер.
            this.btnAction = new System.Windows.Forms.Button();
            this.btnLoan = new System.Windows.Forms.Button();
            this.lblBalance = new System.Windows.Forms.Label();
            this.lblNumbers = new System.Windows.Forms.Label();
            this.lblGameOver = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // btnAction
            this.btnAction.Location = new System.Drawing.Point(50, 150);
            this.btnAction.Name = "btnAction";
            this.btnAction.Size = new System.Drawing.Size(100, 40);
            this.btnAction.TabIndex = 0;
            this.btnAction.Text = "ДЕП";
            this.btnAction.UseVisualStyleBackColor = true;

            // btnLoan
            this.btnLoan.Location = new System.Drawing.Point(200, 150);
            this.btnLoan.Name = "btnLoan";
            this.btnLoan.Size = new System.Drawing.Size(100, 40);
            this.btnLoan.TabIndex = 1;
            this.btnLoan.Text = "Займ";
            this.btnLoan.UseVisualStyleBackColor = true;

            // lblBalance
            this.lblBalance.AutoSize = true;
            this.lblBalance.Location = new System.Drawing.Point(50, 50);
            this.lblBalance.Name = "lblBalance";
            this.lblBalance.Size = new System.Drawing.Size(70, 17);
            this.lblBalance.TabIndex = 2;
            this.lblBalance.Text = "Баланс: 0";

            // lblNumbers
            this.lblNumbers.AutoSize = true;
            this.lblNumbers.Font = new System.Drawing.Font("Courier New", 16F, System.Drawing.FontStyle.Bold);
            this.lblNumbers.Location = new System.Drawing.Point(50, 100);
            this.lblNumbers.Name = "lblNumbers";
            this.lblNumbers.Size = new System.Drawing.Size(200, 30);
            this.lblNumbers.TabIndex = 3;
            this.lblNumbers.Text = "0 0 0 0 0 0 0";

            // lblGameOver
            this.lblGameOver.AutoSize = true;
            this.lblGameOver.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblGameOver.ForeColor = System.Drawing.Color.Red;
            this.lblGameOver.Location = new System.Drawing.Point(50, 200);
            this.lblGameOver.Name = "lblGameOver";
            this.lblGameOver.Size = new System.Drawing.Size(300, 25);
            this.lblGameOver.TabIndex = 4;
            this.lblGameOver.Text = "Ты всё слил в нулину...";
            this.lblGameOver.Visible = false;

            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 300);
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

        // Объявление контролов (нужно для дизайнера)
        private System.Windows.Forms.Button btnAction;
        private System.Windows.Forms.Button btnLoan;
        private System.Windows.Forms.Label lblBalance;
        private System.Windows.Forms.Label lblNumbers;
        private System.Windows.Forms.Label lblGameOver;
    }
}