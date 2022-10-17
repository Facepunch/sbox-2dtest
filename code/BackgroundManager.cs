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
    public BackgroundManager()
    {

    }

    public void Restart()
    {
        foreach (var tile in _tiles)
            tile.Delete();

        _tiles.Clear();

        var boundsSize = MyGame.Current.BOUNDS_MAX - MyGame.Current.BOUNDS_MIN + new Vector2( 8f, 8f );

        var x_min = MyGame.Current.BOUNDS_MIN.x;
        var y_min = MyGame.Current.BOUNDS_MIN.y;
        var x_max = MyGame.Current.BOUNDS_MAX.x;
        var y_max = MyGame.Current.BOUNDS_MAX.y;

        var x = x_min - TILE_WIDTH / 2f - 0.2f;
        var y = y_min - TILE_HEIGHT / 2f - 0.2f;

        while (x < x_max + TILE_WIDTH)
        {
            y = y_min;

            while (y < y_max + TILE_HEIGHT)
            {
                AddTile(new Vector2(x, y));

                y += TILE_HEIGHT;
            }

            x += TILE_WIDTH;
        }
    }

    void AddTile(Vector2 pos)
    {
        _tiles.Add( new BackgroundTile
		{
			Depth = -512f,
			Scale = boundsSize,
			UvRect = new Rect( 0f, boundsSize / new Vector2( TILE_WIDTH, TILE_HEIGHT ) )
		} );
    }
}
