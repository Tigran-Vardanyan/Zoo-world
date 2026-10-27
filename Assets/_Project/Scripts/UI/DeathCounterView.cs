using TMPro;
using UnityEngine;

namespace ZooWorld.UI
{
    /// <summary>
    /// Passive view of the dead animals counter; knows nothing about where the numbers come from.
    /// </summary>
    public sealed class DeathCounterView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _preyText;
        [SerializeField] private TMP_Text _predatorsText;
        [SerializeField] private string _preyFormat = "Dead prey: {0}";
        [SerializeField] private string _predatorsFormat = "Dead predators: {0}";

        public void Show(int deadPrey, int deadPredators)
        {
            _preyText.SetText(string.Format(_preyFormat, deadPrey));
            _predatorsText.SetText(string.Format(_predatorsFormat, deadPredators));
        }
    }
}
