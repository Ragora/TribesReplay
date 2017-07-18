//------------------------------------------------------------------------------
//
// ChooseFilterDlg.cs
//
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function ChooseFilterDlg::onWake( %this )
{
   CF_FilterList.clear();
   CF_FilterList.addRow( 0, "All servers" );
   CF_FilterList.addRow( 1, "Servers with buddies" );
   for ( %i = 0; $pref::ServerBrowser::Filter[%i] !$= ""; %i++ )
      CF_FilterList.addRow( %i + 2, $pref::ServerBrowser::Filter[%i] );

   if ( $pref::ServerBrowser::activeFilter >= %i + 2 )
      $pref::ServerBrowser::activeFilter = 0;

   CF_FilterList.setSelectedById( $pref::ServerBrowser::activeFilter );

	CF_GoBtn.makeFirstResponder( 1 );
}

//------------------------------------------------------------------------------
function ChooseFilterDlg::onSleep( %this )
{
   // export out all the filters...
   %count = CF_FilterList.rowCount();
   for ( %row = 2; %row < %count; %row++ )
      $pref::ServerBrowser::Filter[%row - 2] = CF_FilterList.getRowText( %row );
}

//------------------------------------------------------------------------------
function ChooseFilterDlg::newFilter( %this )
{
   // get an updated list of game types:
   queryMasterGameTypes();
   %this.editFilterIndex = CF_FilterList.rowCount();

   FilterEditName.setValue( "New Filter" );
   FilterEditMinPlayers.setValue( 0 );
   FilterEditMaxPlayers.setValue( 255 );
   FilterEditGameType.setText( "Any" );
   FilterEditMissionType.setText( "Any" );
   for ( %i = 0; isObject( "FilterEditLocMask" @ %i ); %i++ )
      ( "FilterEditLocMask" @ %i ).setValue( true );
   FilterEditUsePingTgl.setValue( false );
   FilterEditMaxPing.setValue( 50 );
   FilterEditMaxPing.setVisible( false );

   Canvas.pushDialog( FilterEditDlg );
}

//------------------------------------------------------------------------------
function ChooseFilterDlg::editFilter( %this )
{
   %rowId = CF_FilterList.getSelectedId();
   if ( %rowId < 2 ) // can't edit default filters
      return;

   // get an updated list of game types:
   queryMasterGameTypes();
   %rowText = CF_FilterList.getRowTextById( %rowId );
   %filterName = getField( %rowText, 0 );
   %gameType = getField( %rowText, 1 );
   %misType = getField( %rowText, 2 );
   %minPlayers = getField( %rowText, 3 );
   %maxPlayers = getField( %rowText, 4 );
   %regionCode = getField( %rowText, 5 );
   %maxPing = getField( %rowText, 6 );
   
   FilterEditName.setValue( %filterName );
   FilterEditMinPlayers.setValue( %minPlayers );
   FilterEditMaxPlayers.setValue( %maxPlayers );
   FilterEditGameType.setText( %gameType );
   FilterEditMissionType.setText( %misType );
   %index = 0;
   while ( isObject( "FilterEditLocMask" @ %index ) )
   {
      ( "FilterEditLocMask" @ %index ).setValue( %regionCode & 1 );
      %index++;
      %regionCode >>= 1;
   }

   if ( %maxPing == 0 )
   {
      FilterEditUsePingTgl.setValue( false );
      FilterEditMaxPing.setValue( 50 );
      FilterEditMaxPing.setVisible( false );
   }
   else
   {
      FilterEditUsePingTgl.setValue( true );
      FilterEditMaxPing.setValue( %maxPing );
      FilterEditMaxPing.setVisible( true );
   }

   %this.editFilterIndex = %rowId;
   Canvas.pushDialog( FilterEditDlg );
}

//------------------------------------------------------------------------------
function ChooseFilterDlg::saveFilter( %this )
{
   %filterName = FilterEditName.getValue();
   %gameType = FilterEditGameType.getText();
   %misType = FilterEditMissionType.getText();
   %minPlayers = FilterEditMinPlayers.getValue();
   %maxPlayers = FilterEditMaxPlayers.getValue();
   %regionCode = 0;
   for ( %i = 0; isObject( "FilterEditLocMask" @ %i ); %i++ )
   {
      if ( ( "FilterEditLocMask" @ %i ).getValue() )
         %regionCode |= ( 1 << %i );
   }
   %maxPing = FilterEditUsePingTgl.getValue() ? FilterEditMaxPing.getValue() : 0;
   %row = %filterName TAB %gameType TAB %misType
        TAB %minPlayers TAB %maxPlayers TAB %regionCode TAB %maxPing;
        
   CF_FilterList.setRowById( %this.editFilterIndex, %row );
   CF_FilterList.setSelectedById( %this.editFilterIndex );
   %this.editFilterIndex = "";
   Canvas.popDialog( FilterEditDlg );
}

