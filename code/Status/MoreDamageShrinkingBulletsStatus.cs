using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 111f)]
public class MoreDamageShrinkingBulletsStatus : Status
{
	public MoreDamageShrinkingBulletsStatus()
    {
		Title = "Frontloaded";
		IconPath = "textures/icons/blank_icon.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.BulletFlatDamageAddition, GetDamageAdd(Level), ModifierType.Add);
        Player.Modify(this, PlayerStat.BulletDamageShrink, GetShrinkForLevel(Level), ModifierType.Add);
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("Your bullets do +{0} damage but shrink by {1} damage per second", GetDamageAddPrint(Level), GetShrinkPrintForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format(" Your bullets do +{0} → +{1} damage but shrink by {2} → {3} damage per second", GetDamageAddPrint(newLevel - 1), GetDamageAddPrint(newLevel), GetShrinkPrintForLevel(newLevel - 1), GetShrinkPrintForLevel(newLevel)) : GetDescription(newLevel);
	}

    public float GetDamageAdd(int level)
    {
        return 1f * level;
    }

    public float GetDamageAddPrint(int level)
    {
        return 1 * level;
    }

    public float GetShrinkForLevel(int level)
    {
        return 2f * level;
    }

    public float GetShrinkPrintForLevel(int level)
    {
        return 2 * level;
    }
}
