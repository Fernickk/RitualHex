//css_ref System.IO.Compression;
//css_ref System.IO.Compression.FileSystem;
using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using System.Collections.Generic;

class Script
{
	static public void Zip()
	{
        DirectoryInfo dirInfo = new DirectoryInfo(".");
        foreach (var set in dirInfo.GetDirectories())
        {
            Console.WriteLine(set.Name);
            if (File.Exists(set.Name + ".mse-set"))
            {
                File.Delete(set.Name + ".mse-set");
            }
            ZipFile.CreateFromDirectory(set.Name, set.Name + ".mse-set");
        }
	}

	public class Action
	{
		public string Name;
		public string Type;
		public string Target;
		public string Requires;
		public string AP;
		public string Text;
	}

	public class Minion
	{
		public string Name;
		public string Image;
		public int Level;
		public int Health;
		public int Strength;
		public int Reflexes;
		public int Perception;
		public int Magic;
		public int FirstEntry;
		public int LastEntry;

		public Action[] Actions;
	}

	static public void RenderMinions(List<Minion> minions)
	{
		foreach (var minion in minions)
		{
			Console.WriteLine(minion.Name);
			foreach (var action in minion.Actions)
			{
				Console.WriteLine(" " + action.Name);
    		}
		}
	}

	[STAThread]
	static public void Main(string[] args)
	{
		List<Minion> minions = new List<Minion>();
		minions.Add(new Minion
		{
			Name = "Imp",
			Image = "image7",
			FirstEntry = 1,
			LastEntry = 4,
			Actions = new Action[] 
			{
				new Action
				{
					Name = "Claw Attack",
					Type = "Attack action",
					Target = "Body part",
					Requires = "Claw",
					AP = "3",
					Text = "<b>Check:</b> [<sym>S</sym>(1)] vs. [<sym>D</sym>]\nDeal [2] - [target's <sym>R</sym>] wounds to the target.",
				}
			}
		});
		RenderMinions(minions);
		//Zip();
	}
}

