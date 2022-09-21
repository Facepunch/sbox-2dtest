using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sandbox.MyGame;

namespace Sandbox;

public class Status
{
	public bool ShouldUpdate { get; protected set; }
	public PlayerCitizen Player { get; protected set; }
	public int Level { get; protected set; }

	public Status()
	{

	}

	public virtual void Init(PlayerCitizen player)
    {
		Player = player;
    }

	public virtual void Update(float dt)
    {

    }

	public virtual void Remove()
    {

    }
}
