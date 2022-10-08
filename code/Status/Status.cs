using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public partial class Status : BaseNetworkable
{
	public bool ShouldUpdate { get; protected set; }
	public PlayerCitizen Player { get; protected set; }
	[Net] public int Level { get; set; }
	public TimeSince ElapsedTime { get; protected set; }
	[Net] public string Title { get; protected set; }
	[Net] public string Description { get; protected set; }
	[Net] public string IconPath { get; protected set; }

	public Status()
	{
		Level = 1;
	}

	public virtual void Init(PlayerCitizen player)
    {
		Player = player;
		ElapsedTime = Time.Now;
		ShouldUpdate = false;
	}

	// when gaining or leveling up
	public virtual void Refresh()
    {

    }

	public virtual void Update(float dt)
    {
        //if (ElapsedTime > 10f)
        //    Player.RemoveStatus(this);
    }

	public virtual void Remove()
    {

    }

	public virtual string GetUpgradeDescription(int newLevel)
	{
		return "...";
	}
}
