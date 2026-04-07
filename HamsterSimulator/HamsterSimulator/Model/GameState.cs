using System;

namespace HamsterSimulator.Model
{
    public class GameState
    {
        private const double MammothConstant = 0.42;
        private const double HamsterWarmupCoefficient = 1.15;
        private readonly Random _random;

        public int Balance { get; private set; }
        public int LoanCount { get; private set; }
        public int MicroLoanCount { get; private set; }
        public int[] CurrentNumbers { get; private set; } = new int[7];
        public bool IsGameOver { get; private set; }
        public string GameOverMessage { get; private set; } = string.Empty;

        // ЛудоДофамин
        public int LudoDopamine { get; private set; }
        private int _winStreak = 0;
        private const int WinTarget = 25;

        // Здоровье и коллекторы
        public int Health { get; private set; }
        private double _currentCollectionChance;
        private const double InitialCollectionChance = 0.0;
        private const double LoanChanceIncrease = 0.05;
        private const double MicroLoanChanceIncrease = 0.03;
        private const double PostTriggerChanceDecrease = 0.05;

        // Подкрутка
        private int _riggingUsesLeft;
        private bool _isNextSpinRigged;

        public int RiggingUsesLeft => _riggingUsesLeft;
        public bool CanUseRigging => !IsGameOver && _riggingUsesLeft > 0;
        public double CurrentCollectionChance => _currentCollectionChance;

        public event Action OnCollectorsTriggered;

        public GameState() : this(new Random()) { }

        public GameState(Random random)
        {
            _random = random;
            ResetGame();
        }

        public void ResetGame()
        {
            Balance = 100;
            LoanCount = 0;
            MicroLoanCount = 0;
            _riggingUsesLeft = 3;
            _isNextSpinRigged = false;
            Health = 5;
            _currentCollectionChance = InitialCollectionChance;
            LudoDopamine = 0;
            _winStreak = 0;
            IsGameOver = false;
            GameOverMessage = string.Empty;
            for (int i = 0; i < CurrentNumbers.Length; i++)
                CurrentNumbers[i] = 0;
            CheckGameStatus();
        }

        public int CalculateSpinPenalty()
        {
            return LoanCount * 2 + MicroLoanCount * 1;
        }

        public int CalculateTotalSpinCost()
        {
            return 10 + CalculateSpinPenalty();
        }

        public void Spin()
        {
            if (IsGameOver) return;

            int totalCost = CalculateTotalSpinCost();
            if (Balance < totalCost)
            {
                CheckGameStatus();
                return;
            }

            Balance -= totalCost;

            for (int i = 0; i < 7; i++)
            {
                CurrentNumbers[i] = _random.Next(0, 10);
            }

            bool isWin = false;

            if (_isNextSpinRigged)
            {
                Balance += 50;
                _isNextSpinRigged = false;
                _riggingUsesLeft--;
                isWin = true;
            }
            else
            {
                isWin = ApplyCombinationEffects();
            }

            // Начисление ЛудоДофамина
            if (isWin)
            {
                _winStreak++;
                int dopamineGain = (_winStreak >= 2) ? 2 : 1;
                LudoDopamine += dopamineGain;
            }
            else
            {
                _winStreak = 0;
            }

            // Проверка победы
            if (LudoDopamine >= WinTarget && !IsGameOver)
            {
                IsGameOver = true;
                GameOverMessage = "Невероятно, но ты не нищий!";
                return;
            }

            CheckGameStatus();
            CheckCollectors();
        }

        public void TakeLoan()
        {
            if (IsGameOver) return;

            if (LoanCount < 3)
            {
                LoanCount++;
                Balance += 50;
                IncreaseCollectionChance(LoanChanceIncrease);
                CheckGameStatus();
            }
        }

        public void TakeMicroLoan()
        {
            if (IsGameOver) return;

            if (MicroLoanCount < 5)
            {
                MicroLoanCount++;
                Balance += 30;
                IncreaseCollectionChance(MicroLoanChanceIncrease);
                CheckGameStatus();
            }
        }

        public void UseRigging()
        {
            if (!CanUseRigging) return;
            _isNextSpinRigged = true;
        }

        private void IncreaseCollectionChance(double increment)
        {
            _currentCollectionChance += increment;
            if (_currentCollectionChance > 1.0) _currentCollectionChance = 1.0;
        }

        private void CheckCollectors()
        {
            if (IsGameOver) return;
            if (Health <= 0)
            {
                TriggerGameOverByCollectors();
                return;
            }

            double roll = _random.NextDouble();
            if (roll < _currentCollectionChance)
            {
                Health--;
                _currentCollectionChance = Math.Max(0, _currentCollectionChance - PostTriggerChanceDecrease);
                OnCollectorsTriggered?.Invoke();

                if (Health <= 0)
                {
                    TriggerGameOverByCollectors();
                }
            }
        }

        private void TriggerGameOverByCollectors()
        {
            IsGameOver = true;
            GameOverMessage = "Тебя нашли коллекторы, прощай...";
        }

        /// <returns>True, если был выигрыш (прибавка к балансу)</returns>
        private bool ApplyCombinationEffects()
        {
            double luckIndex = CalculateLuckIndex();
            bool isWin = false;

            if (luckIndex > 14.0)
            {
                if (AllNumbersSame())
                {
                    Balance += 100;
                    isWin = true;
                }
                else
                {
                    Balance += 50;
                    isWin = true;
                }
            }
            else
            {
                Balance = Math.Max(0, Balance - 10);
                isWin = false;
            }
            return isWin;
        }

        private bool AllNumbersSame()
        {
            if (CurrentNumbers.Length == 0) return false;
            int first = CurrentNumbers[0];
            for (int i = 1; i < CurrentNumbers.Length; i++)
                if (CurrentNumbers[i] != first) return false;
            return true;
        }

        private double CalculateLuckIndex()
        {
            double sum = 0;
            for (int i = 0; i < CurrentNumbers.Length; i++)
            {
                sum += CurrentNumbers[i] * Math.Pow(HamsterWarmupCoefficient, i % 3);
            }
            double result = (sum / MammothConstant) % 15;
            return result;
        }

        private void CheckGameStatus()
        {
            if (IsGameOver) return;
            CheckForGameOver();
            CheckForDeadEnd();
        }

        private void CheckForGameOver()
        {
            if (Balance <= 0 && LoanCount >= 3 && MicroLoanCount >= 5)
            {
                IsGameOver = true;
                GameOverMessage = "Ты всё слил в нулину, побрили хомяка :(";
            }
            if (Balance < 0) Balance = 0;
        }

        private void CheckForDeadEnd()
        {
            if (IsGameOver) return;

            bool cannotSpin = Balance < CalculateTotalSpinCost();
            bool noLoansLeft = LoanCount >= 3 && MicroLoanCount >= 5;

            if (cannotSpin && noLoansLeft)
            {
                IsGameOver = true;
                GameOverMessage = "Ты всё слил в нулину, побрили хомяка :(";
            }
        }
    }
}