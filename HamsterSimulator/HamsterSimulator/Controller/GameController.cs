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
            _model.OnCollectorsTriggered += _view.TriggerCollectorsEffect;
        }

        public void Spin()
        {
            if (_model.IsGameOver) return;
            if (_model.Balance < _model.CalculateTotalSpinCost()) return;

            _model.Spin();
            _view.UpdateUI();
        }

        public void TakeLoan()
        {
            if (_model.IsGameOver) return;
            _model.TakeLoan();
            _view.UpdateUI();
        }

        public void TakeMicroLoan()
        {
            if (_model.IsGameOver) return;
            _model.TakeMicroLoan();
            _view.UpdateUI();
        }

        public void UseRigging()
        {
            if (_model.IsGameOver) return;
            _model.UseRigging();
            _view.UpdateUI();
        }

        public void ResetGame()
        {
            _model.ResetGame();
            _view.UpdateUI();
        }

        public int Balance => _model.Balance;
        public int[] CurrentNumbers => _model.CurrentNumbers;
        public bool IsGameOver => _model.IsGameOver;
        public string GameOverMessage => _model.GameOverMessage;
        public int LoanCount => _model.LoanCount;
        public int MicroLoanCount => _model.MicroLoanCount;
        public int SpinPenalty => _model.CalculateSpinPenalty();
        public int TotalSpinCost => _model.CalculateTotalSpinCost();
        public int RiggingUsesLeft => _model.RiggingUsesLeft;
        public bool CanUseRigging => _model.CanUseRigging;
        public int Health => _model.Health;
        public double CollectionChance => _model.CurrentCollectionChance;
        public int LudoDopamine => _model.LudoDopamine;
    }
}