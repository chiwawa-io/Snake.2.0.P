using UnityEngine;

namespace Game.UI
{
    public class LoadingScreenUIController : BaseUIController
    {
        [SerializeField] private GameObject loadingScreen;

        public override void Show()
        {
            OnShow();
        }

        public override void Hide()
        {
            OnHide();
        }

        protected override void OnShow()
        {
            loadingScreen.SetActive(true);
            GameManager.StateChange?.Invoke("loadingScreen");
        }

        protected override void OnHide()
        {
            loadingScreen.SetActive(false);
        }
    }
}


