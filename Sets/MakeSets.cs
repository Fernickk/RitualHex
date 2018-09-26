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
    static public Attribute Target = new Attribute { Letter = "G" };
    static public Attribute UsedBodyPart = new Attribute { Letter = "U" };

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
        public string Code;
        public Dictionary<Attribute, int> Scores = new Dictionary<Attribute, int>();
        public int Level;
        public string SpecialRules = "";
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
            writer.WriteLine("mse_version: 0.3.8");
            writer.WriteLine("game: ritualhex");
            writer.WriteLine("stylesheet: action");
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
                    writer.WriteLine("\t\t\ttarget: " + action.Target);
                    writer.WriteLine("\t\t\trequires: " + action.Requires);
                    writer.WriteLine("\tname: " + action.Name);
                    writer.WriteLine("\timage: " + minion.Code);
                    writer.WriteLine("\trule_text: " + action.Type);
                    writer.WriteLine("\tback_text:");
                    using (var textReader = new StringReader(action.Text))
                    {
                        while (true)
                        {
                            string line = textReader.ReadLine();
                            if (line == null)
                                break;
                            writer.WriteLine("\t\t" + line);
                        }
                    }
                    writer.WriteLine("\tfighter: " + minion.Name);
                    writer.WriteLine("\taction_points: " + action.AP);
                    File.Copy(Path.Combine(@"..\Resources\MinionImages", minion.Code + ".png"), Path.Combine("Actions", minion.Code), true);
                }
            }
            writer.WriteLine("version_control:");
            writer.WriteLine("\ttype: none");
            writer.WriteLine("apprentice_code: ");
        }
    }

    static public string CheckText(object level, object difficulty)
    {
        return "<b>Check vs. </b>" + Target + "<b>:</b> " + Rand(level) + " vs. " + difficulty;
    }

    static public string CheckFixedText(object level, object difficulty)
    {
        return "<b>Check:</b> " + Rand(level) + " vs. " + difficulty;
    }

    static public string CheckText()
    {
        return "<b>Check:</b> None";
    }

    static public string AttackText(object attribute, object difficulty, object damage)
    {
        return CheckText(attribute, difficulty) + "\n" +
            "Deal " + damage + " " + Wound + " to " + Target + ".";
    }

    static public string NegateDefAttackText(object attribute, object difficulty, object damage)
    {
        return CheckText(attribute, difficulty) + "\n" +
            "Deal " + damage + " " + Wound + " to " + Target + ", ignoring their " + Defense + ".";
    }

    static public string HealText(object attribute, object difficulty, object damage)
    {
        return CheckText(attribute, difficulty) + "\n" +
            "Remove up to " + damage + " " + Wound + " from " + Target + ". They can be removed from multiple body parts.";
    }

    static public string GoreText(object attribute, object difficulty, object damage)
    {
        return AttackText(attribute, difficulty, damage) + "\n" +
            "Cancel any action " + Target + " may have in progress.";
    }

    static public string SlapText(object attribute, object difficulty, object damage, object action)
    {
        return AttackText(attribute, difficulty, damage) + "\n" +
            "You may follow this action with " + action + ".";
    }

    static public string BiteText(object attribute, object difficulty, object damage, object vulnerability)
    {
        return AttackText(attribute, difficulty, damage) + "\n" +
            "You may pin " + Target + " with " + UsedBodyPart + ".";
    }

    static public string DrainAttackText(object attribute, object difficulty, object damage)
    {
        return AttackText(attribute, difficulty, damage) + "\n" +
            "You may remove " + Wound + " from yourself, up to the number of " + Wound + " dealt. They can be removed from multiple body parts.";
    }

    static public string GnawText(object damage)
    {
        return "<i>This action can't be interrupted.</i>\n" +
            CheckText() + "\n" +
            "Deal " + damage + " " + Wound + " to " + Target + ".";
    }

    static public string GrabText(object attribute, object difficulty, object vulnerability)
    {
        return CheckText(attribute, difficulty) + "\n" +
            "Pin " + Target + " with " + UsedBodyPart + ".";
    }

    static public string PounceText(object attribute, object difficulty, object delay, object action)
    {
        return CheckText(attribute, difficulty) + "\n" +
            "Increase " + Target + "'s " + Time + " by " + delay + ". You may follow this action with " + action + ".";
    }

    static public string TeleportText()
    {
        return CheckText() + "\n" +
            "Move to an adjacent room.";
    }

    static public string MoveText(object additionalReflexes)
    {
        return "<i>While performing this action: add " + additionalReflexes + " to your  " + Reflexes + ". If you receive a  " + Wound + " or one of your body parts is held, cancel this action.</i>\n" +
            CheckText() + "\n" +
            "Move to an adjacent, unblocked room.";
    }

    static public string FlyText(object additionalReflexes)
    {
        return "<i>Can only be resolved if you have no  " + Wound + " on your wings.</i>\n" +
            MoveText(additionalReflexes);
    }

    static public string WalkText(object additionalReflexes, string affectedPart = "legs")
    {
        return "<i>Add 1 to the " + Time + " cost for each  " + Wound + " on your " + affectedPart + ".</i>\n" +
            MoveText(additionalReflexes);
    }

    static public string DefendOtherText(object level, object difficulty)
    {
        return "<i>Interrupt an attack action targeting a fighter other than you.</i>\n" +
            CheckText(level, difficulty) + "\n" +
            "Change the target of the interrupted action to be you (or one of your body parts, if the target was a body part). If the interrupted action has more than one target, you can change any number of targets directed to the same fighter.";
    }

    static public string CounterspellText(object level, object difficulty)
    {
        return "<i>Interrupt a magic action.</i>\n" +
            CheckText(level, difficulty) + "\n" +
            "Increase the difficulty of the interrupted action's check by 1. If the interrupted action has no check, it now has a check of " + Rand(Magic) + " vs. 1.";
    }

    static public string MindControlText(object level, object difficulty, object turns)
    {
        return CheckText(level, difficulty) + "\n" +
            "Pin " + Target + " with " + UsedBodyPart + ".";
    }

    static public Minion Imp()
    {
        Minion imp = new Minion
        {
            Name = "Imp",
            Code = "imp",
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
            Text = NegateDefAttackText(imp[Magic], Rand(Reflexes), 1),
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
            Code = "thrall",
            Level = 1,
            SpecialRules = "May equip a spear or staff without level increase.",
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
            Code = "acolyte",
            Level = 2,
            SpecialRules = "May equip a dagger without level increase.",
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
            Code = "hellHound",
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
            Text = PounceText(hellHound[Strength], Rand(Strength), 2, "'Bite', reducing the " + Time + " cost by 2"),
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
            Code = "axeFiend",
            Level = 3,
            SpecialRules = "May equip a greataxe and armor without level increase.",
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
            Code = "succubus",
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
            Name = "Hold Control",
            Type = "Magic action",
            Target = "Fighter pinned with your Head",
            Requires = "None",
            AP = "-",
            Text = CheckFixedText(succubus[Magic], Rand("+1 tokens on your Head")) + "\n" +
                "Add a +1 token to your Head.\n" +
                "<b>Failure: </b> Stop pinning " + Target + " with your Head.",
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

    static public Minion ShadowDancer()
    {
        Minion shadowDancer = new Minion
        {
            Name = "Shadow Dancer",
            Code = "shadowDancer",
            Level = 4,
            SpecialRules = "May equip a sword without level increase. " + 
                "Ignore the " + Defense + " of your targets while performing actions.",
            FirstEntry = 20,
            LastEntry = 21,
        };
        shadowDancer.Scores[Strength] = 3;
        shadowDancer.Scores[Reflexes] = 4;
        shadowDancer.Scores[Magic] = 2;
        shadowDancer.Scores[Perception] = 2;
        shadowDancer.Scores[Defense] = 4;
        shadowDancer.Scores[Wound] = 12;
        shadowDancer.Actions.Add(new Action
        {
            Name = "Kick",
            Type = "Attack action",
            Target = "Body part",
            Requires = "Feet",
            AP = "4",
            Text = AttackText(shadowDancer[Strength], Rand(Reflexes), Rand(2)),
        });
        shadowDancer.Actions.Add(new Action
        {
            Name = "Teleport",
            Type = "Magic action",
            Target = "None",
            Requires = "None",
            AP = "4",
            Text = TeleportText(),
        });
        return shadowDancer;
    }

    static public Minion AbyssHorror()
    {
        Minion abyssHorror = new Minion
        {
            Name = "Abyss Horror",
            Code = "abyssHorror",
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
            Text = SlapText(abyssHorror[Strength], Rand(Reflexes), Rand(2), "'Grab', reducing the " + Time + " cost by 2, with the same " + Target + " and " + UsedBodyPart + " as this action"),
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

    static public Minion Blighter()
    {
        Minion blighter = new Minion
        {
            Name = "Blighter",
            Code = "blighter",
            Level = 5,
            SpecialRules = "Actions targeting you require an extra " + Time,
            FirstEntry = 23,
            LastEntry = 23,
        };
        blighter.Scores[Strength] = 3;
        blighter.Scores[Reflexes] = 2;
        blighter.Scores[Magic] = 7;
        blighter.Scores[Perception] = 3;
        blighter.Scores[Defense] = 3;
        blighter.Scores[Wound] = 11;
        blighter.Actions.Add(new Action
        {
            Name = "Drain Attack",
            Type = "Attack action",
            Target = "Body part",
            Requires = "Claw",
            AP = "4",
            Text = DrainAttackText(blighter[Strength], Rand(Reflexes), Rand(5)),
        });
        blighter.Actions.Add(new Action
        {
            Name = "Walk",
            Type = "Action",
            Target = "None",
            Requires = "Legs",
            AP = "5*",
            Text = WalkText(1),
        });
        return blighter;
    }

    static public Minion InfernalSpawn()
    {
        Minion infernalSpawn = new Minion
        {
            Name = "Infernal Spawn",
            Code = "infernalSpawn",
            Level = 5,
            SpecialRules = "When a fighter passes the check of an attack action targeting you, deal [1] " + Wound + " to one of the " + UsedBodyPart + " of the attack.",
            FirstEntry = 24,
            LastEntry = 24,
        };
        infernalSpawn.Scores[Strength] = 4;
        infernalSpawn.Scores[Reflexes] = 3;
        infernalSpawn.Scores[Magic] = 3;
        infernalSpawn.Scores[Perception] = 2;
        infernalSpawn.Scores[Defense] = 5;
        infernalSpawn.Scores[Wound] = 12;
        infernalSpawn.Actions.Add(new Action
        {
            Name = "Punch",
            Type = "Attack action",
            Target = "Body part",
            Requires = "Hand",
            AP = "4",
            Text = AttackText(infernalSpawn[Strength], Rand(Reflexes), Rand(4)),
        });
        infernalSpawn.Actions.Add(new Action
        {
            Name = "Fire Breath",
            Type = "Magic action",
            Target = "Body part",
            Requires = "None",
            AP = "5",
            Text = NegateDefAttackText(infernalSpawn[Magic], Rand(Reflexes), Rand(3)),
        });
        infernalSpawn.Actions.Add(new Action
        {
            Name = "Walk",
            Type = "Action",
            Target = "None",
            Requires = "Legs",
            AP = "4*",
            Text = WalkText(2),
        });
        return infernalSpawn;
    }

    static public Minion HulkingDemon()
    {
        Minion hulkingDemon = new Minion
        {
            Name = "Hulking Demon",
            Code = "hulkingDemon",
            Level = 6,
            SpecialRules = "Remove " + Rand(3) + " " + Wound + " from you on every full " + Time + "cycle.",
            FirstEntry = 25,
            LastEntry = 25,
        };
        hulkingDemon.Scores[Strength] = 7;
        hulkingDemon.Scores[Reflexes] = 2;
        hulkingDemon.Scores[Magic] = 3;
        hulkingDemon.Scores[Perception] = 3;
        hulkingDemon.Scores[Defense] = 4;
        hulkingDemon.Scores[Wound] = 15;
        hulkingDemon.Actions.Add(new Action
        {
            Name = "Gore",
            Type = "Attack action",
            Target = "Body part",
            Requires = "Head",
            AP = "6",
            Text = GoreText(hulkingDemon[Strength], Rand(Reflexes), Rand(2)),
        });
        hulkingDemon.Actions.Add(new Action
        {
            Name = "Quake",
            Type = "Attack action",
            Target = "Body part",
            Requires = "Head",
            AP = "6",
            Text = GoreText(hulkingDemon[Strength], Rand(Reflexes), Rand(2)),
        });
        hulkingDemon.Actions.Add(new Action
        {
            Name = "Walk",
            Type = "Action",
            Target = "None",
            Requires = "Legs",
            AP = "4*",
            Text = WalkText(2),
        });
        return hulkingDemon;
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
        minions.Add(ShadowDancer());
        minions.Add(AbyssHorror());
        minions.Add(Blighter());
        minions.Add(InfernalSpawn());
        minions.Add(HulkingDemon());
        RenderActions(minions);
        Zip();
    }
}

