using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

public class MovespeedStatus : Status
{
	public MovespeedStatus()
    {
		Title = "Fast Shoes";
		IconPath = "textures/icons/shoe.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = string.Format("Increase movespeed by {0}x", GetMultForLevel(Level));

		Player.Modify(this, "MoveSpeed", GetMultForLevel(Level), ModifierType.Mult);
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase movespeed by {0}x -> {1}x", GetMultForLevel(newLevel - 1), GetMultForLevel(newLevel)) : string.Format("Increase movespeed by {0}x", GetMultForLevel(newLevel));
	}

	public float GetMultForLevel(int level)
    {
		return 1f + 0.2f * level;
    }
}
