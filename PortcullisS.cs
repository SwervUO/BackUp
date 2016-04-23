using System;
using System.Collections;
using System.Collections.Generic;
using Server.Network;
using Server.Targeting;
using Server.Commands;

namespace Server.Items
{
	public class PortcullisS : Item, ILockable
    {
        #region Properties
        private bool m_AutoClose, m_Open, m_Locked;
        private PortcullisS m_Link;
		private uint m_KeyValue;
		private int m_BaseZ;
		private int m_RaiseAmount;
        private int m_SoundID1;
        private int m_SoundID2;
        private Timer m_CloseTimer;

        [CommandProperty(AccessLevel.Seer)]
        public bool AutoClose
        {
            get { return m_AutoClose; }
            set { m_AutoClose = value; }
        }

		[CommandProperty( AccessLevel.Seer )]
        public bool Open
        {
            get { return m_Open; }
            set 
            {
                m_Open = value;

                if (m_AutoClose)
                {
                    if (this.m_Open)
                        this.m_CloseTimer.Start();
                    else
                        this.m_CloseTimer.Stop();
                }
            }
        }

        [CommandProperty(AccessLevel.Seer)]
        public bool Locked
        {
            get { return m_Locked; }
            set { m_Locked = value; }
        }

		[CommandProperty( AccessLevel.Seer )]
		public uint KeyValue
		{
			get { return m_KeyValue; }
			set { m_KeyValue = value; }
		}

		[CommandProperty( AccessLevel.Seer )]
		public int BaseZ
		{
			get { return m_BaseZ; }
			set { m_BaseZ = value; }
		}

		[CommandProperty( AccessLevel.Seer )]
		public int RaiseAmount
		{
			get { return m_RaiseAmount; }
			set { m_RaiseAmount = value; }
		}

        [CommandProperty(AccessLevel.Seer)]
        public int SoundID1
        {
            get { return m_SoundID1; }
            set { m_SoundID1 = value; }
        }

        [CommandProperty(AccessLevel.Seer)]
        public int SoundID2
        {
            get { return m_SoundID2; }
            set { m_SoundID2 = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public PortcullisS Link
        {
            get
            {
                if (this.m_Link != null && this.m_Link.Deleted)
                    this.m_Link = null;

                return this.m_Link;
            }
            set
            {
                this.m_Link = value;
            }
        }

        public virtual bool UseChainedFunction
        {
            get
            {
                return true;
            }
        }
        #endregion

        public static void Initialize()
        {
            CommandSystem.Register("LinkPortcullis", AccessLevel.GameMaster, new CommandEventHandler(LinkPortcullis_OnCommand));
            CommandSystem.Register("ChainLinkPortcullis", AccessLevel.GameMaster, new CommandEventHandler(ChainLinkPortcullis_OnCommand));
        }

        [Constructable]
		public PortcullisS() : base ( 0x6F5 )
		{
            this.m_CloseTimer = new CloseTimer(this);
            AutoClose = false;  //Will not reclose by itself by default
			Locked = false;     //Locked false by default, not yet completed working key locking system
			//BaseZ = this.Z;     //Set the BaseZ at time of creation to establish ground level
			Movable = false;    //Of course needs to be non movable besides its d-click operations
			RaiseAmount = 16;   //Default amount to raise, typical heigth for players to go under- can adjust for custom areas in game
            SoundID1 = 0x0F0;   //Pure Raise/Lower Portcullis winding chain sound
            SoundID2 = 0x0EE;   //Final Lowering sound with Slamming effect at end.
		}

        public virtual void OnOpened(Mobile from)
        {
        }

        public virtual void OnClosed(Mobile from)
        {
        }

        public virtual bool UseLocks()
        {
            return true;
        }

        public List<PortcullisS> GetChain()
        {
            List<PortcullisS> list = new List<PortcullisS>();
            PortcullisS c = this;

            do
            {
                list.Add(c);
                c = c.Link;
            }
            while (c != null && !list.Contains(c));

            return list;
        }

		public override void OnDoubleClick(Mobile from)
		{
            if (from.AccessLevel == AccessLevel.Player && (/*!from.InLOS( this ) || */!from.InRange(GetWorldLocation(), 2)))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            }

            else if (this.Z != this.BaseZ && (this.Z > this.BaseZ && this.Z < this.BaseZ + 16))
            {
                from.SendMessage("The portcullis is already in motion.");
                return;
            }

            else
                VerifyAndUse(from);
		}

        public virtual void VerifyAndUse(Mobile from)
		{
            if (m_Locked && !m_Open && this.UseLocks())
			{
                if (from.AccessLevel >= AccessLevel.GameMaster)
                {
                    from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502502); // That is locked, but you open it with your godly powers.
                }
                else if (Key.ContainsKey(from.Backpack, this.KeyValue))
                {
                    from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501282); // You quickly unlock, open, and relock the door
                }

                else
                {
                    from.SendMessage("The portcullis seems to be locked and wont budge.");
                    return;
                }
			}

