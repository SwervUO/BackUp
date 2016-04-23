#region Header
//   Vorspire    _,-'/-'/  AddonGenerator.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2015  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
//
//	Based on Arya's "Yet Another Addon Generator"
//	Total rewrite for future support of all artworks (ItemIDs)
//	.NET 4.0 compliant code utilizing Type Generics and Tuples
#endregion

#region References
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Server.Commands;
using Server.Gumps;
using Server.Network;
using Server.Targeting;

using VitaNex.IO;
using VitaNex.Targets;
#endregion

namespace Server.Items
{
	public static class AddonGenerator
	{
		public const string OutputDirectory = "Scripts/__GEN/Addons";

		#region Template
		private const string _Template = @"#region Header
/*
 * Name: ~NAME~
 */
#endregion

#region References
~USING~
#endregion

namespace ~NAMESPACE~
{
	public class ~NAME~Addon : BaseAddon
	{
		private static readonly Tuple<int, Point3D, int, int, int, string>[] _Components = new[]
		{
			~LIST~
		};
		
		public override BaseAddonDeed Deed { get { return new ~NAME~AddonDeed(); } }

		[Constructable]
		public ~NAME~Addon()
		{
			Name = ""~NAME~ Deed"";

			foreach(var o in _Components)
			{
				AddComponent(o.Item1, o.Item2, o.Item3, o.Item4, o.Item5, o.Item6);
			}
		}

        public ~NAME~Addon(Serial serial) 
			: base(serial)
        { }
		
		protected virtual void AddComponent(int itemID, Point3D offset, int amount, int hue, int light, string name)
		{
			AddonComponent ac = new AddonComponent(itemID);

			if (ac.Name != null)
			{
				ac.Name = name;
			}

			if (hue > 0)
			{
				ac.Hue = hue;
			}

			if (amount > 1)
			{
				ac.Stackable = true;
				ac.Amount = amount;
			}

			if (light > -1)
			{
				ac.Light = (LightType)light;
			}

			AddComponent(ac, offset.X, offset.Y, offset.Z);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();
		}
	}

	public class ~NAME~AddonDeed : BaseAddonDeed
	{
		public override BaseAddon Addon { get { return new ~NAME~Addon(); } }

		[Constructable]
		public ~NAME~AddonDeed()
		{
			Name = ""~NAMESPLIT~ Deed"";
		}

		public ~NAME~AddonDeed(Serial serial) 
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();
		}
	}
}";
		#endregion

		public static void Initialize()
		{
			CommandSystem.Register("AddonGen", AccessLevel.Administrator, OnAddonGen);
		}

		[Usage("AddonGen [<name> [namespace]]"), Description("Brings up the addon script generator gump.")]
		private static void OnAddonGen(CommandEventArgs e)
		{
			var states = new object[]
			{
				"", "Server.Items", true, false, false, true, true, true, true, Region.MinZ, Region.MaxZ, 2, (int)UInt16.MaxValue, 2,
				(int)UInt16.MaxValue, 2, (int)UInt16.MaxValue, false
			};

			if (e.Arguments.Length > 0)
			{
				states[0] = e.Arguments[0];

				if (e.Arguments.Length > 1)
				{
					states[1] = e.Arguments[1];
				}
			}

			e.Mobile.SendGump(new InternalGump(e.Mobile, states));
		}

