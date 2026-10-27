using System;

namespace ZooWorld.Statistics
{
    public interface IDeathStatistics
    {
        event Action Changed;

        int DeadPrey { get; }
        int DeadPredators { get; }
    }
}
