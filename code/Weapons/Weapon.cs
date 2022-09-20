using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox;

public partial class Weapon
{
    
    public PlayerCitizen PlayerOwner { get; protected set; }
    
    public Weapon(PlayerCitizen player)
    {
        PlayerOwner = player;
    }

    public virtual void Update(float dt)
    {
        
    }

    public virtual void Shoot()
    {
        
    }
}