		private static void PickerCallback(Mobile m, Map map, Point3D start, Point3D end, object state)
		{
			var args = state as object[];

			if (args == null)
			{
				return;
			}

			if (start.X > end.X)
			{
				var x = start.X;

				start.X = end.X;
				end.X = x;
			}

			if (start.Y > end.Y)
			{
				var y = start.Y;

				start.Y = end.Y;
				end.Y = y;
			}

			var bounds = new Rectangle2D(start, end);

			var name = args[0] as string;
			var namesplit = name;

			if (name != null)
			{
				namesplit = name.SpaceWords().ToUpperWords();
				name = namesplit.Replace(" ", String.Empty);
			}

			var ns = args[1] as string;

			var getStatics = (bool)args[2];
			var getItems = (bool)args[3];
			var getTiles = (bool)args[4];
			var includeStaticRange = (bool)args[5];
			var includeItemRange = (bool)args[6];
			var includeTileRange = (bool)args[7];
			var includeZRange = (bool)args[8];
			var generateTest = (bool)args[17];

			int minZ, maxZ, minStaticID, maxStaticID, minItemID, maxItemID, minTileID, maxTileID;

			if (!Int32.TryParse(args[9] as string, out minZ))
			{
				minZ = Region.MinZ;
			}

			if (!Int32.TryParse(args[10] as string, out maxZ))
			{
				maxZ = Region.MaxZ;
			}

			if (!Int32.TryParse(args[11] as string, out minStaticID))
			{
				minStaticID = 2;
			}

			if (!Int32.TryParse(args[12] as string, out maxStaticID))
			{
				maxStaticID = UInt16.MaxValue;
			}

			if (!Int32.TryParse(args[13] as string, out minItemID))
			{
				minItemID = 2;
			}

			if (!Int32.TryParse(args[14] as string, out maxItemID))
			{
				maxItemID = UInt16.MaxValue;
			}

			if (!Int32.TryParse(args[15] as string, out minTileID))
			{
				minTileID = 2;
			}

			if (!Int32.TryParse(args[16] as string, out maxTileID))
			{
				maxTileID = UInt16.MaxValue;
			}

			var cList = GetComponents(
				bounds,
				map,
				getTiles,
				getStatics,
				getItems,
				includeZRange,
				minZ,
				maxZ,
				includeTileRange,
				minTileID,
				maxTileID,
				includeStaticRange,
				minStaticID,
				maxStaticID,
				includeItemRange,
				minItemID,
				maxItemID);

			if (cList == null || cList.Count == 0)
			{
				m.SendMessage(0x40, "No components have been selected.");
				m.SendGump(new InternalGump(m, args));
				return;
			}

			var list = String.Join(
				"\n\t\t\t",
				cList.Select((s, i) => s + (i < cList.Count - 1 ? "," : String.Empty) + " // " + (i + 1)));

			var fileOut = new StringBuilder(_Template);

			var useref = "using System;";

			if (!ns.StartsWith("Server"))
			{
				useref += "\nusing Server;";
				useref += "\nusing Server.Items;";
			}
			else if (!ns.StartsWith("Server.Items"))
			{
				useref += "\nusing Server.Items;";
			}

			fileOut.Replace("~USING~", useref);
			fileOut.Replace("~NAMESPACE~", ns);
			fileOut.Replace("~NAME~", name);
			fileOut.Replace("~NAMESPLIT~", namesplit);
			fileOut.Replace("~LIST~", list);

			var path = Path.IsPathRooted(OutputDirectory) ? OutputDirectory : Path.Combine(Core.BaseDirectory, OutputDirectory);

			var file = IOUtility.EnsureFile(path + "/" + name + "Addon.cs", true);

			try
			{
				file.AppendText(true, fileOut.ToString());
			}
			catch (Exception ex)
			{
				ex.ToConsole(true, true);

				m.SendMessage(0x40, "An error occurred while writing the Addon file.");
				return;
			}

			m.SendMessage(0x40, "Addon saved to {0}", file);
			m.SendMessage(0x40, "Total components in Addon: {0}", cList.Count);

			if (!generateTest)
			{
				return;
			}

			var ia = new CEOIdentifyAddon();

			for (var i = 0; i < cList.Count; i++)
			{
				AddTestComponent(ia, cList[i], i + 1);
			}

			m.SendMessage(0x37, "Target a location to place the test Addon...");
			var target =
				m.Target =
					new GenericSelectTarget<IPoint3D>(
						(u, t) => ia.MoveToWorld(t.ToPoint3D(), u.Map),
						u => ia.Delete(),
						-1,
						true,
						TargetFlags.None);

			Timer timer = null;

			timer = Timer.DelayCall(
				TimeSpan.FromSeconds(1.0),
				TimeSpan.FromSeconds(1.0),
				() =>
				{
					if (m.Target == target)
					{
						return;
					}

					if (ia.Map == null || ia.Map == Map.Internal)
					{
						ia.Delete();
					}

					if (timer != null)
					{
						timer.Stop();
					}
				});

			timer.Start();
		}

