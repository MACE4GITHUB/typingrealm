﻿using System.Collections.Generic;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.World
{
    public sealed record LocationInfo(
        string LocationId,
        string Name,
        string Description);

    public sealed record LocationEntrance(
        string LocationId,
        string Name,
        Typer Typer);

    public sealed class WorldScreenState
    {
        private readonly ITyperPool _typerPool;

        public WorldScreenState(
            ITyperPool typerPool)
        {
            _typerPool = typerPool;
            CurrentLocation = null;

            Disconnect = _typerPool.MakeUniqueTyper("disconnect");
        }

        public Typer Disconnect { get; }

        public LocationInfo? CurrentLocation { get; set; }
        public bool IsLoading => CurrentLocation == null;

        public HashSet<LocationEntrance> LocationEntrances { get; set; }
            = new HashSet<LocationEntrance>();
    }
}
