// Services/Input/InputService.cs

using Core.Enums;
using Core.Events;
using UnityEngine;
using Zenject;

public class InputService : IInitializable, ITickable
{
    private readonly SignalBus _signalBus;
    private bool _isActive = true;
    private Vector2Int _lastInput = Vector2Int.up;

    public InputService(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    public void Initialize()
    {
        _signalBus.Subscribe<GameStateChangedSignal>(OnStateChanged);
    }

    public void Tick()
    {
        if (!_isActive) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2Int dir = Vector2Int.zero;

        if (Mathf.Abs(h) > 0.1f)
        {
            dir = new Vector2Int(h > 0 ? 1 : -1, 0);
        }
        else if (Mathf.Abs(v) > 0.1f)
        {
            dir = new Vector2Int(0, v > 0 ? 1 : -1);
        }

        if (dir != Vector2Int.zero)
        {
            _signalBus.Fire(new InputDirectionSignal { Direction = dir });
        }
    }

    private void OnStateChanged(GameStateChangedSignal signal)
    {
        _isActive = (signal.NewState == GameState.InGame);
    }
}