		private static void AddTestComponent(CEOIdentifyAddon ai, AddonTileInfo info, int index)
		{
			if (ai == null || info == null)
			{
				return;
			}

			var ac = new AddonComponent(info.ItemID)
			{
				Name = String.Format("#{0} {1}", index, info.Offset)
			};

			if (info.Hue > 0)
			{
				ac.Hue = info.Hue;
			}

			if (info.Amount > 1)
			{
				ac.Stackable = true;
				ac.Amount = info.Amount;
			}

			if (info.Light > -1)
			{
				ac.Light = (LightType)info.Light;
			}

			ai.AddComponent(ac, info.X, info.Y, info.Z);
		}

		public static List<AddonTileInfo> GetComponents(
			Rectangle2D bounds,
			Map map,
			bool tiles,
			bool statics,
			bool items,
			bool incZRange,
			int minZ,
			int maxZ,
			bool incTileRange,
			int minTileID,
			int maxTileID,
			bool incStaticRange,
			int minStaticID,
			int maxStaticID,
			bool incItemRange,
			int minItemID,
			int maxItemID)
		{
			var list = new List<AddonTileInfo>(Math.Min(1000, bounds.Width * bounds.Height));

			if (tiles)
			{
				foreach (var p in bounds.EnumeratePoints())
				{
					list.AddRange(
						map.GetStaticTiles(p, true)
						   .Where(t => !incZRange || (t.Z >= minZ && t.Z <= maxZ))
						   .Where(t => !incTileRange || (t.ID >= minTileID && t.ID <= maxTileID))
						   .Select(t => new AddonTileInfo(t.ID, t.ToPoint3D(), 1, t.Hue, -1, null)));
				}
			}

			IEnumerable<Item> check = null;

			if (items)
			{
				check = bounds.FindEntities<Item>(map);
			}

			if (statics && check == null)
			{
				check = bounds.FindEntities<Static>(map);
			}
			else if (!statics && check != null)
			{
				check = check.Not(i => i is Static);
			}

			if (check != null)
			{
				foreach (var i in check.Where(t => !incZRange || (t.Z >= minZ && t.Z <= maxZ)))
				{
					if (i is Static)
					{
						if (incStaticRange && (i.ItemID < minStaticID || i.ItemID > maxStaticID))
						{
							continue;
						}
					}
					else
					{
						if (incItemRange && (i.ItemID < minItemID || i.ItemID > maxItemID))
						{
							continue;
						}
					}

					list.Add(new AddonTileInfo(i.ItemID, i.Location, i.Amount, i.Hue, (int)i.Light, i.Name));
				}
			}

			// Get center
			var center = new Point3D(0, 0, Region.MaxZ);

			int x1 = bounds.End.X, y1 = bounds.End.Y;
			int x2 = bounds.Start.X, y2 = bounds.Start.Y;

			// Get bounds
			foreach (var o in list)
			{
				center.Z = Math.Min(center.Z, o.Z);

				x1 = Math.Min(x1, o.X);
				y1 = Math.Min(y1, o.Y);

				x2 = Math.Max(x2, o.X);
				y2 = Math.Max(y2, o.Y);
			}

			center.X = x1 + ((x2 - x1) / 2);
			center.Y = y1 + ((y2 - y1) / 2);

			list.ForEach(o => o.Offset = o.Clone3D(-center.X, -center.Y, -center.Z));
			list.Free(false);

			return list;
		}

