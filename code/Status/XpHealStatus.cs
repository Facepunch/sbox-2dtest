using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f)]
public class XpHealStatus : Status
{
	public XpHealStatus()
    {
		Title = "XP Recovery";
		IconPath = "textures/icons/xp_heal.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("When you gain XP, heal for {0} per point", GetPrintAmountForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
	{
		return newLevel > 1 ? string.Format("When you gain XP, heal for {0} → {1} per point", GetPrintAmountForLevel(newLevel - 1), GetPrintAmountForLevel(newLevel)) : GetDescription(newLevel);
	}

    public override void OnGainExperience(int xp)
    {
        Player.RegenHealth(xp * GetAmountForLevel(Level));
    }

	public float GetAmountForLevel(int level)
	{
		return level * 0.22f;
	}

    public string GetPrintAmountForLevel(int level)
    {
        return string.Format("{0:0.00}", GetAmountForLevel(level));
    }
}
