using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(4, 0, 1f)]
public class FreezeArmorStatus : Status
{
	public FreezeArmorStatus()
    {
		Title = "Frozen Skin";
		IconPath = "textures/icons/freeze_armor.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.FreezeOnMeleeChance, GetAddForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("{0}% chance to freeze enemy melee attackers", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("{0}%→{1}% chance to freeze enemy melee attackers", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAddForLevel(int level)
    {
		return 0.25f * level;
    }

	public float GetPercentForLevel(int level)
	{
		return 25 * level;
	}
}
