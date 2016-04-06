#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Server.Gumps;
using Server.Network;

#endregion

namespace Server
{
    public class TomeOfCharacterDevelopment : Item
    {
        public const int NumberOfChoices = 7;
        public const double AmountToRaiseTo = 100.0;

        [Constructable]
        public TomeOfCharacterDevelopment() : base(0x0E3B)
        {
            Name = "Tome of Character Development";
            Weight = 1.0;
            Hue = 1462;
            LootType = LootType.Blessed;
        }

        public TomeOfCharacterDevelopment( Serial serial ) : base( serial ) 
		{ 
		}

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1060640);
                return;
            }

            if(from.HasGump(typeof(TomeOfCharacterDevelopmentGump)))
            {
                from.CloseGump(typeof (TomeOfCharacterDevelopmentGump));
            }

            Movable = false;
            from.SendGump(new TomeOfCharacterDevelopmentGump(this));   
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            list.Add("Sets All Skills to 0, Then Lets You Choose {0} Skills To Boost To {1}", NumberOfChoices, AmountToRaiseTo);
        }

        public override void Serialize( GenericWriter writer ) 
		{ 
			base.Serialize( writer ); 
			writer.Write( (int) 0 ); // version 
		}

		public override void Deserialize( GenericReader reader ) 
		{ 
			base.Deserialize( reader ); 
			int version = reader.ReadInt(); 
		} 
    }

    public class TomeOfCharacterDevelopmentGump : Gump
    {
        private readonly TomeOfCharacterDevelopment m_Tome;

        private static readonly List<SkillEntry> Skills = new List<SkillEntry>
        {
            new SkillEntry(0, SkillName.Alchemy, "Alchemy"),
            new SkillEntry(1, SkillName.Anatomy, "Anatomy"),
            new SkillEntry(2, SkillName.AnimalLore, "Animal Lore"),
            new SkillEntry(3, SkillName.ItemID, "Item Identification"),
            new SkillEntry(4, SkillName.ArmsLore, "Arms Lore"),
            new SkillEntry(5, SkillName.Parry, "Parrying"),
            new SkillEntry(6, SkillName.Begging, "Begging"),
            new SkillEntry(7, SkillName.Blacksmith, "Blacksmithy"),
            new SkillEntry(8, SkillName.Fletching, "Fletching"),
            new SkillEntry(9, SkillName.Peacemaking, "Peacemaking"),
            new SkillEntry(10, SkillName.Camping, "Camping"),
            new SkillEntry(11, SkillName.Carpentry, "Carpentry"),
            new SkillEntry(12, SkillName.Cartography, "Cartography"),
            new SkillEntry(13, SkillName.Cooking, "Cooking"),
            new SkillEntry(14, SkillName.DetectHidden, "Detect Hidden"),
            new SkillEntry(15, SkillName.Discordance, "Discordance"),
            new SkillEntry(16, SkillName.EvalInt, "Evaluating Intelligence"),
            new SkillEntry(17, SkillName.Healing, "Healing"),
            new SkillEntry(18, SkillName.Fishing, "Fishing"),
            new SkillEntry(19, SkillName.Forensics, "Forensic Evaluation"),
            new SkillEntry(20, SkillName.Herding, "Herding"),
            new SkillEntry(21, SkillName.Hiding, "Hiding"),
            new SkillEntry(22, SkillName.Provocation, "Provocation"),
            new SkillEntry(23, SkillName.Inscribe, "Inscription"),
            new SkillEntry(24, SkillName.Lockpicking, "Lock Picking"),
            new SkillEntry(25, SkillName.Magery, "Magery"),
            new SkillEntry(26, SkillName.MagicResist, "Resisting Magic"),
            new SkillEntry(27, SkillName.Tactics, "Tactics"),
            new SkillEntry(28, SkillName.Snooping, "Snooping"),
            new SkillEntry(29, SkillName.Musicianship, "Musicianship"),
            new SkillEntry(30, SkillName.Poisoning, "Poisoning"),
            new SkillEntry(31, SkillName.Archery, "Archery"),
            new SkillEntry(32, SkillName.SpiritSpeak, "Spirit Speak"),
            new SkillEntry(33, SkillName.Stealing, "Stealing"),
            new SkillEntry(34, SkillName.Tailoring, "Tailoring"),
            new SkillEntry(35, SkillName.AnimalTaming, "Animal Taming"),
            new SkillEntry(36, SkillName.TasteID, "Taste Identification"),
            new SkillEntry(37, SkillName.Tinkering, "Tinkering"),
            new SkillEntry(38, SkillName.Tracking, "Tracking"),
            new SkillEntry(39, SkillName.Veterinary, "Veterinary"),
            new SkillEntry(40, SkillName.Swords, "Swordsmanship"),
            new SkillEntry(41, SkillName.Macing, "Mace Fighting"),
            new SkillEntry(42, SkillName.Fencing, "Fencing"),
            new SkillEntry(43, SkillName.Wrestling, "Wrestling"),
            new SkillEntry(44, SkillName.Lumberjacking, "Lumberjacking"),
            new SkillEntry(45, SkillName.Mining, "Mining"),
            new SkillEntry(46, SkillName.Meditation, "Meditation"),
            new SkillEntry(47, SkillName.Stealth, "Stealth"),
            new SkillEntry(48, SkillName.RemoveTrap, "Removing Traps")
        };

        public TomeOfCharacterDevelopmentGump(TomeOfCharacterDevelopment tome) : base(0, 0)
        {
            m_Tome = tome;

            if (Core.AOS)
            {
                Skills.Add(new SkillEntry(49, SkillName.Necromancy, "Necromancy"));
                Skills.Add(new SkillEntry(50, SkillName.Focus, "Focus"));
                Skills.Add(new SkillEntry(51, SkillName.Chivalry, "Chivalry"));
            }

            if (Core.SE)
            {
                Skills.Add(new SkillEntry(52, SkillName.Bushido, "Bushido"));
                Skills.Add(new SkillEntry(53, SkillName.Ninjitsu, "Ninjitsu"));
            }

            if (Core.ML)
            {
                Skills.Add(new SkillEntry(54, SkillName.Spellweaving, "Spellweaving"));
            }

            if (Core.SA)
            {
                Skills.Add(new SkillEntry(55, SkillName.Mysticism, "Mysticism"));
                Skills.Add(new SkillEntry(56, SkillName.Imbuing, "Imbuing"));
                Skills.Add(new SkillEntry(57, SkillName.Throwing, "Throwing"));
            }

            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;

            AddPage(0);
            AddBackground(5, 4, 718, 573, 9380);
            AddImage(679, 108, 10441);
            AddLabel(43, 7, 167,
                String.Format("Select {0} skills to raise to {1}.", TomeOfCharacterDevelopment.NumberOfChoices, (int) TomeOfCharacterDevelopment.AmountToRaiseTo));
            AddButton(344, 553, 247, 248, 1, GumpButtonType.Reply, 0);

            int checkx = 43;
            int textx = 73;
            int y = 48;     

            bool firstcolumnpassed = false;
            bool secondcolumnpassed = false;

            foreach (SkillEntry skill in Skills)
            {
                AddCheck(checkx, y, 210, 211, false, skill.SkillID);
                AddLabel(textx, y, 887, skill.SkillString);        
          
                if (y > 491 && !firstcolumnpassed)
                {
                    y = 48;
                    checkx = 220;
                    textx = 250;
                    firstcolumnpassed = true;
                }
                else if (y > 491 && firstcolumnpassed && !secondcolumnpassed)
                {
                    y = 48;
                    checkx = 393;
                    textx = 423;
                    secondcolumnpassed = true;
                }
                else if (y > 491 && secondcolumnpassed)
                {
                    y = 48;
                    checkx = 546;
                    textx = 576;
                }        
                else
                {
                    y += 30;
                }
            }  
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile m = sender.Mobile;

            if (m == null || m.Deleted)
                return;

            switch (info.ButtonID)
            {
                case 0:
                {
                    m.SendMessage("You decide not to use the Tome of Character Development just yet.");
                    m_Tome.Movable = true;
                    break;
                }
                case 1:
                {
                    if (info.Switches.Length > TomeOfCharacterDevelopment.NumberOfChoices || info.Switches.Length < TomeOfCharacterDevelopment.NumberOfChoices)
                    {
                        m.SendMessage("You must choose {0} skills, but you chose {1}. Please try again.", TomeOfCharacterDevelopment.NumberOfChoices, info.Switches.Length);
                        return;
                    }
        
                    Skills skills = m.Skills;
                    for (int i = 0; i < skills.Length; ++i)
                        skills[i].Base = 0;              

                    foreach (SkillEntry entry in Skills.Where(entry => info.IsSwitched(entry.SkillID)))
                    {
                        m.Skills[entry.SkillName].Base = TomeOfCharacterDevelopment.AmountToRaiseTo;
                    }

                    m_Tome.Delete();

                    break;
                }
            }
        }
    }

    public class SkillEntry
    {
        private readonly SkillName m_SkillName;
        private readonly int m_SkillID;
        private readonly string m_SkillString;

        public SkillName SkillName
        {
            get { return m_SkillName; }
        }

        public int SkillID
        {
            get { return m_SkillID; }
        }

        public string SkillString
        {
            get { return m_SkillString; }
        }

        public SkillEntry(int id, SkillName skillname, string skillstring)
        {
            m_SkillID = id;
            m_SkillName = skillname;
            m_SkillString = skillstring;
        }
    }
}