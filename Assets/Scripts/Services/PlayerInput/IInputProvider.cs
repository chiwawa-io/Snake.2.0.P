using UnityEngine;

namespace Services.PlayerInput
{
    public interface IInputProvider
    {
        void SetActive(bool isActive);
        void ProcessInput();
    }
}
