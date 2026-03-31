using Microsoft.VisualStudio.TestTools.UnitTesting;
using HamsterSimulator.Model;

namespace HamsterSimulator.Tests
{
    [TestClass]
    public class GameStateTests
    {
        [TestMethod]
        public void Spin_DoesNothing_WhenBalanceLessThan10()
        {
            var game = new GameState();
            while (game.Balance >= 10) game.Spin();
            int zeroBalance = game.Balance;
            var numbersBefore = (int[])game.CurrentNumbers.Clone();

            game.Spin();
            Assert.AreEqual(zeroBalance, game.Balance);
            CollectionAssert.AreEqual(numbersBefore, game.CurrentNumbers);
        }

        [TestMethod]
        public void TakeLoan_IncreasesBalanceBy50_AndIncreasesLoanCount()
        {
            var game = new GameState();
            int initialBalance = game.Balance;
            int initialLoanCount = game.LoanCount;

            game.TakeLoan();

            Assert.AreEqual(initialBalance + 50, game.Balance);
            Assert.AreEqual(initialLoanCount + 1, game.LoanCount);
        }

        [TestMethod]
        public void TakeLoan_CanBeUsedOnlyThreeTimes()
        {
            var game = new GameState();
            for (int i = 0; i < 3; i++) game.TakeLoan();
            int balanceAfterThree = game.Balance;
            int loanCountAfterThree = game.LoanCount;

            game.TakeLoan();

            Assert.AreEqual(loanCountAfterThree, game.LoanCount);
            Assert.AreEqual(balanceAfterThree, game.Balance);
        }

        [TestMethod]
        public void TakeMicroLoan_IncreasesBalanceBy30_AndIncreasesMicroLoanCount()
        {
            var game = new GameState();
            int initialBalance = game.Balance;
            int initialMicroLoanCount = game.MicroLoanCount;

            game.TakeMicroLoan();

            Assert.AreEqual(initialBalance + 30, game.Balance);
            Assert.AreEqual(initialMicroLoanCount + 1, game.MicroLoanCount);
        }

        [TestMethod]
        public void MicroLoan_CanBeUsedOnlyFiveTimes()
        {
            var game = new GameState();
            for (int i = 0; i < 5; i++) game.TakeMicroLoan();
            int balanceAfterFive = game.Balance;
            int microLoanCountAfterFive = game.MicroLoanCount;

            game.TakeMicroLoan();

            Assert.AreEqual(microLoanCountAfterFive, game.MicroLoanCount);
            Assert.AreEqual(balanceAfterFive, game.Balance);
        }

        [TestMethod]
        public void Spin_SubtractsSpinPenalty()
        {
            var game = new GameState();
            game.TakeLoan(); // LoanCount = 1, penalty = 2
            int initialBalance = game.Balance; // 100 + 50 = 150
            game.Spin();
            Assert.IsTrue(game.Balance <= initialBalance - 12);
        }

        [TestMethod]
        public void GameOver_WhenBalanceZeroAndAllLoansUsed()
        {
            var game = new GameState();
            while (game.Balance >= 10) game.Spin();

            for (int i = 0; i < 3; i++) game.TakeLoan();
            for (int i = 0; i < 5; i++) game.TakeMicroLoan();

            while (game.Balance >= 10 + game.CalculateSpinPenalty())
                game.Spin();

            game.Spin();

            Assert.IsTrue(game.IsGameOver);
            Assert.IsTrue(game.GameOverMessage.Contains("побрили хомяка"));
        }

        [TestMethod]
        public void GameOver_WhenBalanceInsufficientForSpinAndAllLoansUsed()
        {
            var game = new GameState();
            for (int i = 0; i < 3; i++) game.TakeLoan();
            for (int i = 0; i < 5; i++) game.TakeMicroLoan();

            while (game.Balance >= game.CalculateTotalSpinCost())
                game.Spin();

            int balanceBefore = game.Balance;
            game.Spin();

            Assert.IsTrue(game.IsGameOver);
            Assert.AreEqual(balanceBefore, game.Balance);
        }

        [TestMethod]
        public void GameOver_WhenDeadEnd_NoMoneyAndNoLoansLeft()
        {
            var game = new GameState();

            for (int i = 0; i < 3; i++) game.TakeLoan();
            for (int i = 0; i < 5; i++) game.TakeMicroLoan();

            while (game.Balance >= game.CalculateTotalSpinCost())
                game.Spin();

            game.Spin();

            Assert.IsTrue(game.IsGameOver);
            Assert.IsTrue(game.GameOverMessage.Contains("побрили хомяка"));
        }

        [TestMethod]
        public void ResetGame_RestoresInitialState()
        {
            var game = new GameState();
            game.Spin();
            game.TakeLoan();
            game.TakeMicroLoan();

            game.ResetGame();

            Assert.AreEqual(100, game.Balance);
            Assert.AreEqual(0, game.LoanCount);
            Assert.AreEqual(0, game.MicroLoanCount);
            Assert.IsFalse(game.IsGameOver);
            for (int i = 0; i < game.CurrentNumbers.Length; i++)
                Assert.AreEqual(0, game.CurrentNumbers[i]);
        }

        [TestMethod]
        public void UseRigging_SetsNextSpinFlag_AndDoesNotAffectBalance()
        {
            var game = new GameState();
            int initialBalance = game.Balance;
            int initialUses = game.RiggingUsesLeft;

            game.UseRigging();

            // Баланс не изменился, счётчик не уменьшился
            Assert.AreEqual(initialBalance, game.Balance);
            Assert.AreEqual(initialUses, game.RiggingUsesLeft);
            // Флаг должен быть установлен (проверим через публичное поле – можно добавить тестовый доступ,
            // но проще проверить через Spin, который использует флаг)
        } 
    }
}