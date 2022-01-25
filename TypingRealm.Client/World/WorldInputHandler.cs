using System;
using System.Linq;
using TypingRealm.Client.Interaction;
using TypingRealm.Client.Typing;
using TypingRealm.World.Activities.RopeWar;
using TypingRealm.World.Movement;

namespace TypingRealm.Client.World;

public sealed class WorldInputHandler : MultiTyperInputHandler
{
    private readonly IScreenNavigation _screenNavigation;
    private readonly IConnectionManager _connectionManager;
    private readonly WorldScreenState _state;

    public WorldInputHandler(
        ITyperPool typerPool,
        ComponentPool componentPool,
        WorldScreenState state,
        IScreenNavigation screenNavigation,
        IConnectionManager connectionManager) : base(typerPool, componentPool)
    {
        _screenNavigation = screenNavigation;
        _connectionManager = connectionManager;
        _state = state;
    }

    protected override void OnTyped(Typer typer)
    {
        base.OnTyped(typer);

        if (typer == _state.Disconnect)
        {
            _connectionManager.DisconnectFromWorld();
            _screenNavigation.Screen = GameScreen.MainMenu;
            return;
        }

        if (typer == _state.CreateRopeWarTyper)
        {
            _ = _connectionManager.WorldConnection?.SendAsync(new ProposeRopeWarContest
            {
                Name = Guid.NewGuid().ToString(),
                Bet = 100,
                Side = RopeWarSide.Left
            }, default);
            return;
        }

        if (_state.CurrentRopeWar != null)
        {
            if (typer == _state.CurrentRopeWar.LeaveRopeWarTyper)
            {
                _ = _connectionManager.WorldConnection?.SendAsync(new LeaveJoinedRopeWarContest
                {
                }, default);
                return;
            }

            if (typer == _state.CurrentRopeWar.SwitchSides)
            {
                _ = _connectionManager.WorldConnection?.SendAsync(new SwitchSides(), default);
                return;
            }
        }

        foreach (var rw in _state.RopeWars)
        {
            if (typer == rw.Typer)
            {
                _ = _connectionManager.WorldConnection?.SendAsync(new JoinRopeWarContest
                {
                    RopeWarId = rw.RopeWarId,
                    Side = RopeWarSide.Left
                }, default);
                return;
            }
        }

        var entrance = _state.LocationEntrances.FirstOrDefault(
            e => e.Typer == typer);

        if (entrance != null)
        {
            _ = _connectionManager.WorldConnection?.SendAsync(new MoveToLocation
            {
                LocationId = entrance.LocationId
            }, default);
            return;
        }

        // Handle other actions.
    }

    protected override bool IsTyperDisabled(Typer typer)
    {
        var rwTyper = _state.RopeWars.FirstOrDefault(rw => rw.Typer == typer);
        if (rwTyper != null && _state.CurrentRopeWar != null)
            return false;

        if (typer == _state.CreateRopeWarTyper && _state.CurrentRopeWar != null)
            return false;

        return base.IsTyperDisabled(typer);
    }
}
