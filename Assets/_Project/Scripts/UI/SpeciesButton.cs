using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZooWorld.UI
{
    /// <summary>One "Spawn &lt;species&gt;" button of the control panel; instantiated per catalog entry.</summary>
    public sealed class SpeciesButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;

        public event Action<int> Clicked;

        public void Bind(int index, string speciesName)
        {
            _label.SetText($"+ {speciesName}");
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => Clicked?.Invoke(index));
        }
    }
}
