using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f, typeof(DashFearStatus), typeof(GrenadeFearStatus))]
public class FearPainStatus : Status
{
	public FearPainStatus()
    {
		Title = "Killer Stress";
		IconPath = "textures/icons/fear_pain.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.FearPainPercent, GetAddForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Enemies you scare lose {0}% of their remaining HP each second", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Enemies you scare lose {0}% → {1}% of their remaining HP each second", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAddForLevel(int level)
    {
		return level * 0.07f;
    }

	public float GetPercentForLevel(int level)
	{
		return level * 7;
	}
}
