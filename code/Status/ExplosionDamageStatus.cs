using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f, typeof(GrenadeShootReloadStatus))]
public class ExplosionDamageStatus : Status
{
    public ExplosionDamageStatus()
    {
        Title = "Gunpowder Expert";
        IconPath = "textures/icons/blank_icon.png";
    }

    public override void Init(PlayerCitizen player)
    {
        base.Init(player);
    }

    public override void Refresh()
    {
        Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.ExplosionDamageMultiplier, GetExplosionMultForLevel(Level), ModifierType.Mult);
        Player.Modify(this, PlayerStat.BulletDamage, GetBulletMultForLevel(Level), ModifierType.Mult);
    }

    public override string GetDescription(int newLevel)
    {
        return string.Format("Your explosions deal {0}% extra damage but your bullets deal {1}% less", GetExplosionPercentForLevel(Level), GetBulletPercentForLevel(Level));
    }

    public override string GetUpgradeDescription(int newLevel)
    {
        return newLevel > 1 ? string.Format("Your explosions deal {0}% → {1}% extra damage but your bullets deal {2}% → {3}% less", GetExplosionPercentForLevel(newLevel - 1), GetExplosionPercentForLevel(newLevel), GetBulletPercentForLevel(newLevel - 1), GetBulletPercentForLevel(newLevel)) : GetDescription(newLevel);
    }

    public float GetExplosionMultForLevel(int level) { return 1f + 0.2f * level; }
    public float GetExplosionPercentForLevel(int level) { return 20 * level; }
    public float GetBulletMultForLevel(int level) { return 1f - 0.15f * level; }
    public float GetBulletPercentForLevel(int level) { return 15 * level; }
}
