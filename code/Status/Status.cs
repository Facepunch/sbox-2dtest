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
	//public string StatusName { get; set; }

	public bool ShouldUpdate { get; protected set; }
	public PlayerCitizen Player { get; protected set; }
	[Net] public int Level { get; set; }
	[Net] public int MaxLevel { get; set; }
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
		ElapsedTime = 0f;
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

	public virtual string GetDescription(int newLevel)
	{
		return "...";
	}

	public virtual string GetUpgradeDescription(int newLevel)
	{
		return "...";
	}

	public virtual void Colliding(Thing other, float percent, float dt) { }
    public virtual void OnDashStarted() {}
	public virtual void OnDashFinished() { }
	public virtual void OnDashRecharged() { }
	public virtual void OnHit(Enemy enemy, bool isCritical) {}
    public virtual void OnBurn(Enemy enemy) { }
    public virtual void OnFreeze(Enemy enemy) { }
    public virtual void OnFear(Enemy enemy) { }
    public virtual void OnHurt(float amount) { }
}
