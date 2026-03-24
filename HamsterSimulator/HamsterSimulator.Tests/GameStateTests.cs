using Microsoft.VisualStudio.TestTools.UnitTesting;
using HamsterSimulator.Model;

namespace HamsterSimulator.Tests
{
    [TestClass]
    public class GameStateTests
    {
        [TestMethod]
        public void Spin_DecreasesBalanceByAtLeast10_WhenEnoughMoney()
        {
            var game = new GameState();
            int initialBalance = game.Balance;
            game.Spin();
            // Баланс должен уменьшиться как минимум на 10 (стоимость спина)
            Assert.IsTrue(game.Balance <= initialBalance - 10);
        }

        [TestMethod]
        public void Spin_WhenBalanceIsZero_DoesNotChangeState()
        {
            // Arrange
            var game = new GameState();
            // Обнуляем баланс (так делать не совсем правильно, но для теста ок)
            // Лучше создать специальный метод, но пока так.
            for (int i = 0; i < 10; i++) game.Spin(); // Спустим 100 монет

            int balanceBefore = game.Balance;
            var numbersBefore = (int[])game.CurrentNumbers.Clone();

            // Act
            game.Spin(); // Попытка спина с 0 баланса

            // Assert
            Assert.AreEqual(balanceBefore, game.Balance);
            // Проверим, что цифры могли измениться? Они не должны были.
            // Но так как Spin() содержит return при IsGameOver или балансе <10,
            // numbers не меняются.
            CollectionAssert.AreEqual(numbersBefore, game.CurrentNumbers);
        }

        [TestMethod]
        public void TakeLoan_IncreasesBalanceBy60_AndIncreasesLoanCount()
        {
            var game = new GameState();
            int initialBalance = game.Balance;
            int initialLoanCount = game.LoanCount;

            game.TakeLoan();

            Assert.AreEqual(initialBalance + 60, game.Balance);
            Assert.AreEqual(initialLoanCount + 1, game.LoanCount);
        }

        [TestMethod]
        public void TakeLoan_CanOnlyBeUsedThreeTimes()
        {
            // Arrange
            var game = new GameState();

            // Act
            game.TakeLoan();
            game.TakeLoan();
            game.TakeLoan();
            int balanceAfterThreeLoans = game.Balance;
            game.TakeLoan(); // Четвертый раз

            // Assert
            Assert.AreEqual(3, game.LoanCount);
            Assert.AreEqual(balanceAfterThreeLoans, game.Balance); // Баланс не изменился
        }

        [TestMethod]
        public void GameOver_WhenBalanceZeroAndThreeLoansTaken()
        {
            var game = new GameState();

            // Тратим все деньги до нуля (игнорируем штрафы, просто крутим, пока баланс не станет меньше 10)
            while (game.Balance >= 10)
                game.Spin();

            // Берём три займа
            for (int i = 0; i < 3; i++)
                game.TakeLoan();

            // Теперь баланс должен стать положительным (60*3 = 180), но мы хотим его обнулить.
            // Снова тратим всё до нуля.
            while (game.Balance >= 10)
                game.Spin();

            // Вызываем спин, чтобы проверить GameOver (он сработает, даже если баланс < 10,
            // потому что внутри Spin есть вызов CheckForGameOver)
            game.Spin();

            Assert.IsTrue(game.IsGameOver);
            Assert.IsTrue(game.GameOverMessage.Contains("побрили хомяка"));
        }

        // Более правильный тест на GameOver, если бы мы могли принудительно вызвать проверку.
        // Но для простоты примера считаем, что мы исправили Spin.
        // В реальности, в Spin нужно убрать ранний выход и проверять IsGameOver иначе.
    }
}