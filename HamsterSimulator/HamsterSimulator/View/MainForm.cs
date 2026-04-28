using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using HamsterSimulator.Controller;
using HamsterSimulator.Model;

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

        private Timer _flashTimer;
        private Color _originalBackColor;

        private int _previousBalance;

        // Элементы для героев
        private Label _lblHeroInfo;
        private Button _btnAbility;
        private Label _lblQuote;
        private Random _rand = new Random();
        private string[] _quotes = {
            "«Прикормил казино, сейчас попрёт!»",
            "«КАК Я МОГУ БЫТЬ НИЩИМ, КАК???»",
            "«Три самых важных слова в мире заработка — деп, додеп и ласт деп!» - Уоррен Баффет.",
            "«На сдачу можно хот-догов купить в Трассе...»",
            "«Бл[ЦЕНЗУРА]»",
            "«Я не слил деньги, я просто инвестировал в казино»",
            "«Пока ещё не Футболист, жить можно»",
            "«Раньше играл в Монополию, а теперь в казик...»",
            "«Пора торговать концентратом апельсинового сока!»",
            "«Пора прокатиться на самокате до нового МФО!»"
        };

        // Картинка-уведомление о смене героя
        private PictureBox _heroSwitchPicture;
        private Timer _heroSwitchTimer;

        public MainForm()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            _controller = new GameController(this);
            _previousBalance = _controller.Balance;

            // Создаём героев
            var heroes = new List<Hero>
            {
                Hero.CreateDefault(),
                Hero.CreateKalivan(),
                Hero.CreateInfluencer(),
                Hero.CreateTank()
            };
            _controller.InitializeHeroes(heroes);

            // Подписка на события героев
            _controller.OnKalivanGuessRequest += OnKalivanGuessRequest;
            _controller.OnHeroSwitched += OnHeroChanged;
            _controller.OnKalivanResult += (player, comp) =>
            {
                MessageBox.Show($"Компьютер загадал {comp}. Вы назвали {player}. Результат применён.", "Способность Калывана");
            };

            _gameTimer = new Timer();
            _gameTimer.Interval = 100;
            _gameTimer.Tick += Timer_Tick;
            _gameTimer.Start();

            _animationTimer = new Timer();
            _animationTimer.Interval = 100;
            _animationTimer.Tick += AnimationTimer_Tick;

            _flashTimer = new Timer();
            _flashTimer.Interval = 3000;
            _flashTimer.Tick += FlashTimer_Tick;
            _originalBackColor = this.BackColor;

            // Метка для цитаты
            _lblQuote = new Label
            {
                AutoSize = false,
                Width = 450,
                Height = 120,
                Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold),
                Location = new Point(20, 180),
                ForeColor = Color.DarkBlue,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(_lblQuote);
            ShowRandomQuote();

            // Информация о герое
            _lblHeroInfo = new Label
            {
                AutoSize = false,
                Width = 300,
                Height = 60,
                Font = new Font("Microsoft Sans Serif", 10F),
                Location = new Point(20, 20),
                ForeColor = Color.Black,
                BackColor = Color.LightGray
            };
            this.Controls.Add(_lblHeroInfo);

            // Кнопка активной способности
            _btnAbility = new Button
            {
                Text = "Способность",
                Location = new Point(20, 90),
                Size = new Size(120, 30),
                BackColor = Color.Gold
            };
            _btnAbility.Click += BtnAbility_Click;
            this.Controls.Add(_btnAbility);

            // Картинка уведомления о смене героя (правый нижний угол)
            _heroSwitchPicture = new PictureBox
            {
                Size = new Size(128, 128),
                SizeMode = PictureBoxSizeMode.Zoom,
                Visible = false,
                BackColor = Color.Transparent
            };
            // Загружаем картинку из файла (положите switch_hero.png в папку с exe или в Resources)
            string imagePath = System.IO.Path.Combine(Application.StartupPath, "switch_hero.png");
            if (System.IO.File.Exists(imagePath))
            {
                _heroSwitchPicture.Image = Image.FromFile(imagePath);
            }
            else
            {
                // Если файла нет, создадим простую заглушку (цветной квадрат с текстом)
                Bitmap bmp = new Bitmap(128, 128);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Gray);
                    g.DrawString("Hero\nSwitch", SystemFonts.DefaultFont, Brushes.White, new RectangleF(0, 0, 128, 128));
                }
                _heroSwitchPicture.Image = bmp;
            }
            // Размещаем в правом нижнем углу (отступы 20 пикселей)
            _heroSwitchPicture.Location = new Point(this.ClientSize.Width - _heroSwitchPicture.Width - 20,
                                                   this.ClientSize.Height - _heroSwitchPicture.Height - 20);
            this.Controls.Add(_heroSwitchPicture);
            _heroSwitchPicture.BringToFront();

            // Таймер для скрытия картинки
            _heroSwitchTimer = new Timer { Interval = 4000 };
            _heroSwitchTimer.Tick += (s, e) => {
                _heroSwitchPicture.Visible = false;
                _heroSwitchTimer.Stop();
            };

            // Отображаем начального героя
            UpdateHeroInfo(_controller.CurrentHero);

            btnAction.Click += BtnAction_Click;
            btnLoan.Click += BtnLoan_Click;
            btnMicroLoan.Click += BtnMicroLoan_Click;
            btnRigging.Click += BtnRigging_Click;
            this.KeyDown += MainForm_KeyDown;
            this.KeyPreview = true;
            this.Resize += (s, e) => RepositionHeroPicture(); // при изменении размера окна

            UpdateUI();
        }

        private void RepositionHeroPicture()
        {
            if (_heroSwitchPicture != null)
            {
                _heroSwitchPicture.Location = new Point(this.ClientSize.Width - _heroSwitchPicture.Width - 20,
                                                       this.ClientSize.Height - _heroSwitchPicture.Height - 20);
            }
        }

        private void OnHeroChanged(Hero hero)
        {
            UpdateHeroInfo(hero);
            // Показать картинку на 4 секунды
            _heroSwitchPicture.Visible = true;
            _heroSwitchTimer.Stop();
            _heroSwitchTimer.Start();
            UpdateUI();
        }

        private void UpdateHeroInfo(Hero hero)
        {
            if (hero == null) return;
            _lblHeroInfo.Text = $"{hero.Name}\n{hero.Description}";
            _btnAbility.Enabled = hero.IsActiveAbility && !_controller.IsGameOver;
        }

        private void BtnAbility_Click(object sender, EventArgs e)
        {
            if (_controller.IsGameOver) return;
            _controller.ActivateHeroAbility();
        }

        private void OnKalivanGuessRequest(int computerNumber)
        {
            Form prompt = new Form()
            {
                Width = 300,
                Height = 150,
                Text = "Способность Калывана",
                StartPosition = FormStartPosition.CenterParent
            };
            Label textLabel = new Label() { Left = 10, Top = 20, Text = "Введите число от 1 до 10:" };
            TextBox textBox = new TextBox() { Left = 10, Top = 50, Width = 200 };
            Button confirm = new Button() { Text = "ОК", Left = 10, Top = 80, Width = 80 };
            confirm.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirm);
            prompt.ShowDialog();

            if (int.TryParse(textBox.Text, out int guess) && guess >= 1 && guess <= 10)
            {
                _controller.ApplyKalivanResult(guess, computerNumber);
            }
            else
            {
                MessageBox.Show("Неверный ввод. Способность не применена.", "Ошибка");
            }
        }

        private void ShowRandomQuote()
        {
            int index = _rand.Next(_quotes.Length);
            _lblQuote.Text = _quotes[index];
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_isAnimating)
                UpdateUI();
        }

        public void UpdateUI()
        {
            int newBalance = _controller.Balance;

            if (newBalance < _previousBalance)
            {
                VibrateForm();
                RedFlash();

                lblBalance.ForeColor = Color.Red;
                var timer = new Timer { Interval = 500 };
                timer.Tick += (s, ev) => { lblBalance.ForeColor = SystemColors.ControlText; timer.Stop(); };
                timer.Start();
            }
            else if (newBalance > _previousBalance)
            {
                lblBalance.ForeColor = Color.Green;
                var timer = new Timer { Interval = 500 };
                timer.Tick += (s, ev) => { lblBalance.ForeColor = SystemColors.ControlText; timer.Stop(); };
                timer.Start();
            }
            _previousBalance = newBalance;

            lblBalance.Text = $"Баланс: {_controller.Balance}";
            lblLoanCount.Text = $"Займы: {_controller.LoanCount}/{3 + (_controller.CurrentHero?.ExtraLoans ?? 0)}";
            lblMicroLoanCount.Text = $"Микрозаймы: {_controller.MicroLoanCount}/{5 + (_controller.CurrentHero?.ExtraMicroLoans ?? 0)}";
            lblPenalty.Text = $"Штраф за долги: {_controller.SpinPenalty}";
            lblRiggingUses.Text = $"Подкрутка: {_controller.RiggingUsesLeft}/3";

            int health = _controller.Health;
            string hearts = new string('♥', health) + new string('♡', (_controller.CurrentHero?.StartingHealth ?? 5) - health);
            lblHealth.Text = $"Здоровье: {hearts}";

            lblCollectionChance.Text = $"Шанс коллекторов: {_controller.CollectionChance:P0}";
            lblDopamine.Text = $"ЛудоДофамин: {_controller.LudoDopamine}/25";

            int streak = _controller.WinStreak;
            if (streak >= 3)
            {
                lblWinStreak.Text = $"Серия: {streak} 🔥";
                lblWinStreak.ForeColor = Color.Gold;
            }
            else
            {
                lblWinStreak.Text = $"Серия: {streak}";
                lblWinStreak.ForeColor = SystemColors.ControlText;
            }
            lblMaxStreak.Text = $"Рекорд: {_controller.MaxWinStreak}";

            if (!_isAnimating && _controller.CurrentNumbers != null)
                lblNumbers.Text = string.Join(" ", _controller.CurrentNumbers);

            if (_controller.IsGameOver)
            {
                lblGameOver.Text = _controller.GameOverMessage;
                lblGameOver.Visible = true;
                btnAction.Enabled = false;
                btnLoan.Enabled = false;
                btnMicroLoan.Enabled = false;
                btnRigging.Enabled = false;
                if (_btnAbility != null) _btnAbility.Enabled = false;
            }
            else
            {
                lblGameOver.Visible = false;
                bool canSpin = !_isAnimating && _controller.Balance >= _controller.TotalSpinCost;
                btnAction.Enabled = canSpin;
                btnLoan.Enabled = !_isAnimating && _controller.LoanCount < (3 + (_controller.CurrentHero?.ExtraLoans ?? 0));
                btnMicroLoan.Enabled = !_isAnimating && _controller.MicroLoanCount < (5 + (_controller.CurrentHero?.ExtraMicroLoans ?? 0));
                btnRigging.Enabled = !_isAnimating && _controller.CanUseRigging;
                if (_btnAbility != null)
                    _btnAbility.Enabled = _controller.CurrentHero?.IsActiveAbility == true && !_isAnimating && !_controller.IsGameOver;
            }

            string[] buttonTexts = { "ДЕП", "ДОДЕП", "ЛАСТ ДЕП" };
            btnAction.Text = buttonTexts[_buttonClickCount % 3];
        }

        private void VibrateForm()
        {
            var original = this.Location;
            for (int i = 0; i < 8; i++)
            {
                this.Location = new Point(original.X + (i % 2 == 0 ? 6 : -6), original.Y + (i % 3 == 0 ? 4 : -4));
                System.Threading.Thread.Sleep(15);
            }
            this.Location = original;
        }

        private void RedFlash()
        {
            var originalBack = this.BackColor;
            this.BackColor = Color.DarkRed;
            Timer timer = new Timer { Interval = 200 };
            timer.Tick += (s, e) => { this.BackColor = originalBack; timer.Stop(); timer.Dispose(); };
            timer.Start();
        }

        public void TriggerCollectorsEffect()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(TriggerCollectorsEffect));
                return;
            }
            _originalBackColor = this.BackColor;
            this.BackColor = Color.Red;
            _flashTimer.Start();
        }

        private void FlashTimer_Tick(object sender, EventArgs e)
        {
            _flashTimer.Stop();
            this.BackColor = _originalBackColor;
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
                ShowRandomQuote();
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
            if (_btnAbility != null) _btnAbility.Enabled = false;

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
                    ShowRandomQuote();
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
            this.lblHealth = new System.Windows.Forms.Label();
            this.lblCollectionChance = new System.Windows.Forms.Label();
            this.lblDopamine = new System.Windows.Forms.Label();
            this.lblWinStreak = new System.Windows.Forms.Label();
            this.lblMaxStreak = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnAction
            // 
            this.btnAction.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.btnAction.Location = new System.Drawing.Point(637, 511);
            this.btnAction.Name = "btnAction";
            this.btnAction.Size = new System.Drawing.Size(186, 93);
            this.btnAction.TabIndex = 0;
            this.btnAction.Text = "ДЕП";
            this.btnAction.UseVisualStyleBackColor = true;
            // 
            // btnLoan
            // 
            this.btnLoan.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.btnLoan.Location = new System.Drawing.Point(467, 400);
            this.btnLoan.Name = "btnLoan";
            this.btnLoan.Size = new System.Drawing.Size(208, 93);
            this.btnLoan.TabIndex = 1;
            this.btnLoan.Text = "Займ";
            this.btnLoan.UseVisualStyleBackColor = true;
            // 
            // btnMicroLoan
            // 
            this.btnMicroLoan.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.btnMicroLoan.Location = new System.Drawing.Point(786, 400);
            this.btnMicroLoan.Name = "btnMicroLoan";
            this.btnMicroLoan.Size = new System.Drawing.Size(216, 93);
            this.btnMicroLoan.TabIndex = 5;
            this.btnMicroLoan.Text = "Микрозайм";
            this.btnMicroLoan.UseVisualStyleBackColor = true;
            // 
            // btnRigging
            // 
            this.btnRigging.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.btnRigging.Location = new System.Drawing.Point(1299, 14);
            this.btnRigging.Name = "btnRigging";
            this.btnRigging.Size = new System.Drawing.Size(157, 77);
            this.btnRigging.TabIndex = 9;
            this.btnRigging.Text = "Подкрутка";
            this.btnRigging.UseVisualStyleBackColor = true;
            // 
            // lblBalance
            // 
            this.lblBalance.AutoSize = true;
            this.lblBalance.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold);
            this.lblBalance.Location = new System.Drawing.Point(649, 49);
            this.lblBalance.Name = "lblBalance";
            this.lblBalance.Size = new System.Drawing.Size(145, 31);
            this.lblBalance.TabIndex = 2;
            this.lblBalance.Text = "Баланс: 0";
            // 
            // lblNumbers
            // 
            this.lblNumbers.AutoSize = true;
            this.lblNumbers.Font = new System.Drawing.Font("Courier New", 28F, System.Drawing.FontStyle.Bold);
            this.lblNumbers.Location = new System.Drawing.Point(540, 309);
            this.lblNumbers.Name = "lblNumbers";
            this.lblNumbers.Size = new System.Drawing.Size(387, 53);
            this.lblNumbers.TabIndex = 3;
            this.lblNumbers.Text = "0 0 0 0 0 0 0";
            // 
            // lblGameOver
            // 
            this.lblGameOver.AutoSize = true;
            this.lblGameOver.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold);
            this.lblGameOver.ForeColor = System.Drawing.Color.Red;
            this.lblGameOver.Location = new System.Drawing.Point(533, 607);
            this.lblGameOver.Name = "lblGameOver";
            this.lblGameOver.Size = new System.Drawing.Size(371, 36);
            this.lblGameOver.TabIndex = 4;
            this.lblGameOver.Text = "Ты всё слил в нулину...";
            this.lblGameOver.Visible = false;
            // 
            // lblLoanCount
            // 
            this.lblLoanCount.AutoSize = true;
            this.lblLoanCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.lblLoanCount.Location = new System.Drawing.Point(508, 101);
            this.lblLoanCount.Name = "lblLoanCount";
            this.lblLoanCount.Size = new System.Drawing.Size(136, 29);
            this.lblLoanCount.TabIndex = 6;
            this.lblLoanCount.Text = "Займы: 0/3";
            // 
            // lblMicroLoanCount
            // 
            this.lblMicroLoanCount.AutoSize = true;
            this.lblMicroLoanCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.lblMicroLoanCount.Location = new System.Drawing.Point(781, 101);
            this.lblMicroLoanCount.Name = "lblMicroLoanCount";
            this.lblMicroLoanCount.Size = new System.Drawing.Size(209, 29);
            this.lblMicroLoanCount.TabIndex = 7;
            this.lblMicroLoanCount.Text = "Микрозаймы: 0/5";
            // 
            // lblPenalty
            // 
            this.lblPenalty.AutoSize = true;
            this.lblPenalty.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.lblPenalty.ForeColor = System.Drawing.Color.DarkRed;
            this.lblPenalty.Location = new System.Drawing.Point(623, 143);
            this.lblPenalty.Name = "lblPenalty";
            this.lblPenalty.Size = new System.Drawing.Size(222, 29);
            this.lblPenalty.TabIndex = 8;
            this.lblPenalty.Text = "Штраф за долги: 0";
            // 
            // lblRiggingUses
            // 
            this.lblRiggingUses.AutoSize = true;
            this.lblRiggingUses.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblRiggingUses.Location = new System.Drawing.Point(1294, 121);
            this.lblRiggingUses.Name = "lblRiggingUses";
            this.lblRiggingUses.Size = new System.Drawing.Size(150, 25);
            this.lblRiggingUses.TabIndex = 10;
            this.lblRiggingUses.Text = "Подкрутка: 3/3";
            // 
            // lblHealth
            // 
            this.lblHealth.AutoSize = true;
            this.lblHealth.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.lblHealth.Location = new System.Drawing.Point(476, 192);
            this.lblHealth.Name = "lblHealth";
            this.lblHealth.Size = new System.Drawing.Size(149, 29);
            this.lblHealth.TabIndex = 11;
            this.lblHealth.Text = "Здоровье: 5";
            // 
            // lblCollectionChance
            // 
            this.lblCollectionChance.AutoSize = true;
            this.lblCollectionChance.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblCollectionChance.Location = new System.Drawing.Point(823, 196);
            this.lblCollectionChance.Name = "lblCollectionChance";
            this.lblCollectionChance.Size = new System.Drawing.Size(225, 25);
            this.lblCollectionChance.TabIndex = 12;
            this.lblCollectionChance.Text = "Шанс коллекторов: 0%";
            // 
            // lblDopamine
            // 
            this.lblDopamine.AutoSize = true;
            this.lblDopamine.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblDopamine.Location = new System.Drawing.Point(605, 254);
            this.lblDopamine.Name = "lblDopamine";
            this.lblDopamine.Size = new System.Drawing.Size(251, 29);
            this.lblDopamine.TabIndex = 13;
            this.lblDopamine.Text = "ЛудоДофамин: 0/25";
            // 
            // lblWinStreak
            // 
            this.lblWinStreak.AutoSize = true;
            this.lblWinStreak.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.lblWinStreak.Location = new System.Drawing.Point(900, 254);
            this.lblWinStreak.Name = "lblWinStreak";
            this.lblWinStreak.Size = new System.Drawing.Size(99, 25);
            this.lblWinStreak.TabIndex = 14;
            this.lblWinStreak.Text = "Серия: 0";
            // 
            // lblMaxStreak
            // 
            this.lblMaxStreak.AutoSize = true;
            this.lblMaxStreak.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblMaxStreak.Location = new System.Drawing.Point(1005, 258);
            this.lblMaxStreak.Name = "lblMaxStreak";
            this.lblMaxStreak.Size = new System.Drawing.Size(90, 20);
            this.lblMaxStreak.TabIndex = 15;
            this.lblMaxStreak.Text = "Рекорд: 0";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1468, 707);
            this.Controls.Add(this.lblMaxStreak);
            this.Controls.Add(this.lblWinStreak);
            this.Controls.Add(this.lblDopamine);
            this.Controls.Add(this.lblCollectionChance);
            this.Controls.Add(this.lblHealth);
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
        private System.Windows.Forms.Label lblHealth;
        private System.Windows.Forms.Label lblCollectionChance;
        private System.Windows.Forms.Label lblDopamine;
        private System.Windows.Forms.Label lblWinStreak;
        private System.Windows.Forms.Label lblMaxStreak;
    }
}