using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(5, 0, 1f)]
public class MoreDamageLessHealthStatus : Status
{
	public MoreDamageLessHealthStatus()
    {
		Title = "Crystal Cannon";
		IconPath = "textures/icons/more_damage_less_health.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.OverallDamageMultiplier, GetDamageMultForLevel(Level), ModifierType.Mult);
        Player.Modify(this, PlayerStat.MaxHp, GetHealthMultForLevel(Level), ModifierType.Mult);

        if (Player.Health > Player.Stats[PlayerStat.MaxHp])
            Player.Health = Player.Stats[PlayerStat.MaxHp];
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("Deal {0}% more damage but reduce your max health by {1}%", GetDamagePercentForLevel(Level), GetHealthPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Deal {0}%→{1}% more damage but reduce your max health by {2}%→{3}%", GetDamagePercentForLevel(newLevel - 1), GetDamagePercentForLevel(newLevel), GetHealthPercentForLevel(newLevel - 1), GetHealthPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

    public float GetDamageMultForLevel(int level)
    {
        return 1f + 0.18f * level;
    }

    public float GetDamagePercentForLevel(int level)
    {
        return 18 * level;
    }

    public float GetHealthMultForLevel(int level)
    {
        return 1f - 0.15f * level;
    }

    public float GetHealthPercentForLevel(int level)
    {
        return 15 * level;
    }
}
