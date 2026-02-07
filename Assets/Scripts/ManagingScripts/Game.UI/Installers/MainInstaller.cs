using Game.Core;
using UnityEngine;
using Zenject;

public class MainInstaller : MonoInstaller
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private PlayerDataManager playerDataManager;
    [SerializeField] private InactivityDetector inactivityDetector;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private NetworkManager networkManager;

    public override void InstallBindings()
    {
        Container.Bind<AudioManager>()
            .FromInstance(audioManager)
            .AsSingle();
        
        Container.Bind<InactivityDetector>()
            .FromInstance(inactivityDetector)
            .AsSingle();
        
        Container.Bind<PlayerDataManager>()
            .FromInstance(playerDataManager)
            .AsSingle()
            .NonLazy();
        
        Container.Bind<GameManager>().
            FromInstance(gameManager).
            AsSingle().
            NonLazy();
        
        Container.Bind<NetworkManager>().
            FromInstance(networkManager).
            AsSingle().
            NonLazy();
    }
}
