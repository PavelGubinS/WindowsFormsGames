using System;

namespace HamsterSimulator.Model
{
    public class GameState
    {
        private const double MammothConstant = 0.42;
        private const double HamsterWarmupCoefficient = 1.15;
        private readonly Random _random = new Random();

        public int Balance { get; private set; }
        public int LoanCount { get; private set; }
        public int MicroLoanCount { get; private set; }
        public int[] CurrentNumbers { get; private set; } = new int[7];
        public bool IsGameOver { get; private set; }
        public string GameOverMessage { get; private set; } = string.Empty;

        public GameState()
        {
            ResetGame();
        }

        public void ResetGame()
        {
            Balance = 100;
            LoanCount = 0;
            MicroLoanCount = 0;
            IsGameOver = false;
            GameOverMessage = string.Empty;
            for (int i = 0; i < CurrentNumbers.Length; i++)
                CurrentNumbers[i] = 0;
        }

        public int CalculateSpinPenalty()
        {
            return LoanCount * 2 + MicroLoanCount * 1;
        }

        public void Spin()
        {
            if (IsGameOver) return;

            int totalCost = 10 + CalculateSpinPenalty();
            if (Balance < totalCost)
            {
                IsGameOver = true;
                GameOverMessage = "Ты всё слил в нулину, побрили хомяка :(";
                return;
            }

            Balance -= totalCost;

            for (int i = 0; i < 7; i++)
            {
                CurrentNumbers[i] = _random.Next(0, 10);
            }

            ApplyCombinationEffects();
            CheckForGameOver();
        }

        public void TakeLoan()
        {
            if (IsGameOver) return;

            if (LoanCount < 3)
            {
                LoanCount++;
                Balance += 50;
            }
        }

        public void TakeMicroLoan()
        {
            if (IsGameOver) return;

            if (MicroLoanCount < 5)
            {
                MicroLoanCount++;
                Balance += 30;
            }
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

        private void CheckForGameOver()
        {
            if (Balance <= 0 && LoanCount >= 3 && MicroLoanCount >= 5)
            {
                IsGameOver = true;
                GameOverMessage = "Ты всё слил в нулину, побрили хомяка :(";
            }
            if (Balance < 0) Balance = 0;
        }
    }
}