            if (this.m_Open && !this.IsFreeToClose())
                return;

            if (this.m_Open)
                this.OnClosed(from);
            else
                this.OnOpened(from);

            if (this.UseChainedFunction) //&& !m_Locked)
            {
                List<PortcullisS> list = this.GetChain();

                for (int i = 0; i < list.Count; ++i)
                    new PortcullisOpenTimer(list[i]).Start();
                return;
            }

            else
                new PortcullisOpenTimer(this).Start();
		}

        public virtual void VerifyAndUse()
        {
            if (this.UseChainedFunction)
            {
                List<PortcullisS> list = this.GetChain();

                for (int i = 0; i < list.Count; ++i)
                    new PortcullisOpenTimer(list[i]).Start();
                return;
            }

            else
                new PortcullisOpenTimer(this).Start();
        }

        public bool CanClose()
        {
            //Leaving room for later development here; Dian
            Map map = this.Map;

            if (map == null)
                return false;
            else
                return true;
        }

        public bool IsFreeToClose()
        {
            if (!this.UseChainedFunction)
                return this.CanClose();

            List<PortcullisS> list = this.GetChain();

            bool freeToClose = true;

            for (int i = 0; freeToClose && i < list.Count; ++i)
                freeToClose = list[i].CanClose();

            return freeToClose;
        }

		private class PortcullisOpenTimer : Timer
		{
			private PortcullisS m_PortcullisS;

			public PortcullisOpenTimer( PortcullisS portcullisNS ) : base( TimeSpan.FromSeconds( 0.30 ) )
			{
				Priority = TimerPriority.FiftyMS;

				m_PortcullisS = portcullisNS;
				
			}

			protected override void OnTick()
			{
				if( !m_PortcullisS.Open )
				{
					if( m_PortcullisS.Z <= (m_PortcullisS.BaseZ + m_PortcullisS.RaiseAmount ) ) 
					{
						Effects.PlaySound( m_PortcullisS.Location, m_PortcullisS.Map, m_PortcullisS.SoundID1 );
						m_PortcullisS.Z += 1;
						Start();
					}
					else
					{
						m_PortcullisS.Open = true;
						Stop();

                        //if (m_PortcullisS.m_AutoClose == true)
                        //    m_PortcullisS.m_CloseTimer.Start();
					}
				}
				else
				{
					if( m_PortcullisS.Z < m_PortcullisS.BaseZ + 4 && m_PortcullisS.Z > m_PortcullisS.BaseZ  )
					{
						m_PortcullisS.Z -= 1;
						Start();
					}

					if( m_PortcullisS.Z <= m_PortcullisS.BaseZ )
					{
                        Effects.PlaySound(m_PortcullisS.Location, m_PortcullisS.Map, m_PortcullisS.SoundID2);
						m_PortcullisS.Open = false;
						Stop();
                        //if (m_PortcullisS.m_AutoClose == true)
                        //    m_PortcullisS.m_CloseTimer.Stop();
					}
					else
					{
                        Effects.PlaySound(m_PortcullisS.Location, m_PortcullisS.Map, m_PortcullisS.SoundID1);
						m_PortcullisS.Z -= 1;
						Start();
					}
				}
			}
		}

        private class CloseTimer : Timer
        {
            private readonly PortcullisS m_Portcullis;
            public CloseTimer(PortcullisS portcullis)
                : base(TimeSpan.FromSeconds(5.0))
            {
                this.Priority = TimerPriority.OneSecond;
                this.m_Portcullis = portcullis;
            }

            protected override void OnTick()
            {
                if (this.m_Portcullis.Open && this.m_Portcullis.IsFreeToClose())
                    this.m_Portcullis.VerifyAndUse();
            }
        }

		public PortcullisS( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer ) 
		{ 
			base.Serialize( writer ); 
			writer.Write( (int) 0 );

            writer.Write(this.m_Link);
            writer.Write((bool)m_AutoClose);
            writer.Write((bool)m_Open);
            writer.Write((bool)m_Locked);
            writer.Write((uint)m_KeyValue);
            writer.Write((int)m_BaseZ);
            writer.Write((int)m_RaiseAmount);

            writer.Write((int)m_SoundID1);
            writer.Write((int)m_SoundID2);
		} 
		
