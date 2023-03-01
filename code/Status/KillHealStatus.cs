using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f)]
public class KillHealStatus : Status
{
	public KillHealStatus()
    {
		Title = "Sadist";
		IconPath = "textures/icons/blank_icon.png";
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
		return string.Format("Heal for {0} whenever you kill an enemy", GetPrintAmountForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
	{
		return newLevel > 1 ? string.Format("Heal for {0} → {1} whenever you kill an enemy", GetPrintAmountForLevel(newLevel - 1), GetPrintAmountForLevel(newLevel)) : GetDescription(newLevel);
	}

    public override void OnKill(Enemy enemy)
    {
		Player.RegenHealth(GetAmountForLevel(Level));
		// todo: sfx
    }

	public float GetAmountForLevel(int level)
	{
		return level * 0.35f;
	}

    public string GetPrintAmountForLevel(int level)
    {
        return GetAmountForLevel(level).ToString("#.##");
    }
}
