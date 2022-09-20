using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox;

public partial class Pistol : Weapon
{
    public float Timer { get; protected set; }
    public float ShotDelay { get; protected set; }
    public int AmmoCount { get; protected set; }
    public int MaxAmmoCount { get; protected set; }

    public bool IsReloading { get; protected set; }
    public float ReloadTime { get; protected set; }

    public Pistol(PlayerCitizen player) : base(player)
    {
        Timer = ShotDelay = 0.125f;
        AmmoCount = MaxAmmoCount = 6;
        ReloadTime = 1.25f;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        Timer -= dt * PlayerOwner.AttackSpeed;
        if (Timer <= 0f)
        {
            if(IsReloading)
            {
                IsReloading = false;
                AmmoCount = MaxAmmoCount;
            }

            Shoot();

            AmmoCount--;
            if(AmmoCount <= 0)
            {
                IsReloading = true;
                Timer += ReloadTime;
            } 
            else
            {
                Timer += ShotDelay;
            }
        }

        PlayerOwner.DebugText(AmmoCount.ToString());
    }

    public override void Shoot()
    {
        base.Shoot();

        var bullet = new Bullet
        {
            Position = PlayerOwner.Position,
            Depth = -1f,
            Velocity = PlayerOwner.AimDir * 7.5f,
            Shooter = PlayerOwner,
            Damage = 10f,
            Force = 2.25f,
            TempWeight = 3f,
            Lifetime = 1.5f,
        };

        PlayerOwner.Game.AddThing(bullet);
    }
}
