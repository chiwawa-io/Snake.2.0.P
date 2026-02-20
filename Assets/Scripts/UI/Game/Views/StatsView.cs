using TMPro;
using UI.Global;

namespace UI.Game.Views
{
    public class StatsView : BaseView
    { 
        private TextMeshProUGUI _gemsText; 
        private TextMeshProUGUI _preciousGemsText; 
        private TextMeshProUGUI _powerUpsText; 
        private TextMeshProUGUI _distanceText; 
        private TextMeshProUGUI _trapsText;

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