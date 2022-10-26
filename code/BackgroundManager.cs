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
	        Depth = -512f,
	        Scale = boundsSize,
	        UvRect = new Rect( 0f, boundsSize / new Vector2( TILE_WIDTH, TILE_HEIGHT ) )
        } );
	}
}
