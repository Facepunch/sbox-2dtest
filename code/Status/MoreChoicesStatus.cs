using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(3, 0, 1f)]
public class MoreChoicesStatus : Status
{
	public MoreChoicesStatus()
    {
		Title = "More Choices";
		IconPath = "textures/icons/more_choices.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.NumUpgradeChoices, GetAddForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("See {0} extra upgrade choice", GetAddForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("See {0} → {1} extra upgrade choices", GetAddForLevel(newLevel - 1), GetAddForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAddForLevel(int level)
    {
		return 1f * level;
    }
}
