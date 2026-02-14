using System;
using System.Collections;
using Core.Events;
using UnityEngine;
using Zenject;

public class ItemLifeSpan : MonoBehaviour
{
    [SerializeField] private float lifeSpan;
    [SerializeField] private bool isOnBomb;

    private Animator _animator;
    private SignalBus _signalBus;
    private DiContainer _container;

    [Inject]
    public void Construct(SignalBus signalBus, DiContainer container)
    {
        _signalBus = signalBus;
        _container = container;
    }

    private void Start()
    {
        _signalBus.Subscribe<PreciousGemEatenSignal>(Destroy);

        if (isOnBomb)
            _animator = GetComponent<Animator>();
    }

    private void OnDisable()
    {
        _signalBus.Unsubscribe<PreciousGemEatenSignal>(Destroy);
    }

    private void Destroy()
    {
        StartCoroutine(DestroyAfterTime());
    } 
        
    private IEnumerator DestroyAfterTime()
    {
        if (isOnBomb) _animator.enabled = true;
        yield return new WaitForSeconds(lifeSpan);
        _signalBus.Fire(new ItemDestroyedSignal {ItemPosition = Vector2Int.RoundToInt(this.transform.position)});
        Destroy(gameObject);
    }
}
