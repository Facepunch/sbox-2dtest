using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public abstract partial class EnemyStatus : BaseNetworkable
{
	//public string StatusName { get; set; }

	public bool ShouldUpdate { get; protected set; }
	public Enemy Enemy { get; protected set; }
	public TimeSince ElapsedTime { get; protected set; }
	public float Priority { get; set; }

	public EnemyStatus()
	{
		
	}

	public virtual void Init(Enemy enemy)
    {
		Enemy = enemy;
		ElapsedTime = 0f;
		ShouldUpdate = true;
	}

	public virtual void Update(float dt)
    {
        
    }

	public virtual void StartDying()
	{

	}

	public virtual void Remove()
    {

    }

	public virtual void Refresh()
    {

    }

	public virtual void Colliding(Thing other, float percent, float dt)
	{

	}
}
