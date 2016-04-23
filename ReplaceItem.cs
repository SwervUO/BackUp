//Visam
using System; 
using System.Collections; 
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Commands;

namespace Server.Commands
{
	public class Replace
	{
		public static void Initialize()
		{
			CommandSystem.Register( "ReplaceItemByType", AccessLevel.Administrator, new CommandEventHandler( ReplaceItemByType_OnCommand ) );
		}

		[Usage( "Replaces first item with the second. ReplaceItemByType <OldItemType> <NewItemType>" )]
		[Description( "Replaces first item with the second. Specified by Type." )]
		public static void ReplaceItemByType_OnCommand( CommandEventArgs e )
		{
			if ( e.Length == 2 )
			{
				Type OldItem = ScriptCompiler.FindTypeByName( e.GetString( 0 ), true );
				Type NewItem = ScriptCompiler.FindTypeByName( e.GetString( 1 ), true );

				ArrayList OldItemslist = new ArrayList();

				foreach ( Item item in World.Items.Values )
				{
					if ( item.GetType() == OldItem )
						OldItemslist.Add( item );
				}

				e.Mobile.SendMessage( "ReplaceItemByType " + OldItemslist.Count + "] items");

				for ( int i=0 ; i < OldItemslist.Count ; i++ )
				{
					Item item = (Item)OldItemslist[i];
					Item ItemNew = null;
					try
					{
						ItemNew = Activator.CreateInstance(NewItem) as Item;
					}
					catch {e.Mobile.SendMessage( "An error has ocurred or invalid item type... Please Check Format: ReplaceItemByType <OldItemType> <NewItemType>" ); }

					if (ItemNew != null) 
					{
						if (item.Parent is BaseCreature)
						{
							item.Delete();
						}
						else if (item.Parent is PlayerMobile)
						{
							PlayerMobile pm = (PlayerMobile)item.Parent;
							Container cont = pm.Backpack;
							if (!item.Movable)
								ItemNew.Movable = false;
							
							item.Delete();
							cont.DropItem(ItemNew);

							ItemNew.Amount = item.Amount;
							ItemNew.LootType = item.LootType;
						}
						else if (item.Parent is Container)
						{
							Container cont = (Container)item.Parent;
							if (!item.Movable)
								ItemNew.Movable = false;
							
							item.Delete();
							cont.DropItem(ItemNew);

							ItemNew.Amount = item.Amount;
							ItemNew.LootType = item.LootType;
						}
						else
						{
							ItemNew.MoveToWorld(item.Location, item.Map);
							if (!item.Movable)
							ItemNew.Movable = false;

							ItemNew.Amount = item.Amount;
							ItemNew.LootType = item.LootType;
					
							item.Delete();
						}
					}
				}
				e.Mobile.SendMessage ("ReplaceItemByType Complete!");
			}
			else
			{
				e.Mobile.SendMessage( "Format: ReplaceItemByType <OldItemType> <NewItemType>" );
			}
		}
	}
}