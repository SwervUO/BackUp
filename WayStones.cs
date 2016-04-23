using System;
using Server;
using Server.Engines.PartySystem;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
	public class WayStone : Item
	{
		[Constructable]
		public WayStone() : base( 0x023B )
		{
			Name = "Way Stone.";
		}
		
		public override void OnDoubleClick(Mobile from) {
			if(from.AccessLevel > AccessLevel.Player || from.Backpack == this.Parent)
			{
				Party p = Party.Get(from);
				if(p != null && p.Members.Count > 1)
				{
					for(int i = 0; i < p.Members.Count; i++)
					{
						Mobile m = ((PartyMemberInfo)p.Members[i]).Mobile;
						if(m != from)
						{
							m.SendGump(new InternalGump(from));
							object[] arg = new object[] {m};
							Timer.DelayCall( TimeSpan.FromMinutes( 2.0 ), new TimerStateCallback( CloseInternalGump ), arg);
						}
						else
						{
							m.SendMessage("You open a request to join you at your location for each of your party members.");
						}
					}
					this.Delete();
				}
				else
				{
					from.SendMessage("You can not use this while not in a party.");
				}
			}
			else
			{
				from.SendMessage("That must be in your backpack to be used.");
			}
		}
		
		public static void CloseInternalGump(object state)
		{
			object[] states = (object[])state;
			Mobile m = (Mobile)states[0];
			if(m.HasGump(typeof(InternalGump)))
				m.CloseGump(typeof(InternalGump));
		}
		
		public WayStone( Serial serial ) : base( serial )
		{
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

			switch ( version )
			{
				case 0:
				{
					break;
				}
			}
		}
	}
	
	public class InternalGump : Gump
    {
		private Mobile m_Summoner;
		
		public InternalGump(Mobile from) : base( 0, 0 )
        {
			m_Summoner = from;
            this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;

			AddPage(0);
			AddBackground(180, 57, 396, 100, 2600);
			AddImage(264, 35, 1419);
			AddImageTiled(338, 34, 86, 43, 2601);
			AddLabel(338, 47, 0, "Way Stone");
			AddLabel(278, 76, 0, String.Format("You Are Being Summond By {0}.", from.Name));
			AddLabel(300, 90, 0, "Do You Want To Accept?");
			AddButton(304, 113, 247, 248, 1, GumpButtonType.Reply, 0);
			AddButton(385, 114, 243, 241, 0, GumpButtonType.Reply, 0);
        }
		
		public override void OnResponse(NetState sender, RelayInfo info)
		{
			Mobile from = sender.Mobile;
			
			if(info.ButtonID == 1)
			{
				from.FixedParticles( 0x376A, 9, 32, 5030, EffectLayer.Waist );
				from.MoveToWorld(m_Summoner.Location, m_Summoner.Map);
				from.FixedParticles( 0x376A, 9, 32, 5030, EffectLayer.Waist );
			}
		}
	}
}