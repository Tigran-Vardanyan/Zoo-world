namespace ZooWorld.Cameras
{
    /// <summary>Switches between the fixed arena view and the player-controlled free camera.</summary>
    public interface ICameraSwitcher
    {
        bool IsFree { get; }

        void SetFree(bool isFree);

        /// <summary>Positive = zoom in. Only affects the free camera.</summary>
        void Zoom(float amount);

        /// <summary>Puts the free camera back to the arena view.</summary>
        void ResetView();
    }
}
