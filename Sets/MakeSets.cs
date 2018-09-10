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

    public class Symbol
    {
        public string Letter { get; set; }
        public override string ToString() { return "<sym>" + Letter + "</sym>"; }
    }
    static public Symbol Str = new Symbol { Letter = "S" };
    static public Symbol Mag = new Symbol { Letter = "M" };
    static public Symbol Per = new Symbol { Letter = "O" };
    static public Symbol Hlt = new Symbol { Letter = "H" };
    static public Symbol Ddg = new Symbol { Letter = "P" };
    static public Symbol Def = new Symbol { Letter = "D" };
    static public Symbol Tim = new Symbol { Letter = "T" };

    static public string Rnd(string v)
    {
        return "[" + v + "]";
    }

    static public string Rnd(Symbol s)
    {
        return Rnd(s.ToString());
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

        public string StrengthText
        {
            get { return Str + "(" + Strength + ")"; }
        }

        public string MagicText
        {
            get { return Mag + "(" + Magic + ")"; }
        }
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

    static public string CheckText(string level, string difficulty)
    {
        return "<b>Check:</b> " + Rnd(level) + @" vs. " + difficulty;
    }

    static public string CheckText()
    {
        return "<b>Check:</b> None";
    }

    static public string AttackText(string attribute, string difficulty, string damage, bool resistance = true)
    {
        return CheckText(attribute, difficulty) + @"\n" +
            "Deal " + damage + (resistance ? " - " + Rnd("target's " + Def) : "") + " " + Hlt + " to the target.";
    }

    static public string HealText(string attribute, string difficulty, string damage)
    {
        return CheckText(attribute, difficulty) + @"\n" +
            "Remove up to " + damage + " " + Hlt + " from the target. They can be removed from multiple body parts.";
    }

    static public string MoveText(int additionalReflex)
    {
        return "<i>While performing this action: add " + additionalReflex + " to your  " + Ddg + ". If you receive a  " + Hlt + " or one of your body parts is held, cancel this action.</i>" + @"\n" +
            CheckText() + @"\n" +
            "Move to an adjacent, unblocked room.";
    }

    static public string FlyText(int additionalReflex)
    {
        return "<i>Can only be resolved if you have no  " + Hlt + " on your wings.</i>" + @"\n" +
            MoveText(additionalReflex);
    }

    static public string WalkText(int additionalReflex)
    {
        return "<i>Add 1 to the " + Tim + " cost for each  " + Hlt + " on your legs.</i>" + @"\n" +
            MoveText(additionalReflex);
    }

    static public string DefendOtherText(string level, string difficulty)
    {
        return "<i>Interrupt an attack action targeting a fighter (or body part) other than you (or yours).</i>" + @"\n" +
            CheckText(level, difficulty) + @"\n" +
            "Change the target of the interrupted action to be you (or one of your body parts). If the action has more than one target, choose only one of those targets to change.";
    }

    static public string CounterspellText(string level, string difficulty)
    {
        return "<i>Interrupt a magic action.</i>" + @"\n" +
            CheckText(level, difficulty) + @"\n" +
            "Increase the difficulty of the interrupted action's check by 1. If the action has no check, it now has a check of " + Rnd(Mag) + " vs. 1.";
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
			Text = AttackText(imp.StrengthText, Rnd(Ddg), "[2]"),
        });
        imp.Actions.Add(new Action
        {
            Name = "Fire Breath",
            Type = "Magic action",
            Target = "Body part",
            Requires = "None",
            AP = "5",
            Text = AttackText(imp.MagicText, Rnd(Ddg), "1", false),
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

        Minion thrall = new Minion
        {
            Name = "Thrall",
            Image = "image10",
            FirstEntry = 5,
            LastEntry = 8,
        };
        thrall.Actions.Add(new Action
        {
            Name = "Defend Other",
            Type = "Block action",
            Target = "Fighter - Interrupt",
            Requires = "Feet",
            AP = "3",
            Text = DefendOtherText("2", Rnd(Per)),
        });
        thrall.Actions.Add(new Action
        {
            Name = "Punch",
            Type = "Attack action",
            Target = "Body part",
            Requires = "Hand",
            AP = "4",
            Text = AttackText(thrall.StrengthText, Rnd(Ddg), "[1]"),
        });
        thrall.Actions.Add(new Action
        {
            Name = "Walk",
            Type = "Action",
            Target = "None",
            Requires = "Legs",
            AP = "4*",
            Text = WalkText(2),
        });

        Minion acolyte = new Minion
        {
            Name = "Acolyte",
            Image = "image4",
            FirstEntry = 9,
            LastEntry = 11,
        };
        acolyte.Actions.Add(new Action
        {
            Name = "Heal",
            Type = "Magic action",
            Target = "Fighter",
            Requires = "Hand",
            AP = "5",
            Text = HealText(acolyte.MagicText, "1", "[2]"),
        });
        acolyte.Actions.Add(new Action
        {
            Name = "Counterspell",
            Type = "Magic action",
            Target = "Fighter - Interrupt",
            Requires = "Hand",
            AP = "3",
            Text = CounterspellText(acolyte.MagicText, Rnd(Mag)),
        });
        acolyte.Actions.Add(new Action
        {
            Name = "Punch",
            Type = "Attack action",
            Target = "Body part",
            Requires = "Hand",
            AP = "4",
            Text = AttackText(acolyte.StrengthText, Rnd(Ddg), "[1]"),
        });
        acolyte.Actions.Add(new Action
        {
            Name = "Walk",
            Type = "Action",
            Target = "None",
            Requires = "Legs",
            AP = "4*",
            Text = WalkText(2),
        });

        List<Minion> minions = new List<Minion>();
        minions.Add(imp);
        minions.Add(thrall);
        minions.Add(acolyte);
        RenderActions(minions);
		Zip();
	}
}