		public override void Deserialize( GenericReader reader ) 
		{ 
			base.Deserialize( reader ); 
			int version = reader.ReadInt();

            m_Link = reader.ReadItem() as PortcullisS;
            m_AutoClose = reader.ReadBool();
            m_Open = reader.ReadBool();
            m_Locked = reader.ReadBool();
            m_KeyValue = reader.ReadUInt();
            m_BaseZ = reader.ReadInt();
            m_RaiseAmount = reader.ReadInt();

            m_SoundID1 = reader.ReadInt();
            m_SoundID2 = reader.ReadInt();

            this.m_CloseTimer = new CloseTimer(this);

            if (this.m_Open)
                this.m_CloseTimer.Start();
        }

        #region Link Methods
        [Usage("LinkPortcullis")]
        [Description("Links two targeted Portcullis together.")]
        private static void LinkPortcullis_OnCommand(CommandEventArgs e)
        {
            e.Mobile.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(Link_OnFirstTarget));
            e.Mobile.SendMessage("Target the first portcullis to link.");
        }

        private static void Link_OnFirstTarget(Mobile from, object targeted)
        {
            PortcullisS door = targeted as PortcullisS;

            if (door == null)
            {
                from.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(Link_OnFirstTarget));
                from.SendMessage("That is not a portcullis. Try again.");
            }
            else
            {
                from.BeginTarget(-1, false, TargetFlags.None, new TargetStateCallback(Link_OnSecondTarget), door);
                from.SendMessage("Target the second portcullis to link.");
            }
        }

        private static void Link_OnSecondTarget(Mobile from, object targeted, object state)
        {
            PortcullisS first = (PortcullisS)state;
            PortcullisS second = targeted as PortcullisS;

            if (second == null)
            {
                from.BeginTarget(-1, false, TargetFlags.None, new TargetStateCallback(Link_OnSecondTarget), first);
                from.SendMessage("That is not a portcullis. Try again.");
            }
            else
            {
                first.Link = second;
                second.Link = first;
                from.SendMessage("The portcullis have been linked.");
            }
        }

        [Usage("ChainLinkPortcullis")]
        [Description("Chain-links two or more targeted portcullis together.")]
        private static void ChainLinkPortcullis_OnCommand(CommandEventArgs e)
        {
            e.Mobile.BeginTarget(-1, false, TargetFlags.None, new TargetStateCallback(ChainLinkPortcullis_OnTarget), new List<PortcullisS>());
            e.Mobile.SendMessage("Target the first of a sequence of portcullis to link.");
        }

        private static void ChainLinkPortcullis_OnTarget(Mobile from, object targeted, object state)
        {
            PortcullisS door = targeted as PortcullisS;

            if (door == null)
            {
                from.BeginTarget(-1, false, TargetFlags.None, new TargetStateCallback(ChainLinkPortcullis_OnTarget), state);
                from.SendMessage("That is not a portcullis. Try again.");
            }
            else
            {
                List<PortcullisS> list = (List<PortcullisS>)state;

                if (list.Count > 0 && list[0] == door)
                {
                    if (list.Count >= 2)
                    {
                        for (int i = 0; i < list.Count; ++i)
                            list[i].Link = list[(i + 1) % list.Count];

                        from.SendMessage("The chain of portcullis have been linked.");
                    }
                    else
                    {
                        from.BeginTarget(-1, false, TargetFlags.None, new TargetStateCallback(ChainLinkPortcullis_OnTarget), state);
                        from.SendMessage("You have not yet targeted two unique portcullis. Target the second portcullis to link.");
                    }
                }
                else if (list.Contains(door))
                {
                    from.BeginTarget(-1, false, TargetFlags.None, new TargetStateCallback(ChainLinkPortcullis_OnTarget), state);
                    from.SendMessage("You have already targeted that portcullis. Target another portcullis, or retarget the first portcullis to complete the chain.");
                }
                else
                {
                    list.Add(door);

                    from.BeginTarget(-1, false, TargetFlags.None, new TargetStateCallback(ChainLinkPortcullis_OnTarget), state);

                    if (list.Count == 1)
                        from.SendMessage("Target the second portcullis to link.");
                    else
                        from.SendMessage("Target another portcullis to link. To complete the chain, retarget the first portcullis.");
                }
            }
        }
        #endregion
    }

    public class PortcullisE : PortcullisS
    {
        [Constructable]
		public PortcullisE() : base()
		{
            AutoClose = false;
            ItemID = 0x6F6;
			Locked = false;
			Movable = false;
			RaiseAmount = 16;
            SoundID1 = 0x0F0; //Pure Raise/Lower Portcullis winding chain sound
            SoundID2 = 0x0EE; //Final Lowering sound with Slamming effect at end.
		}

        public PortcullisE( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer ) 
		{
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt(); 
        }
    }
}
