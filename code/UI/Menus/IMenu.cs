﻿using Sandbox;
using Sandcube.Players;

namespace Sandcube.UI.Menus;

public interface IMenu : IValid
{
    bool IsOpened { get; }

    void Open();
    void Close();

    bool StillValid(SandcubePlayer player) => true;
}
