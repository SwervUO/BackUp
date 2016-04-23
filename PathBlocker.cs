#region Header
/*
 * JustUO.com / PlayUO.org
 * PathBlocker.cs
 * Tile that will block the path of Mobiles by the set properties provided
 * 
 * To Add:
 * LOS blocker, 
 * Specific skills - stats?
 * 
*/
#endregion

#region References
using System;
using System.Collections.Generic;
using Server.Items;
using Server.Gumps;
using Server.Mobiles;
using Server.Spells;
#endregion

namespace Server.Items
{
    public class PathBlocker : Item
    {
        private DateTime m_NextMessage;

        private bool m_Active;

        private bool m_RestrictPlayers;
        private bool m_RestrictCreatures;
        private bool m_RestrictTamedCreatures;

        private bool m_RestrictCombat;
        private bool m_RestrictCriminal;
        private bool m_RestrictMurderer;

        private bool m_RestrictHiddenPlayers;
        private bool m_RestrictMountedPlayers;
        private bool m_RestrictDeadPlayers;
        private bool m_DeadPlayersOnly;

        private bool m_RestrictMale;
        private bool m_RestrictFemale;
        private bool m_RestrictHuman;
        private bool m_RestrictElf;
        private bool m_RestrictGargoyle;

        //private bool m_SourceEffect;
        //private bool m_DestEffect;
        //private int m_SoundID;

