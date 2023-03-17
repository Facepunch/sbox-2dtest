using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(4, 0, 1f)]
public class NumDashesStatus : Status
{
	public NumDashesStatus()
    {
		Title = "More Dashes";
		IconPath = "textures/icons/num_dashes.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.NumDashes, GetNumDashesForLevel(Level), ModifierType.Add);
        Player.Modify(this, PlayerStat.MaxHp, GetHealthMultForLevel(Level), ModifierType.Mult);

		if (Player.Health > Player.Stats[PlayerStat.MaxHp])
			Player.Health = Player.Stats[PlayerStat.MaxHp];
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase num dashes by {0} but reduce max health by {1}%", GetNumDashesForLevel(Level), GetHealthPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase num dashes by {0}→{1} but reduce max health by {2}%→{3}%", GetNumDashesForLevel(newLevel - 1), GetNumDashesForLevel(newLevel), GetHealthPercentForLevel(newLevel - 1), GetHealthPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetNumDashesForLevel(int level)
	{
		return level;
	}

    public float GetHealthMultForLevel(int level)
    {
        return 1f - 0.1f * level;
    }

    public float GetHealthPercentForLevel(int level)
    {
        return 10 * level;
    }
}
