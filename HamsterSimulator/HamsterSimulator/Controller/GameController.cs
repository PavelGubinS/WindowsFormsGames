using System;
using HamsterSimulator.Model;
using HamsterSimulator.View;

namespace HamsterSimulator.Controller
{
    public class GameController
    {
        private GameState _model;
        private MainForm _view;

        public GameController(MainForm view)
        {
            _view = view;
            _model = new GameState();
        }

        // Метод для вращения рулетки
        public void Spin()
        {
            if (_model.IsGameOver) return;
            if (_model.Balance < 10) return;

            _model.Spin();
            _view.UpdateUI(); // обновляем представление
        }

        // Метод для взятия займа
        public void TakeLoan()
        {
            if (_model.IsGameOver) return;

            _model.TakeLoan();
            _view.UpdateUI();
        }

        // Метод для сброса игры
        public void ResetGame()
        {
            _model.ResetGame();
            _view.UpdateUI();
        }

        // Свойства для доступа к состоянию модели (чтобы представление могло их отображать)
        public int Balance => _model.Balance;
        public int[] CurrentNumbers => _model.CurrentNumbers;
        public bool IsGameOver => _model.IsGameOver;
        public string GameOverMessage => _model.GameOverMessage;
    }
}