using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(4, 0, 1f)]
public class PiercingStatus : Status
{
	public PiercingStatus()
    {
		Title = "Piercing";
		IconPath = "textures/icons/piercing.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.BulletNumPiercing, GetNumPiercingForLevel(Level), ModifierType.Add);
		Player.Modify(this, PlayerStat.BulletDamage, GetDamageMultForLevel(Level), ModifierType.Mult);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase bullet pierces by {0} but reduce bullet damage by {1}%", GetNumPiercingForLevel(Level), GetDamagePercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase bullet pierces by {0} → {1} but reduce bullet damage by {2}% → {3}%", GetNumPiercingForLevel(newLevel - 1), GetNumPiercingForLevel(newLevel), GetDamagePercentForLevel(newLevel - 1), GetDamagePercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetNumPiercingForLevel(int level)
	{
		return level;
	}

	public float GetDamageMultForLevel(int level)
	{
		switch(level)
		{
			case 1: return 0.75f;
            case 2: return 0.6f;
            case 3: return 0.45f;
            case 4: return 0.3f;
        }

		return 1f;
	}

	public float GetDamagePercentForLevel(int level)
	{
        switch (level)
        {
            case 1: return 25f;
            case 2: return 40f;
            case 3: return 55f;
            case 4: return 7f;
        }

        return 1f;
    }
}