//------------------------------------------------------------------------------
function ChooseFilterDlg::deleteFilter( %this )
{
   %rowId = CF_FilterList.getSelectedId();
   if ( %rowId < 2 ) // can't delete default filters
      return;

   %row = CF_FilterList.getRowNumById( %rowId );
   %lastFilter = CF_FilterList.rowCount() - 3;

   while ( ( %nextRow = CF_FilterList.getRowTextById( %rowId + 1 ) ) !$= "" )
   {
      CF_FilterList.setRowById( %rowId, %nextRow );
      %rowId++;
   }
   CF_FilterList.removeRowById( %rowId );

   // Get rid of the last filter (now extra):
   $pref::ServerBrowser::Filter[%lastFilter] = "";

   // Select next (or failing that, previous) filter:
   if ( CF_FilterList.getRowTextById( %row ) $= "" )
      %selId = CF_FilterList.getRowId( %row - 1 );
   else
      %selId = CF_FilterList.getRowId( %row );

   CF_FilterList.setSelectedById( %selId );
}

//------------------------------------------------------------------------------
function ChooseFilterDlg::go( %this )
{
   Canvas.popDialog( ChooseFilterDlg );
   GMJ_Browser.runQuery();
}

//------------------------------------------------------------------------------
function CF_FilterList::onSelect( %this, %id, %text )
{
	// Let the user know they can't edit or delete the default filters:
	if ( %id < 2 )
	{
		CF_EditFilterBtn.setActive( false );
		CF_DeleteFilterBtn.setActive( false );
	}
	else
	{
		CF_EditFilterBtn.setActive( true );
		CF_DeleteFilterBtn.setActive( true );
	}
   $pref::ServerBrowser::activeFilter = %id;
}

//------------------------------------------------------------------------------
function FilterEditDlg::setMinPlayers( %this )
{
	%newMin = FilterEditMinPlayers.getValue();
	if ( %newMin < 0 )
	{
		%newMin = 0;
		FilterEditMinPlayers.setValue( %newMin );
	}
	else if ( %newMin > 254 )
	{
		%newMin = 254;
		FilterEditMinPlayers.setValue( %newMin );
	}

	%newMax = FilterEditMaxPlayers.getValue();
	if ( %newMax <= %newMin )
	{
		%newMax = %newMin + 1;
		FilterEditMaxPlayers.setValue( %newMax );
	}
}

//------------------------------------------------------------------------------
function FilterEditDlg::setMaxPlayers( %this )
{
	%newMax = FilterEditMaxPlayers.getValue();
	if ( %newMax < 1 )
	{
		%newMax = 1;
		FilterEditMaxPlayers.setValue( %newMax );
	}
	if ( %newMax > 255 )
	{
		%newMax = 255;
		FilterEditMaxPlayers.setValue( %newMax );
	}

	%newMin = FilterEditMinPlayers.getValue();
	if ( %newMin >= %newMax )
	{
		%newMin = %newMax - 1;
		FilterEditMinPlayers.setValue( %newMin );
	}
}

//------------------------------------------------------------------------------
function FilterEditUsePingTgl::onAction( %this )
{
   FilterEditMaxPing.setVisible( %this.getValue() );
}

//------------------------------------------------------------------------------
function FilterEditDlg::setMaxPing( %this )
{
   %newMax = FilterEditMaxPing.getValue();
   if ( %newMax < 10 )
   {
      %newMax = 10;
      FilterEditMaxPing.setValue( %newMax );
   }
}

//------------------------------------------------------------------------------
function clearGameTypes()
{
   %text = FilterEditGameType.getText();
   FilterEditGameType.clear();
   FilterEditGameType.add( "Any", 0 );
   FilterEditGameType.setText( %text );
}

//------------------------------------------------------------------------------
function addGameType( %type )
{
   FilterEditGameType.add( %type, 0 );
}

//------------------------------------------------------------------------------
function clearMissionTypes()
{
   %text = FilterEditMissionType.getText();
   FilterEditMissionType.clear();
   FilterEditMissionType.add( "Any", 0 );
   FilterEditMissionType.setText( %text );

   // Add all the mission types found on this machine:
   for ( %i = 0; %i < $HostTypeCount; %i++ )
      FilterEditMissionType.add( $HostTypeDisplayName[%i], %i );
}

//------------------------------------------------------------------------------
function addMissionType(%type)
{
   if ( %type !$= "" && FilterEditMissionType.findText( %type ) == -1 )
      FilterEditMissionType.add( %type, 0 );
}

//------------------------------------------------------------------------------
// Make sure we still have at least one region selected:
function FilterEditDlg::checkRegionMasks( %this, %lastIndex )
{
   %index = 0;
   while ( isObject( "FilterEditLocMask" @ %index ) )
   {
      if ( ( "FilterEditLocMask" @ %index ).getValue() )   
         return;
      %index++;
   }

   // None are selected, so reselect the last control:
   ( "FilterEditLocMask" @ %lastIndex ).setValue( true );
}
