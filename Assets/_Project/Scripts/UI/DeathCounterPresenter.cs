using System;
using VContainer.Unity;
using ZooWorld.Statistics;

namespace ZooWorld.UI
{
    /// <summary>
    /// Keeps <see cref="DeathCounterView"/> in sync with <see cref="IDeathStatistics"/>.
    /// </summary>
    public sealed class DeathCounterPresenter : IStartable, IDisposable
    {
        private readonly IDeathStatistics _statistics;
        private readonly DeathCounterView _view;

        public DeathCounterPresenter(IDeathStatistics statistics, DeathCounterView view)
        {
            _statistics = statistics;
            _view = view;
        }

        public void Start()
        {
            _statistics.Changed += Refresh;
            Refresh();
        }

        public void Dispose()
        {
            _statistics.Changed -= Refresh;
        }

        private void Refresh()
        {
            _view.Show(_statistics.DeadPrey, _statistics.DeadPredators);
        }
    }
}