        #region Command Properties
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Active
        {
            get { return m_Active; }
            set
            {
                m_Active = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RestrictPlayers
        {
            get { return m_RestrictPlayers; }
            set
            {
                m_RestrictPlayers = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RestrictCreatures
        {
            get { return m_RestrictCreatures; }
            set
            {
                m_RestrictCreatures = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RestrictTamedCreatures
        {
            get { return m_RestrictTamedCreatures; }
            set
            {
                m_RestrictTamedCreatures = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RestrictCombat
        {
            get { return m_RestrictCombat; }
            set
            {
                m_RestrictCombat = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RestrictCriminal
        {
            get { return m_RestrictCriminal; }
            set
            {
                m_RestrictCriminal = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RestrictMurderer
        {
            get { return m_RestrictMurderer; }
            set
            {
                m_RestrictMurderer = value;
                InvalidateProperties();
            }
        }
        
        [CommandProperty(AccessLevel.GameMaster)]
        public bool RestrictHiddenPlayers
        {
            get { return m_RestrictHiddenPlayers; }
            set
            {
                m_RestrictHiddenPlayers = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RestrictMountedPlayers
        {
            get { return m_RestrictMountedPlayers; }
            set
            {
                m_RestrictMountedPlayers = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RestrictDeadPlayers
        {
            get { return m_RestrictDeadPlayers; }
            set
            {
                m_RestrictDeadPlayers = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DeadPlayersOnly
        {
            get { return m_DeadPlayersOnly; }
            set
            {
                m_DeadPlayersOnly = value;
                InvalidateProperties();
            }
        }

        /* Intended to involve sound and effects but decided to remove it for now
         * This may be added in in the future; Dian
        [CommandProperty(AccessLevel.GameMaster)]
        public bool SourceEffect
        {
            get { return m_SourceEffect; }
            set
            {
                m_SourceEffect = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DestEffect
        {
            get { return m_DestEffect; }
            set
            {
                m_DestEffect = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SoundID
        {
            get { return m_SoundID; }
            set
            {
                m_SoundID = value;
                InvalidateProperties();
            }
        }
        */

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RestrictMale
        {
            get { return m_RestrictMale; }
            set
            {
                m_RestrictMale = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RestrictFemale
        {
            get { return m_RestrictFemale; }
            set
            {
                m_RestrictFemale = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RestrictHuman
        {
            get { return m_RestrictHuman; }
            set
            {
                m_RestrictHuman = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RestrictElf
        {
            get { return m_RestrictElf; }
            set
            {
                m_RestrictElf = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RestrictGargoyle
        {
            get { return m_RestrictGargoyle; }
            set
            {
                m_RestrictGargoyle = value;
                InvalidateProperties();
            }
        }
        #endregion

        /*
        * Default 'Active' setting allows everything to pass over
        * Staff will need to set the properties of what to restrict
        * */
        [Constructable]
        public PathBlocker() : base(0x1822)
        {
            Name = "Path Blocker";
            m_Active = true;
            Movable = false;
            Visible = false;
        }

        public PathBlocker(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel > AccessLevel.Player)
            {
                from.CloseGump(typeof(PropertiesGump));
                from.SendGump(new PropertiesGump(from, this));
            }

            else
                from.SendMessage("You have no idea what to do with that");
        }

        public override bool OnMoveOver(Mobile m)
        {
            if (!Active)
                return true;

            if (m is PlayerMobile)
            {
                PlayerMobile player = m as PlayerMobile;

                if (player.AccessLevel == AccessLevel.Player)
                {
                    if (RestrictPlayers)
                    {
                        if(ReadyToMessage())
                            RestrictionMessage(player, "Only the staff members of this shard may pass");
                        //player.SendMessage("Only the staff members of this shard may pass");
                        return false;
                    }
                    if (DeadPlayersOnly && player.Alive)
                    {
                        if (ReadyToMessage())
                            RestrictionMessage(player, "Only the dead may pass");
                        //player.SendMessage("Only the dead may pass");
                        return false;
                    }
                    if (RestrictDeadPlayers && !player.Alive)
                    {
                        if (ReadyToMessage())
                            RestrictionMessage(player, "Only the living may pass");
                        //player.SendMessage("Only the living may pass");
                        return false;
                    }
                    if (RestrictMountedPlayers && player.Mounted)
                    {
                        if (ReadyToMessage())
                            RestrictionMessage(player, "You may not pass while mounted");
                        //player.SendMessage("You may not pass while mounted");
                        return false;
                    }
                    if (RestrictHiddenPlayers && player.Hidden)
                    {
                        if (ReadyToMessage())
                            RestrictionMessage(player, "You must reveal yourself to pass");
                        //player.SendMessage("You must reveal yourself to pass");
                        return false;
                    }
                    if (RestrictMurderer && player.Kills >= 5)
                    {
                        if (ReadyToMessage())
                            RestrictionMessage(player, "Only the more innocent may pass");
                        //player.SendMessage("Only the more innocent may pass");
                        return false;
                    }
                    if (RestrictCriminal && player.Criminal)
                    {
                        if (ReadyToMessage())
                            RestrictionMessage(player, "You are a criminal and may not pass");
                        //player.SendMessage("You are a criminal and may not pass");
                        return false;
                    }
                    if (RestrictCombat && SpellHelper.CheckCombat(player))
                    {
                        if (ReadyToMessage())
                            RestrictionMessage(player, "You may not pass while involved in combat");
                        //player.SendMessage("You may not pass while involved in combat");
                        return false;
                    }
                    if (RestrictMale && !player.Female)
                    {
                        if (ReadyToMessage())
                            RestrictionMessage(player, "Only the female race may pass");
                        //player.SendMessage("Only the female race may pass");
                        return false;
                    }
                    if (RestrictFemale && player.Female)
                    {
                        if (ReadyToMessage())
                            RestrictionMessage(player, "Only the male race may pass");
                        //player.SendMessage("Only the male race may pass");
                        return false;
                    }
                    if (RestrictHuman && player.Race == Race.Human)
                    {
                        if (ReadyToMessage())
                            RestrictionMessage(player, "Human race are not allowed to pass");
                        //player.SendMessage("Human race are not allowed to pass");
                        return false;
                    }
                    if (RestrictElf && player.Race == Race.Elf)
                    {
                        if (ReadyToMessage())
                            RestrictionMessage(player, "Elf race are not allowed to pass");
                        //player.SendMessage("Elf race are not allowed to pass");
                        return false;
                    }
                    if (RestrictGargoyle && player.Race == Race.Gargoyle)
                    {
                        if (ReadyToMessage())
                            RestrictionMessage(player, "Gargoyle race are not allowed to pass");
                        //player.SendMessage("Gargoyle race are not allowed to pass");
                        return false;
                    }
                }
            }

            if (m is BaseCreature)
            {
                BaseCreature creature = m as BaseCreature;

                if(RestrictCreatures && !creature.Controlled)
                {
                    return false;
                }
                if (m_RestrictTamedCreatures && creature.Controlled)
                {
                    //To do: Dismount if tamed creature has a player mounted?
                    if (ReadyToMessage())
                        RestrictionMessage(creature.ControlMaster, "You cannot bring pets past this point");
                    //creature.ControlMaster.SendMessage("You cannot bring pets past this point");
                    return false;
                }
            }

            return true;
        }

        public bool ReadyToMessage()
        {
            if (m_NextMessage > DateTime.UtcNow)
                return false;
            else
                return true;
        }

        public void RestrictionMessage(Mobile mob, String message)
        {
            mob.SendMessage("{0}", message);

            m_NextMessage = DateTime.UtcNow + TimeSpan.FromSeconds(5);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(4);

            //writer.Write(m_SoundID);
            //writer.Write(m_DestEffect);
            //writer.Write(m_SourceEffect);

            writer.Write(m_RestrictGargoyle);
            writer.Write(m_RestrictElf);
            writer.Write(m_RestrictHuman);
            writer.Write(RestrictFemale);
            writer.Write(RestrictMale);

            writer.Write(m_DeadPlayersOnly);
            writer.Write(m_RestrictDeadPlayers);
            writer.Write(m_RestrictMountedPlayers);
            writer.Write(m_RestrictHiddenPlayers);

            writer.Write(m_RestrictMurderer);
            writer.Write(m_RestrictCriminal);
            writer.Write(m_RestrictCombat);

            writer.Write(m_RestrictTamedCreatures);
            writer.Write(m_RestrictCreatures);
            writer.Write(m_RestrictPlayers);

            writer.Write(m_Active);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                //case 5:
                //    {
                //        m_SoundID = reader.ReadInt();
                //        m_DestEffect = reader.ReadBool();
                //        m_SourceEffect = reader.ReadBool();
                //        break;
                //    }
                case 4:
                    {
                        m_RestrictGargoyle = reader.ReadBool();
                        m_RestrictElf = reader.ReadBool();
                        m_RestrictHuman = reader.ReadBool();
                        m_RestrictFemale = reader.ReadBool();
                        m_RestrictMale = reader.ReadBool();
                        goto case 3;
                    }
                case 3:
                    {
                        m_DeadPlayersOnly = reader.ReadBool();
                        m_RestrictDeadPlayers = reader.ReadBool();
                        m_RestrictMountedPlayers = reader.ReadBool();
                        m_RestrictHiddenPlayers = reader.ReadBool();
                        goto case 2;
                    }
                case 2:
                    {
                        m_RestrictMurderer = reader.ReadBool();
                        m_RestrictCriminal = reader.ReadBool();
                        m_RestrictCombat = reader.ReadBool();
                        goto case 1;
                    }
                case 1:
                    {
                        m_RestrictTamedCreatures = reader.ReadBool();
                        m_RestrictCreatures = reader.ReadBool();
                        m_RestrictPlayers = reader.ReadBool();
                        goto case 0;
                    }
                case 0:
                    {
                        m_Active = reader.ReadBool();
                        break;
                    }
            }
        }
    }
}