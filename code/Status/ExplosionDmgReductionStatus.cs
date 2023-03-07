using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f, typeof(GrenadeShootReloadStatus), typeof(FearDropGrenadeStatus))]
public class ExplosionDmgReductionStatus : Status
{
    public ExplosionDmgReductionStatus()
    {
        Title = "Bombproof Armor";
        IconPath = "textures/icons/explosion_dmg_reduction.png";
    }

    public override void Init(PlayerCitizen player)
    {
        base.Init(player);
    }

    public override void Refresh()
    {
        Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.ExplosionDamageReductionPercent, GetReductionAddForLevel(Level), ModifierType.Add);
        Player.Modify(this, PlayerStat.NonExplosionDamageIncreasePercent, GetIncreaseAddForLevel(Level), ModifierType.Add);
    }

    public override string GetDescription(int newLevel)
    {
        return string.Format("You take {0}% less damage from explosions but {1}% more from non-explosions", GetReductionPercentForLevel(Level), GetIncreasePercentForLevel(Level));
    }

    public override string GetUpgradeDescription(int newLevel)
    {
        return newLevel > 1 ? string.Format("You take {0}% → {1}% less explosion damage but {2}% → {3}% more from non-explosions", GetReductionPercentForLevel(newLevel - 1), GetReductionPercentForLevel(newLevel), GetIncreasePercentForLevel(newLevel - 1), GetIncreasePercentForLevel(newLevel)) : GetDescription(newLevel);
    }

    public float GetReductionAddForLevel(int level) { return 0.1f * level; }
    public float GetReductionPercentForLevel(int level) { return 10 * level; }
    public float GetIncreaseAddForLevel(int level) { return 0.07f * level; }
    public float GetIncreasePercentForLevel(int level) { return 7 * level; }
}