		public sealed class AddonTileInfo : IPoint3D
		{
			public int ItemID { get; set; }

			public Point3D Offset { get; set; }

			public int Amount { get; set; }
			public int Hue { get; set; }
			public int Light { get; set; }
			public string Name { get; set; }

			public int X { get { return Offset.X; } }
			public int Y { get { return Offset.Y; } }
			public int Z { get { return Offset.Z; } }

			public bool IsComplex { get { return Amount > 1 || Hue > 0 || Light >= 0 || !String.IsNullOrWhiteSpace(Name); } }

			public AddonTileInfo(int itemID, Point3D offset, int amount, int hue, int light, string name)
			{
				ItemID = itemID;
				Offset = offset;
				Amount = amount;
				Hue = hue;
				Light = light;
				Name = name;
			}

			public override string ToString()
			{
				return String.Format(
					"Tuple.Create({0}, new Point3D{1}, {2}, {3}, {4}, {5})",
					ItemID,
					Offset,
					Amount,
					Hue,
					Light,
					Name == null ? "(string)null" : "\"" + Name + "\"");
			}
		}

		private sealed class InternalGump : Gump
		{
			private const int LabelHue = 0x480;
			private const int TitleHue = 0x35;

			private readonly object[] _State;

			public InternalGump(Mobile m, object[] state)
				: base(100, 50)
			{
				m.CloseGump(typeof(InternalGump));

				_State = state;

				Closable = true;
				Disposable = true;
				Dragable = true;
				Resizable = false;

				AddPage(0);

				AddBackground(0, 0, 440, 260, 9260);
				AddAlphaRegion(10, 10, 430, 260); //uncomment this line if you like see-thru menus
				AddHtml(0, 15, 440, 20, Center(Color("Addon Generator", 0x000080)), false, false);

				var x = 40;

				AddLabel(20, x, LabelHue, @"Name");
				AddImageTiled(95, x, 165, 18, 9274);
				AddTextEntry(95, x, 165, 20, LabelHue, 0, _State[0] as string); // Name

				x += 20;

				AddLabel(20, x, LabelHue, @"Namespace");
				AddImageTiled(95, x, 165, 18, 9274);
				AddTextEntry(95, x, 165, 20, LabelHue, 1, _State[1] as string); // Namespace
				AddLabel(340, x, TitleHue, @"ID Range");

				x += 20;

				AddLabel(20, x, TitleHue, @"Export");
				AddLabel(170, x, TitleHue, @"ID Range");
				AddLabel(320, x, TitleHue, @"Include/Exclude");

				x += 25;

				// Export Statics, Items, and Tiles
				var exportString = new[] {"Statics", "Items", "Tiles"};

				for (var i = 0; i < 3; i++)
				{
					DisplayExportLine(
						x,
						i,
						(bool)_State[i + 2],
						(bool)_State[i + 5],
						exportString[i],
						_State[11 + (i * 2)].ToString(),
						_State[12 + (i * 2)].ToString());
					x += (i < 2 ? 25 : 15);
				}

				AddImageTiled(15, x + 15, 420, 1, 9304);

				x += 25;

				// Z Range
				AddCheck(350, x, 9026, 9027, (bool)_State[8], 6);
				AddLabel(20, x, LabelHue, @"Z Range");
				AddImageTiled(115, x + 15, 50, 1, 9274);
				AddTextEntry(115, x - 5, 50, 20, LabelHue, 2, _State[9].ToString());
				AddLabel(185, x, LabelHue, @"to");
				AddImageTiled(225, x + 15, 50, 1, 9274);
				AddTextEntry(225, x - 5, 50, 20, LabelHue, 3, _State[10].ToString());

				x += 25;

				// Buttons
				AddButton(20, x, 4020, 4021, 0, GumpButtonType.Reply, 0);
				AddLabel(55, x, LabelHue, @"Cancel");
				AddButton(155, x, 4005, 4006, 1, GumpButtonType.Reply, 0);
				AddLabel(195, x, LabelHue, @"Generate");
				AddButton(300, x, 4005, 4006, 2, GumpButtonType.Reply, 0);
				AddLabel(340, x, LabelHue, @"Test & Gen");
			}

