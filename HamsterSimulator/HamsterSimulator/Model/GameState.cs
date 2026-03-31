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

        private int _riggingUsesLeft;
        private bool _isNextSpinRigged;

        public int RiggingUsesLeft => _riggingUsesLeft;
        public bool CanUseRigging => !IsGameOver && _riggingUsesLeft > 0;

        public GameState() : this(new Random())
        {
        }

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

            if (_isNextSpinRigged)
            {
                // Гарантированный выигрыш
                Balance += 50;
                _isNextSpinRigged = false;
                _riggingUsesLeft--;
            }
            else
            {
                ApplyCombinationEffects();
            }

            CheckGameStatus();
        }

        public void TakeLoan()
        {
            if (IsGameOver) return;

            if (LoanCount < 3)
            {
                LoanCount++;
                Balance += 50;
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
                CheckGameStatus();
            }
        }

        // Активирует подкрутку на следующий спин (если есть использования)
        public void UseRigging()
        {
            if (!CanUseRigging) return;
            _isNextSpinRigged = true;
            // Счётчик не уменьшается здесь, он уменьшится при реальном спине
        }

        private void ApplyCombinationEffects()
        {
            double luckIndex = CalculateLuckIndex();

            if (luckIndex > 14.0)
            {
                if (AllNumbersSame())
                    Balance += 100;
                else
                    Balance += 50;
            }
            else
            {
                Balance = Math.Max(0, Balance - 10);
            }
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