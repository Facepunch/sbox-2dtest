using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox;
using System.Linq;

/// <summary>
/// When a player is within radius of the camera we add this to their entity.
/// We remove it again when they go out of range.
/// </summary>
internal class NameTagComponent : EntityComponent<PlayerCitizen>
{
	NameTag NameTag;

	protected override void OnActivate()
	{
		NameTag = new NameTag( Entity.Client?.Name ?? Entity.Name, Entity.Client?.PlayerId );
		Log.Info("Activate: " + NameTag.ToString());

	}

	protected override void OnDeactivate()
	{
		NameTag?.Delete();
		NameTag = null;
	}

	/// <summary>
	/// Called for every tag, while it's active
	/// </summary>
	[Event.Frame]
	public void FrameUpdate()
	{
        //var tx = Entity.Transform;
        //tx.Position += Vector3.Up * 0.1f;
        //tx.Rotation = Rotation.LookAt( -CurrentView.Rotation.Forward );

        //NameTag.Transform = tx;

        NameTag.Position = Entity.Position;
		//NameTag.Rotation = Rotation.From(0f, 0f, 0f);
		NameTag.Rotation = Rotation.LookAt(-CurrentView.Rotation.Forward);
	}

	/// <summary>
	/// Called once per frame to manage component creation/deletion
	/// </summary>
	[Event.Frame]
	public static void SystemUpdate()
	{
		foreach ( var player in Sandbox.Entity.All.OfType<PlayerCitizen>() )
		{
			//if ( player.IsLocalPawn && player.IsFirstPersonMode)// || BoomerCamera.Target == player )
			//{
			//	var c = player.Components.Get<NameTagComponent>();
			//	c?.Remove();
			//	continue;
			//}

			//var shouldRemove = player.Position.Distance( CurrentView.Position ) > 500;
			//shouldRemove = shouldRemove || player.LifeState != LifeState.Alive;
			//shouldRemove = shouldRemove || player.IsDormant;

			//if ( shouldRemove )
			//{
			//	var c = player.Components.Get<NameTagComponent>();
			//	c?.Remove();
			//	continue;
			//}

			// Add a component if it doesn't have one
			//player.Components.GetOrCreate<NameTagComponent>();
		}
	}
}

/// <summary>
/// A nametag panel in the world
/// </summary>
public class NameTag : WorldPanel
{
	public Panel Avatar;
	public Label NameLabel;

	internal NameTag( string title, long? steamid )
	{
		StyleSheet.Load( "Resource/styles/nametag.scss" );

		if ( steamid != null )
		{
			Avatar = Add.Panel( "avatar" );
			Avatar.Style.SetBackgroundImage( $"avatar:{steamid}" );
		}

		NameLabel = Add.Label( title, "title" );

		// this is the actual size and shape of the world panel
		PanelBounds = new Rect( -100f, -100f, 200f, 200f);
	}
}