			private void DisplayExportLine(int x, int index, bool state, bool include, string heading, string min, string max)
			{
				AddCheck(20, x, 9026, 9027, state, index);
				AddLabel(40, x, LabelHue, heading);
				AddImageTiled(115, x + 15, 50, 1, 9274);
				AddTextEntry(115, x - 5, 50, 20, LabelHue, 4 + (index * 2), min); // Tile ID Min
				AddLabel(185, x, LabelHue, @"to");
				AddImageTiled(225, x + 15, 50, 1, 9274);
				AddTextEntry(225, x - 5, 50, 20, LabelHue, 5 + (index * 2), max); // Tile ID Max
				AddCheck(350, x, 9026, 9027, include, index + 3); // Include or Exclude compare?
			}

			private static string Center(string text)
			{
				return String.Format("<CENTER>{0}</CENTER>", text);
			}

			private static string Color(string text, int color)
			{
				return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</COLOR>", color, text);
			}

			public override void OnResponse(NetState sender, RelayInfo info)
			{
				switch (info.ButtonID)
				{
					case 0:
						return;
					case 1:
						_State[17] = false;
						break;
					default:
						_State[17] = true;
						break;
				}

				foreach (var text in info.TextEntries)
				{
					_State[text.EntryID < 2 ? text.EntryID : text.EntryID + 7] = text.Text;
				}

				// Reset checks
				for (var x = 2; x <= 8; x++)
				{
					_State[x] = false;
				}

				foreach (var check in info.Switches)
				{
					_State[check + 2] = true; // Offset by 2 in the state object
				}

				if (Verify(sender.Mobile, _State))
				{
					BoundingBoxPicker.Begin(sender.Mobile, PickerCallback, _State);
				}
				else
				{
					sender.Mobile.SendMessage(0x40, "Please review the generation parameters, some are invalid.");
					sender.Mobile.SendGump(new InternalGump(sender.Mobile, _State));
				}
			}

			private static bool Verify(Mobile from, object[] state)
			{
				if (String.IsNullOrWhiteSpace(state[0] as string))
				{
					from.SendMessage(0x40, "Name field is invalid or missing.");
					return false;
				}

				if (String.IsNullOrWhiteSpace(state[1] as string))
				{
					from.SendMessage(0x40, "Namespace field is invalid or missing.");
					return false;
				}

				if (!((bool)state[2] || (bool)state[3] || (bool)state[4]))
				{
					from.SendMessage(0x40, "You must have least one Export button selected. (Static/Items/Tiles)");
					return false;
				}

				var errors = new[]
				{
					"Z Range Min", "Z Range Max", "Static Min ID", "Static Max ID", "Item Min ID", "Item Max ID", "Tile Min ID",
					"Tile Max ID"
				};

				for (var x = 0; x < 8; x++)
				{
					if (CheckNumber(state[x + 9] as string))
					{
						continue;
					}

					from.SendMessage(0x40, "There's a problem with the {0} field.", errors[x]);
					return false;
				}

				return true;
			}

			private static bool CheckNumber(string number)
			{
				int value;

				return Int32.TryParse(number, out value);
			}
		}
	}

	public class CEOIdentifyAddon : BaseAddon
	{
		public CEOIdentifyAddon()
		{ }

		public CEOIdentifyAddon(Serial serial)
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();

			if (Map == null || Map == Map.Internal)
			{
				Delete();
			}
		}
	}
}