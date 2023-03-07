using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f, typeof(DashFearStatus), typeof(GrenadeFearStatus))]
public class FearDrainStatus : Status
{
	public FearDrainStatus()
    {
		Title = "Soul Drainer";
		IconPath = "textures/icons/fear_drain.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.FearDrainPercent, GetAddForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("You heal for {0}% of the damage you do to scared enemies", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("You heal for {0}% → {1}% of the damage you do to scared enemies", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAddForLevel(int level)
    {
		return level * 0.09f;
    }

	public float GetPercentForLevel(int level)
	{
		return level * 9;
	}
}
