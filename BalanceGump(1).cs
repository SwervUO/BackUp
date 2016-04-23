using System;
using System.Globalization;
using System.Collections;
using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using Server.Regions;
using Server.Targeting;

namespace Server.Gumps
{
    

    public class MyBalanceGump : Gump
    {
        private const int LabelColor = 0x7FFF;
		private const int LabelHue = 0x480;
        private const int LabelColorDisabled = 0x4210;
        private readonly Mobile m_From;
        public MyBalanceGump(Mobile from)
            : base(50, 50)
        {
            this.m_From = from;

            from.CloseGump(typeof(MyBalanceGump));

            this.AddPage(0);

            this.AddBackground(0, 0, 270, 145, 5054);

            this.AddImageTiled(10, 10, 250, 125, 2624);
            this.AddAlphaRegion(10, 10, 250, 125);

            this.AddHtmlLocalized(10, 10, 250, 20, 1112662, LabelColor, false, false); // Balance

            this.AddButton(10, 110, 4017, 4019, 0, GumpButtonType.Reply, 0);
            this.AddHtmlLocalized(45, 110, 150, 20, 3000363, LabelColor, false, false); // Close

            this.AddHtmlLocalized(45, 60, 200, 20, 1060645, LabelColor, false, false); // Bank Balance:
            this.AddLabel(150, 60, LabelHue, Banker.GetBalance(from).ToString("N0", CultureInfo.GetCultureInfo("en-US")));

          
        }

   
    }
}