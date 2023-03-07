using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 3, 1f)]
public class KillHealStatus : Status
{
	public KillHealStatus()
    {
		Title = "Sadistic Masochist";
		IconPath = "textures/icons/kill_heal.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.HealthRegen, -GetHpDrainAmountForLevel(Level), ModifierType.Add);
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("Heal for {0} whenever you kill an enemy but lose {1} HP/s", GetPrintAmountForLevel(Level), GetHpDrainPrintAmountForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
	{
		return newLevel > 1 ? string.Format("Heal for {0} → {1} whenever you kill an enemy but lose {2} → {3} HP/s", GetPrintAmountForLevel(newLevel - 1), GetPrintAmountForLevel(newLevel), GetHpDrainPrintAmountForLevel(newLevel - 1), GetHpDrainPrintAmountForLevel(newLevel)) : GetDescription(newLevel);
	}

    public override void OnKill(Enemy enemy)
    {
		Player.RegenHealth(GetAmountForLevel(Level));
    }

	public float GetAmountForLevel(int level)
	{
		return level * 0.4f;
	}

    public string GetPrintAmountForLevel(int level)
    {
        return GetAmountForLevel(level).ToString("#.##");
    }

    public float GetHpDrainAmountForLevel(int level)
    {
        return level * 0.3f;
    }

    public string GetHpDrainPrintAmountForLevel(int level)
    {
        return GetHpDrainAmountForLevel(level).ToString("#.##");
    }
}
