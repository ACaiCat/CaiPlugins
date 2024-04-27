namespace HousingPlugin;

public class LPlayer
{
	public int Who { get; set; }

	public int TileX { get; set; }

	public int TileY { get; set; }

	public bool Look { get; set; }

	public LPlayer(int who, int lasttileX, int lasttileY)
	{
		Who = who;
		TileX = lasttileX;
		TileY = lasttileY;
		Look = false;
	}
}
