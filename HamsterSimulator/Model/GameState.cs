using System;

namespace HamsterSimulator.Model
{
    /// <summary>
    /// Модель игрового мира. Содержит всю логику и состояние.
    /// </exceptions>
    public class GameState
    {
        // Константы для "сложной" формулы
        private const double MammothConstant = 0.42; // Постоянная Мамонта 
        private const double HamsterWarmupCoefficient = 1.15; // Коэффициент Прогрева хомяка

        private readonly Random _random = new Random();

        // Свойства баланса и состояния игры (доступны для View)
        public int Balance { get; private set; }
        public int LoanCount { get; private set; }
        public int[] CurrentNumbers { get; private set; } = new int[7];
        public bool IsGameOver { get; private set; }
        public string GameOverMessage { get; private set; } = string.Empty;

        public GameState()
        {
            ResetGame();
        }

        /// <summary>
        /// Сброс игры в начальное состояние.
        /// </summary>
        public void ResetGame()
        {
            Balance = 100; // Начинаем со 100 монет
            LoanCount = 0;
            IsGameOver = false;
            GameOverMessage = string.Empty;
            Array.Fill(CurrentNumbers, 0);
        }

        /// <summary>
        /// Действие "Сделать ставку" (крутить рулетку). Стоимость - 10 монет.
        /// </summary>
        public void Spin()
        {
            if (IsGameOver) return;

            // Если денег нет, мы не крутим, но проверяем, не наступил ли GAME OVER из-за отсутствия денег и наличия 3х займов.
            if (Balance < 10)
            {
                CheckForGameOver(); // Проверим, может уже всё?
                return;
            }

            Balance -= 10;

            for (int i = 0; i < 7; i++)
            {
                CurrentNumbers[i] = _random.Next(0, 10);
            }

            ApplyCombinationEffects();
            CheckForGameOver();
        }

        /// <summary>
        /// Действие "Взять займ". Дает 50 монет. Можно взять максимум 3 раза.
        /// </summary>
        public void TakeLoan()
        {
            if (IsGameOver) return;

            if (LoanCount < 3)
            {
                LoanCount++;
                Balance += 50; // Даем займ
            }
            // Если займов больше нет, просто ничего не делаем.
        }

        /// <summary>
        /// "Выдуманно сложная" логика проверки комбинаций.
        /// Шанс на выигрыш делаем маленьким.
        /// </summary>
        private void ApplyCombinationEffects()
        {
            // Вычисляем некий "индекс удачи" на основе цифр
            double luckIndex = CalculateLuckIndex();

            // Используем Постоянную Мамонта для порогов
            if (luckIndex > 10.0 * MammothConstant) // ~ 4.2 - редкий выигрыш
            {
                // Супер-выигрыш (например, все цифры одинаковые или что-то около того)
                if (AllNumbersSame())
                {
                    Balance += 100;
                }
                else // Просто большой выигрыш
                {
                    Balance += 50;
                }
            }
            else if (luckIndex < 2.5) // Обычный проигрыш (дополнительный штраф)
            {
                // Штраф, если баланс позволяет. Если нет - ничего.
                Balance = Math.Max(0, Balance - 15); // Не даем уйти в минус
            }
            // В остальных случаях просто прокрутка без изменений (уже сняли 10 монет)
        }

        /// <summary>
        /// Проверяет, все ли цифры в массиве одинаковые.
        /// </summary>
        private bool AllNumbersSame()
        {
            if (CurrentNumbers.Length == 0) return false;
            int first = CurrentNumbers[0];
            for (int i = 1; i < CurrentNumbers.Length; i++)
            {
                if (CurrentNumbers[i] != first) return false;
            }
            return true;
        }

        /// <summary>
        /// Самая "сложная" формула. Суммирует цифры, умножает на коэффициент хомяка,
        /// возводит в степень, делит... В общем, создает непредсказуемое число от 0 до ~15.
        /// </summary>
        private double CalculateLuckIndex()
        {
            double sum = 0;
            for (int i = 0; i < CurrentNumbers.Length; i++)
            {
                // Хитрое суммирование с весами
                sum += CurrentNumbers[i] * Math.Pow(HamsterWarmupCoefficient, i % 3);
            }

            // Применяем Постоянную Мамонта как делитель, чтобы создать хаос
            double result = (sum / MammothConstant) % 15; // Остаток от деления, чтобы было от 0 до 15
            return result;
        }

        /// <summary>
        /// Проверка условий проигрыша.
        /// </summary>
        private void CheckForGameOver()
        {
            // Если денег нет (0 или меньше) И займы закончились (уже взяли 3)
            if (Balance <= 0 && LoanCount >= 3)
            {
                IsGameOver = true;
                GameOverMessage = "Ты всё слил в нулину, побрили хомяка :(";
            }
            // Баланс не может быть отрицательным
            if (Balance < 0) Balance = 0;
        }
    }
}