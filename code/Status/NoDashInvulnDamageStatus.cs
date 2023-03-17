using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(1, 0, 1f)]
public class NoDashInvulnDamageStatus : Status
{
	public NoDashInvulnDamageStatus()
    {
		Title = "Overconfidence";
		IconPath = "textures/icons/no_dash_invuln_damage_status.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.OverallDamageMultiplier, GetDamageMultForLevel(Level), ModifierType.Mult);
        Player.Modify(this, PlayerStat.NoDashInvuln, 1f, ModifierType.Add);
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("Deal {0}% more damage but you're no longer invulnerable while dashing", GetDamagePercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Deal {0}%→{1}% more damage but you're no longer invulnerable while dashing", GetDamagePercentForLevel(newLevel - 1), GetDamagePercentForLevel(newLevel)) : GetDescription(newLevel);
	}

    public float GetDamageMultForLevel(int level)
    {
        return 1f + 0.25f * level;
    }

    public float GetDamagePercentForLevel(int level)
    {
        return 25 * level;
    }
}
