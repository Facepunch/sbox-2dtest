using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Test2D;

// CLIENT-ONLY
public class BackgroundManager
{
    public const float TILE_WIDTH = 8f;
    public const float TILE_HEIGHT = 3f;

    private List<BackgroundTile> _tiles = new List<BackgroundTile>();

    public void Restart()
    {
        foreach (var tile in _tiles)
            tile.Delete();

        _tiles.Clear();

        var boundsSize = MyGame.Current.BOUNDS_MAX - MyGame.Current.BOUNDS_MIN + new Vector2( 8f, 8f );

        _tiles.Add( new BackgroundTile
        {
	        Depth = -511f,
	        Scale = boundsSize + new Vector2(-7.8f, -7.9f),
	        UvRect = new Rect( 0f, boundsSize / new Vector2( TILE_WIDTH, TILE_HEIGHT ) )
        } );

		_tiles.Add(new BackgroundTile
		{
			Depth = -512f,
			Scale = (boundsSize) * 2,
            ColorTint = new Color(0.025f, 0.0325f, 0.01f, 1f).Darken( 0.24f ),
			UvRect = new Rect(0f, (boundsSize * 2) / new Vector2(TILE_WIDTH, TILE_HEIGHT))
		});
	}
}
