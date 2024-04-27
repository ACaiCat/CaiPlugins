using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace HousingPlugin;

public class House
{
	public Rectangle HouseArea { get; set; }

	public string Author { get; set; }

	public List<string> Owners { get; set; }

	public string Name { get; set; }

	public bool Locked { get; set; }

	public List<string> Users { get; set; }

	public House(Rectangle housearea, string author, List<string> owners, string name, bool locked, List<string> users)
	{
		HouseArea = housearea;
		Author = author;
		Owners = owners;
		Name = name;
		Locked = locked;
		Users = users;
	}
}
