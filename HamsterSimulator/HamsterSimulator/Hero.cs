using System;

namespace HamsterSimulator.Model
{
    public class Hero
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int StartingBalance { get; set; } = 100;
        public int StartingHealth { get; set; } = 5;
        public int ExtraRigging { get; set; } = 0;

        // Пассивные способности
        public int ExtraLoans { get; set; } = 0;
        public int ExtraMicroLoans { get; set; } = 0;
        public int SpinCostExtra { get; set; } = 0;
        public double WinBonusMultiplier { get; set; } = 1.0;
        public double CollectionResistance { get; set; } = 0.0;

        // Активная способность
        public bool IsActiveAbility { get; set; } = false;
        public Action<GameState> OnActivate { get; set; }

        public static Hero CreateDefault()
        {
            return new Hero
            {
                Name = "Хомяк Обычный",
                Description = "Стандартный хомяк. Без особых способностей.",
                StartingBalance = 100,
                StartingHealth = 5,
                IsActiveAbility = false
            };
        }

        public static Hero CreateKalivan()
        {
            var hero = new Hero
            {
                Name = "Калыван",
                Description = "Зажиточный. Один раз может сыграть в игру: угадай число от 1 до 10. Получишь-таки гешефт!",
                StartingBalance = 200,
                IsActiveAbility = true
            };
            hero.OnActivate = (game) => KalivanAbility(game);
            return hero;
        }

        public static Hero CreateInfluencer()
        {
            return new Hero
            {
                Name = "Инфлюенсер",
                Description = "Все его знают, кредитов одобряют больше и в карамане 300, но спин дороже на 5.",
                StartingBalance = 300,
                StartingHealth = 5,
                ExtraLoans = 2,
                ExtraMicroLoans = 2,
                SpinCostExtra = 5,
                IsActiveAbility = false
            };
        }

        public static Hero CreateTank()
        {
            return new Hero
            {
                Name = "Хомячьё",
                Description = "Много здоровья (10), реально много.",
                StartingBalance = 50,
                StartingHealth = 10,
                IsActiveAbility = false
            };
        }

        private static void KalivanAbility(GameState game)
        {
            if (game.KalivanUsed) return;
            game.KalivanUsed = true;
            // Запрашиваем ввод через событие (обрабатывается в MainForm)
            game.RequestKalivanGuess();
        }
    }
}