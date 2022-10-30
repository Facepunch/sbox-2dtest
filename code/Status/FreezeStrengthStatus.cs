using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(5, 0, 1f, typeof(FreezeShootStatus), typeof(FreezeArmorStatus))]
public class FreezeStrengthStatus : Status
{
	public FreezeStrengthStatus()
    {
		Title = "Frost Power";
		IconPath = "textures/icons/freeze_strength.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, nameof(Player.FreezeTimeScale), GetMultForLevel(Level), ModifierType.Mult);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("{0}% stronger freeze effect", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("{0}% → {1}% stronger freeze effect", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetMultForLevel(int level)
    {
		return 1f - (0.20f * level);
    }

	public float GetPercentForLevel(int level)
	{
		return 20 * level;
	}
}
