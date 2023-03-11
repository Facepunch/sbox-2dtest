using Sandbox;
using Sandbox.UI.Construct;
using Sandbox.UI;

namespace Test2D
{
	public partial class BoomerChatBox : Panel
	{
		static BoomerChatBox Current;

		public Panel Canvas { get; protected set; }
		public TextEntry Input { get; protected set; }

		public bool IsOpen
		{
			get => HasClass( "open" );
			set
			{
				if ( IsOpen ) Close();
				else Open();
			}
		}

		public BoomerChatBox()
		{
			Current = this;

			StyleSheet.Load( "/ui/chat/BoomerChatBox.scss" );

			Canvas = Add.Panel( "chat_canvas" );
			Canvas.PreferScrollToBottom = true;

			Input = Add.TextEntry( "" );
			Input.AddEventListener( "onsubmit", () => Submit() );
			Input.AddEventListener( "onblur", () => Close() );
			Input.AcceptsFocus = true;
			Input.AllowEmojiReplace = true;
		}

		void Open()
		{
			AddClass( "open" );
			Input.Focus();
			Canvas.TryScrollToBottom();
		}

		void Close()
		{
			RemoveClass( "open" );
			Input.Blur();
		}

		void Submit()
		{
			Close();

			var msg = Input.Text.Trim();
			Input.Text = "";

			if ( string.IsNullOrWhiteSpace( msg ) )
				return;

			Chat( msg );
		}

		public void AddEntry( string name, string message, string avatar, string lobbyState = null )
		{
			var e = Canvas.AddChild<BoomerChatEntry>();

			e.Message.Text = message;
			e.NameLabel.Text = name;
			e.Avatar.SetTexture( avatar );

			e.SetClass( "noname", string.IsNullOrEmpty( name ) );
			e.SetClass( "noavatar", string.IsNullOrEmpty( avatar ) );

			if ( lobbyState == "ready" || lobbyState == "staging" )
			{
				e.SetClass( "is-lobby", true );
			}

			Canvas.TryScrollToBottom();
		}

		[Event.Client.BuildInput]
		private void OnBuildInput()
		{
			if ( Sandbox.Input.Pressed( InputButton.Chat ) )
			{
				IsOpen = !IsOpen;
			}
		}


		[ConCmd.Client( "chat_addentry", CanBeCalledFromServer = true )]
		public static void AddChatEntry( string name, string message, string avatar = null, string lobbyState = null )
		{
			Current?.AddEntry( name, message, avatar, lobbyState );

			// Only log clientside if we're not the listen server Game
			if ( !Game.IsListenServer )
			{
				Log.Info( $"{name}: {message}" );
			}
		}

		[ConCmd.Client( "chat_addinfomation", CanBeCalledFromServer = true )]
		public static void AddInformation( string message, string avatar = null )
		{
			Current?.AddEntry( null, message, avatar );
		}

		[ConCmd.Server( "chat" )]
		public static void Chat( string message )
		{
			//Assert.NotNull( ConsoleSystem.Caller );
			//dunno how to fix
			
			// todo - reject more stuff
			if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
				return;

			Log.Info( $"{ConsoleSystem.Caller}: {message}" );
			AddChatEntry( To.Everyone, ConsoleSystem.Caller.Name, message, $"avatar:{ConsoleSystem.Caller.SteamId}" );
		}
	}
}

