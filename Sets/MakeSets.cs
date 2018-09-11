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

    public class Attribute
    {
        public string Letter { get; set; }
        public override string ToString() { return "<sym>" + Letter + "</sym>"; }
        public override int GetHashCode() { return Letter.GetHashCode(); }
        public override bool Equals(object obj) 
        {
            Attribute rhs = obj as Attribute;
            if (rhs == null)
                return false;
            return Letter.Equals(rhs.Letter);
        }
    }
    static public Attribute Strength = new Attribute { Letter = "S" };
    static public Attribute Reflexes = new Attribute { Letter = "P" };
    static public Attribute Magic = new Attribute { Letter = "M" };
    static public Attribute Perception = new Attribute { Letter = "O" };
    static public Attribute Wound = new Attribute { Letter = "H" };
    static public Attribute Defense = new Attribute { Letter = "D" };
    static public Attribute Time = new Attribute { Letter = "T" };

    static public string Rand(object s)
    {
        return "[" + s + "]";
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
        public Dictionary<Attribute, int> Scores = new Dictionary<Attribute, int>();
		public int Level;
		public int FirstEntry;
		public int LastEntry;

		public List<Action> Actions = new List<Action>();

        public string this[Attribute a]
        {
            get
            {
                int score = 0;
                if (Scores.ContainsKey(a))
                    score = Scores[a];
                return a + "(" + score + ")";
            }
        }
    }

	static public void RenderActions(List<Minion> minions)
	{
        string timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd hh:mm:ss");
        if (Directory.Exists("Actions"))
            Directory.Delete("Actions", true);
        Directory.CreateDirectory("Actions");
        using (var writer = new StreamWriter("Actions/set"))
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
                    File.Copy(Path.Combine("Minions", minion.Image), Path.Combine("Actions", minion.Image), true);
    		    }
		    }
            writer.WriteLine("version_control:");
	        writer.WriteLine("\ttype: none");
            writer.WriteLine("apprentice_code: ");
        }
	}

    static public string CheckText(object level, object difficulty)
    {
        return "<b>Check:</b> " + Rand(level) + @" vs. " + difficulty;
    }

    static public string CheckText()
    {
        return "<b>Check:</b> None";
    }

    static public string AttackText(object attribute, object difficulty, object damage, bool resistance = true)
    {
        return CheckText(attribute, difficulty) + @"\n" +
            "Deal " + damage + (resistance ? " - " + Rand("target's " + Defense) : "") + " " + Wound + " to the target.";
    }

    static public string HealText(object attribute, object difficulty, object damage)
    {
        return CheckText(attribute, difficulty) + @"\n" +
            "Remove up to " + damage + " " + Wound + " from the target. They can be removed from multiple body parts.";
    }

    static public string GoreText(object attribute, object difficulty, object damage)
    {
        return AttackText(attribute, difficulty, damage) + @"\n" +
            "Cancel any action the target may have in progress.";
    }

    static public string SlapText(object attribute, object difficulty, object damage, object action, object timeBonus)
    {
        return AttackText(attribute, difficulty, damage) + @"\n" +
            "You may follow this action with '" + action + ", reducing the " + Time + " cost by " + timeBonus + ".";
    }

    static public string BiteText(object attribute, object difficulty, object damage, object vulnerability)
    {
        return AttackText(attribute, difficulty, damage) + @"\n" +
            "You may hold the target body part with your head. You may release the hold at any time. Your " + Defense + " and " + Reflexes + " are decreased by " + vulnerability + " while the fighter being held resolves an action.";
    }

    static public string GnawText(object damage)
    {
        return "<i>This action can't be interrupted.</i>" + @"\n" +
            CheckText() + @"\n" +
            "Deal " + damage + " - " + Rand("target's " + Defense) + " " + Wound + " to the target.";
    }

    static public string GrabText(object attribute, object difficulty, object vulnerability)
    {
        return CheckText(attribute, difficulty) + @"\n" +
            "Hold the target body part with the used member. You may release the hold at any time. Your " + Defense + " and " + Reflexes + " are decreased by " + vulnerability + " while the fighter being held resolves an action.";
    }

    static public string PounceText(object attribute, object difficulty, object delay, object action, object timeBonus)
    {
        return CheckText(attribute, difficulty) + @"\n" +
            "Increase the target's " + Time + " by " + delay + ". You may follow this action with " + action + ", reducing the " + Time + " cost by " + timeBonus + ".";
    }

    static public string MoveText(object additionalReflexes)
    {
        return "<i>While performing this action: add " + additionalReflexes + " to your  " + Reflexes + ". If you receive a  " + Wound + " or one of your body parts is held, cancel this action.</i>" + @"\n" +
            CheckText() + @"\n" +
            "Move to an adjacent, unblocked room.";
    }

    static public string FlyText(object additionalReflexes)
    {
        return "<i>Can only be resolved if you have no  " + Wound + " on your wings.</i>" + @"\n" +
            MoveText(additionalReflexes);
    }

    static public string WalkText(object additionalReflexes, string affectedPart = "legs")
    {
        return "<i>Add 1 to the " + Time + " cost for each  " + Wound + " on your " + affectedPart + ".</i>" + @"\n" +
            MoveText(additionalReflexes);
    }

    static public string DefendOtherText(object level, object difficulty)
    {
        return "<i>Interrupt an attack action targeting a fighter (or body part) other than you (or yours).</i>" + @"\n" +
            CheckText(level, difficulty) + @"\n" +
            "Change the target of the interrupted action to be you (or one of your body parts, if the target was a body part). If the action has more than one target, you can change any number of targets directed to the same fighter.";
    }

    static public string CounterspellText(object level, object difficulty)
    {
        return "<i>Interrupt a magic action.</i>" + @"\n" +
            CheckText(level, difficulty) + @"\n" +
            "Increase the difficulty of the interrupted action's check by 1. If the action has no check, it now has a check of " + Rand(Magic) + " vs. 1.";
    }

    static public string MindControlText(object level, object difficulty, object turns)
    {
        return CheckText(level, difficulty) + @"\n" +
            "Control the target during " + turns + " " + Time + ". Place your Head card as a status marker on the fighter to mark this status. You may cancel this status at any time.";
    }

    static public Minion Imp()
    {
        Minion imp = new Minion
        {
            Name = "Imp",
            Image = "image7",
            Level = 1,
            FirstEntry = 1,
            LastEntry = 4,
        };
        imp.Scores[Strength] = 0;
        imp.Scores[Reflexes] = 2;
        imp.Scores[Magic] = 2;
        imp.Scores[Perception] = 1;
        imp.Scores[Defense] = 0;
        imp.Scores[Wound] = 4;
        imp.Actions.Add(new Action
        {
            Name = "Claw Attack",
            Type = "Attack action",
            Target = "Body part",
            Requires = "Claw",
            AP = "3",
            Text = AttackText(imp[Strength], Rand(Reflexes), Rand(2)),
        });
        imp.Actions.Add(new Action
        {
            Name = "Fire Breath",
            Type = "Magic action",
            Target = "Body part",
            Requires = "None",
            AP = "5",
            Text = AttackText(imp[Magic], Rand(Reflexes), 1, false),
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
        return imp;
    }

    static public Minion Thrall()
    {
        Minion thrall = new Minion
        {
            Name = "Thrall",
            Image = "image10",
            Level = 1,
            FirstEntry = 5,
            LastEntry = 8,
        };
        thrall.Scores[Strength] = 1;
        thrall.Scores[Reflexes] = 2;
        thrall.Scores[Magic] = 0;
        thrall.Scores[Perception] = 1;
        thrall.Scores[Defense] = 1;
        thrall.Scores[Wound] = 6;
        thrall.Actions.Add(new Action
        {
            Name = "Defend Other",
            Type = "Block action",
            Target = "Fighter - Interrupt",
            Requires = "Feet",
            AP = "3",
            Text = DefendOtherText(thrall[Reflexes], Rand(Perception)),
        });
        thrall.Actions.Add(new Action
        {
            Name = "Punch",
            Type = "Attack action",
            Target = "Body part",
            Requires = "Hand",
            AP = "4",
            Text = AttackText(thrall[Strength], Rand(Reflexes), Rand(1)),
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
        return thrall;
    }

    static public Minion Acolyte()
    {
        Minion acolyte = new Minion
        {
            Name = "Acolyte",
            Image = "image4",
            Level = 2,
            FirstEntry = 9,
            LastEntry = 11,
        };
        acolyte.Scores[Strength] = 2;
        acolyte.Scores[Reflexes] = 1;
        acolyte.Scores[Magic] = 3;
        acolyte.Scores[Perception] = 1;
        acolyte.Scores[Defense] = 1;
        acolyte.Scores[Wound] = 8;
        acolyte.Actions.Add(new Action
        {
            Name = "Heal",
            Type = "Magic action",
            Target = "Fighter",
            Requires = "Hand",
            AP = "5",
            Text = HealText(acolyte[Magic], 1, Rand(2)),
        });
        acolyte.Actions.Add(new Action
        {
            Name = "Counterspell",
            Type = "Magic action",
            Target = "Fighter - Interrupt",
            Requires = "Hand",
            AP = "3",
            Text = CounterspellText(acolyte[Magic], Rand(Magic)),
        });
        acolyte.Actions.Add(new Action
        {
            Name = "Punch",
            Type = "Attack action",
            Target = "Body part",
            Requires = "Hand",
            AP = "4",
            Text = AttackText(acolyte[Strength], Rand(Reflexes), Rand(1)),
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
        return acolyte;
    }

    static public Minion HellHound()
    {
        Minion hellHound = new Minion
        {
            Name = "Hell Hound",
            Image = "image13",
            Level = 2,
            FirstEntry = 12,
            LastEntry = 14,
        };
        hellHound.Scores[Strength] = 2;
        hellHound.Scores[Reflexes] = 2;
        hellHound.Scores[Magic] = 1;
        hellHound.Scores[Perception] = 4;
        hellHound.Scores[Defense] = 1;
        hellHound.Scores[Wound] = 7;
        hellHound.Actions.Add(new Action
        {
            Name = "Bite",
            Type = "Attack action",
            Target = "Body part",
            Requires = "Head",
            AP = "5",
            Text = BiteText(hellHound[Strength], Rand(Reflexes), Rand(2), 2),
        });
        hellHound.Actions.Add(new Action
        {
            Name = "Gnaw",
            Type = "Attack action",
            Target = "Body part being held by your head",
            Requires = "None",
            AP = "3",
            Text = GnawText(Rand(1)),
        });
        hellHound.Actions.Add(new Action
        {
            Name = "Pounce",
            Type = "Attack action",
            Target = "Fighter",
            Requires = "All paws",
            AP = "4",
            Text = PounceText(hellHound[Strength], Rand(Strength), 2, "'Bite'", 2),
        });
        hellHound.Actions.Add(new Action
        {
            Name = "Prowl",
            Type = "Action",
            Target = "None",
            Requires = "All paws",
            AP = "4*",
            Text = WalkText(2, "paws"),
        });
        return hellHound;
    }

    static public Minion AxeFiend()
    {
        Minion axeFiend = new Minion
        {
            Name = "Axe Fiend",
            Image = "image3",
            Level = 3,
            FirstEntry = 15,
            LastEntry = 17,
        };
        axeFiend.Scores[Strength] = 4;
        axeFiend.Scores[Reflexes] = 1;
        axeFiend.Scores[Magic] = 0;
        axeFiend.Scores[Perception] = 1;
        axeFiend.Scores[Defense] = 2;
        axeFiend.Scores[Wound] = 10;
        axeFiend.Actions.Add(new Action
        {
            Name = "Gore",
            Type = "Attack action",
            Target = "Body part",
            Requires = "Head",
            AP = "6",
            Text = GoreText(axeFiend[Strength], Rand(Reflexes), Rand(2)),
        });
        axeFiend.Actions.Add(new Action
        {
            Name = "Walk",
            Type = "Action",
            Target = "None",
            Requires = "Legs",
            AP = "6*",
            Text = WalkText(1),
        });
        return axeFiend;
    }

    static public Minion Succubus()
    {
        Minion succubus = new Minion
        {
            Name = "Succubus",
            Image = "image5",
            Level = 3,
            FirstEntry = 18,
            LastEntry = 19,
        };
        succubus.Scores[Strength] = 2;
        succubus.Scores[Reflexes] = 1;
        succubus.Scores[Magic] = 4;
        succubus.Scores[Perception] = 2;
        succubus.Scores[Defense] = 1;
        succubus.Scores[Wound] = 8;
        succubus.Actions.Add(new Action
        {
            Name = "Claw Attack",
            Type = "Attack action",
            Target = "Body part",
            Requires = "Claw",
            AP = "3",
            Text = AttackText(succubus[Strength], Rand(Reflexes), Rand(2)),
        });
        succubus.Actions.Add(new Action
        {
            Name = "Fly",
            Type = "Action",
            Target = "None",
            Requires = "Two wings",
            AP = "2",
            Text = FlyText(3),
        });
        succubus.Actions.Add(new Action
        {
            Name = "Mind Control",
            Type = "Magic action",
            Target = "Fighter",
            Requires = "Head",
            AP = "6",
            Text = MindControlText(succubus[Magic], Rand(Magic), Rand(8)),
        });
        succubus.Actions.Add(new Action
        {
            Name = "Walk",
            Type = "Action",
            Target = "None",
            Requires = "Legs",
            AP = "4*",
            Text = WalkText(2),
        });
        return succubus;
    }

    static public Minion AbyssHorror()
    {
        Minion abyssHorror = new Minion
        {
            Name = "Abyss Horror",
            Image = "image1",
            Level = 4,
            FirstEntry = 22,
            LastEntry = 22,
        };
        abyssHorror.Scores[Strength] = 3;
        abyssHorror.Scores[Reflexes] = 3;
        abyssHorror.Scores[Magic] = 6;
        abyssHorror.Scores[Perception] = 3;
        abyssHorror.Scores[Defense] = 3;
        abyssHorror.Scores[Wound] = 15;
        abyssHorror.Actions.Add(new Action
        {
            Name = "Slap",
            Type = "Attack action",
            Target = "Body part",
            Requires = "Tentacle",
            AP = "5",
            Text = SlapText(abyssHorror[Strength], Rand(Reflexes), Rand(2), "'Grab', with the same target and required part", 2),
        });
        abyssHorror.Actions.Add(new Action
        {
            Name = "Grab",
            Type = "Attack action",
            Target = "Body part",
            Requires = "Tentacle",
            AP = "6",
            Text = GrabText(abyssHorror[Strength], Rand(Strength), 1),
        });
        abyssHorror.Actions.Add(new Action
        {
            Name = "Shuffle",
            Type = "Action",
            Target = "None",
            Requires = "All tentacles",
            AP = "6*",
            Text = WalkText(1, "tentacles"),
        });
        return abyssHorror;
    }

	[STAThread]
	static public void Main(string[] args)
	{
        List<Minion> minions = new List<Minion>();
        minions.Add(Imp());
        minions.Add(Thrall());
        minions.Add(Acolyte());
        minions.Add(HellHound());
        minions.Add(AxeFiend());
        minions.Add(Succubus());
        minions.Add(AbyssHorror());
        RenderActions(minions);
        Zip();
    }
}

