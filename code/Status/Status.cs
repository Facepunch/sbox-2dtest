using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sandbox.MyGame;

namespace Sandbox;

public partial class Status : BaseNetworkable
{
	public bool ShouldUpdate { get; protected set; }
	public PlayerCitizen Player { get; protected set; }
	[Net] public int Level { get; protected set; }
	public TimeSince ElapsedTime { get; protected set; }
	[Net] public string Description { get; protected set; }

	public Status()
	{

	}

	public virtual void Init(PlayerCitizen player)
    {
		Player = player;
		ElapsedTime = Time.Now;
		ShouldUpdate = true;
	}

	public virtual void Update(float dt)
    {
        //if (ElapsedTime > 10f)
        //    Player.RemoveStatus(this);
    }

	public virtual void Remove()
    {

    }
}
