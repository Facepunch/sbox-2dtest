using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f, typeof(MoreRerollsStatus))]
public class RerollHealStatus : Status
{
	public RerollHealStatus()
    {
		Title = "Reroll Recovery";
		IconPath = "textures/icons/reroll_heal.png";
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
		return string.Format("When you reroll, heal for {0}", GetPrintAmountForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
	{
		return newLevel > 1 ? string.Format("When you reroll, heal for {0}→{1}", GetPrintAmountForLevel(newLevel - 1), GetPrintAmountForLevel(newLevel)) : GetDescription(newLevel);
	}

    public override void OnReroll()
    {
        Player.RegenHealth(GetAmountForLevel(Level));
    }

	public float GetAmountForLevel(int level)
	{
		return level * 1.2f;
	}

    public string GetPrintAmountForLevel(int level)
    {
        return string.Format("{0:0.0}", GetAmountForLevel(level));
    }
}
