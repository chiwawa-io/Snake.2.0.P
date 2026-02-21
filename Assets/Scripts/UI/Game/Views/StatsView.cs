using TMPro;
using UI.Global;
using UnityEngine;

namespace UI.Game.Views
{
    public class StatsView : BaseView
    { 
        [Header("Stat Texts")]
        [SerializeField] private TextMeshProUGUI _gemsText; 
        [SerializeField] private TextMeshProUGUI _preciousGemsText; 
        [SerializeField] private TextMeshProUGUI _powerUpsText; 
        [SerializeField] private TextMeshProUGUI _distanceText; 
        [SerializeField] private TextMeshProUGUI _trapsText;

        public void DisplayStats(int gems, int precious, int powerups, int distance, int traps)
        {
            if (_gemsText) _gemsText.text = gems.ToString();
            if (_preciousGemsText) _preciousGemsText.text = precious.ToString();
            if (_powerUpsText) _powerUpsText.text = powerups.ToString();
            if (_distanceText) _distanceText.text = distance.ToString();
            if (_trapsText) _trapsText.text = traps.ToString();
        }
    }
}