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

		public List<Action> Actions = new List<Action>();
	}

	static public void RenderActions(List<Minion> minions)
	{
        string timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd hh:mm:ss");
        if (Directory.Exists("NewActions"))
            Directory.Delete("NewActions", true);
        Directory.CreateDirectory("NewActions");
        using (var writer = new StreamWriter("NewActions/set"))
        {
            writer.WriteLine("mse_version: 2.0.1");
            writer.WriteLine("game: ritualhex");
            writer.WriteLine("stylesheet: action");
            writer.WriteLine("styling:");
	        writer.WriteLine("\tritualhex-action:");
		    writer.WriteLine("\t\tmse_version: 2.0.1");
		    foreach (var minion in minions)
		    {
			    foreach (var action in minion.Actions)
			    {
                    writer.WriteLine("card:");
                    writer.WriteLine("\thas_styling: false");
	                writer.WriteLine("\tnotes: ");
	                writer.WriteLine("\ttime_created: " + timestamp);
	                writer.WriteLine("\ttime_modified: " + timestamp);
	                writer.WriteLine("\textra_data:");
	                writer.WriteLine("\t\tritualhex-action:");
	                writer.WriteLine("\t\t\tmse_version: 2.0.1");
	                writer.WriteLine("\t\t\ttarget: \"" + action.Target + "\"");
	                writer.WriteLine("\t\t\trequires: \"" + action.Requires + "\"");
	                writer.WriteLine("\tname: \"" + action.Name + "\"");
	                writer.WriteLine("\timage: local_image_file(\"" + minion.Image + "\")");
	                writer.WriteLine("\trule_text: \"" + action.Type + "\"");
	                writer.WriteLine("\tback_text: \"" + action.Text + "\"");
	                writer.WriteLine("\tfighter: \"" + minion.Name + "\"");
                    writer.WriteLine("\taction_points: \"" + action.AP + "\"");
                    File.Copy(Path.Combine("Minions", minion.Image), Path.Combine("NewActions", minion.Image), true);
    		    }
		    }
            writer.WriteLine("version_control:");
	        writer.WriteLine("\ttype: none");
            writer.WriteLine("apprentice_code: ");
        }
	}

    static public string StrengthText(Minion minion)
    {
        return "<sym>S</sym>(" + minion.Strength + ")";
    }

    static public string MagicText(Minion minion)
    {
        return "<sym>M</sym>(" + minion.Magic + ")";
    }

    static public string AttackText(string attribute, string damage, bool resistance = true)
    {
        return "<b>Check:</b> [" + attribute + @"] vs. [<sym>P</sym>]\nDeal " + damage + (resistance ? " - [target's <sym>D</sym>]" : "") + " <sym>H</sym> to the target.";
    }

    static public string MoveText(int additionalReflex)
    {
        return @"<i>While performing this action: add " + additionalReflex + @" to your  <sym>P</sym>. If you receive a wound or one of your body parts is held, cancel this action.\n</i><b>Check: </b>None\nMove to an adjacent, unblocked room.";
    }

    static public string FlyText(int additionalReflex)
    {
        return @"<i>Can only be resolved if you have no wounds on your wings.</i>\n" + MoveText(additionalReflex);
    }

    static public string WalkText(int additionalReflex)
    {
        return @"<i>Add 1 to the  <sym>T</sym> cost for each wound on your legs.</i>\n" + MoveText(additionalReflex);
    }

	[STAThread]
	static public void Main(string[] args)
	{
        Minion imp = new Minion
        {
            Name = "Imp",
            Image = "image7",
            FirstEntry = 1,
            LastEntry = 4,
        };
        imp.Actions.Add(new Action
        {
			Name = "Claw Attack",
			Type = "Attack action",
			Target = "Body part",
			Requires = "Claw",
			AP = "3",
			Text = AttackText(StrengthText(imp), "[2]"),
        });
        imp.Actions.Add(new Action
        {
            Name = "Fire Breath",
            Type = "Magic action",
            Target = "Body part",
            Requires = "None",
            AP = "5",
            Text = AttackText(MagicText(imp), "1", false),
        });
        imp.Actions.Add(new Action
        {
            Name = "Fly",
            Type = "Action",
            Target = "None",
            Requires = "Two wings",
            AP = "3",
            Text = FlyText(3),
        });
        imp.Actions.Add(new Action
        {
            Name = "Walk",
            Type = "Action",
            Target = "None",
            Requires = "Legs",
            AP = "6*",
            Text = WalkText(2),
        });
        List<Minion> minions = new List<Minion>();
		minions.Add(imp);
		RenderActions(minions);
		Zip();
	}
}

