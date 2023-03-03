using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(1, 0, 1f)]
public class LessChoicesDamageStatus : Status
{
	public LessChoicesDamageStatus()
    {
		Title = "Strong Faith";
		IconPath = "textures/icons/blank_icon.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.NumUpgradeChoices, -GetAddForLevel(Level), ModifierType.Add);
        Player.Modify(this, PlayerStat.OverallDamageMultiplier, GetDamageMultForLevel(Level), ModifierType.Mult);
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("Deal {0}% more damage but see {1} less upgrade choice", GetDamagePercentForLevel(Level), GetAddForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Deal {0}% → {1}% more damage but see {2} → {3} less upgrade choices", GetDamagePercentForLevel(newLevel - 1), GetDamagePercentForLevel(newLevel), GetAddForLevel(newLevel - 1), GetAddForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAddForLevel(int level)
    {
		return 1f * level;
    }

    public float GetDamageMultForLevel(int level)
    {
        return 1f + 0.2f * level;
    }

    public float GetDamagePercentForLevel(int level)
    {
        return 20 * level;
    }
}
