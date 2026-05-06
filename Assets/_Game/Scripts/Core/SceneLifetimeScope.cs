using BillGameCore.Interfaces;
using BillGameCore.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class SceneLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private PlayerView playerView;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<PlayerManager>(Lifetime.Scoped)
              .As<IPlayerService>()
              .WithParameter(playerData)
              .WithParameter(playerView);
    }
}
