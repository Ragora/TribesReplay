//------------------------------------------------------------------------------
//
// commonDialogs.cs
//
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// MessageBox OK dialog:
//------------------------------------------------------------------------------
function MessageBoxOK( %title, %message, %callback )
{
	MBOKFrame.setTitle( %title );
   MBOKText.setText( "<just:center>" @ %message );
   MessageBoxOKDlg.callback = %callback;
   Canvas.pushDialog( MessageBoxOKDlg );
}

//------------------------------------------------------------------------------
function MessageBoxOKDlg::onWake( %this )
{
   %this.mouseOn = Canvas.isCursorOn();
   if ( !%this.mouseOn )
      CursorOn();
}

//------------------------------------------------------------------------------
function MessageBoxOKDlg::onSleep( %this )
{
   if ( !%this.mouseOn )
      CursorOff();
   %this.callback = "";
}

//------------------------------------------------------------------------------
// MessageBox OK/Cancel dialog:
//------------------------------------------------------------------------------
function MessageBoxOKCancel( %title, %message, %callback, %cancelCallback )
{
	MBOKCancelFrame.setTitle( %title );
   MBOKCancelText.setText( "<just:center>" @ %message );
	MessageBoxOKCancelDlg.callback = %callback;
	MessageBoxOKCancelDlg.cancelCallback = %cancelCallback;
   Canvas.pushDialog( MessageBoxOKCancelDlg );
}

//------------------------------------------------------------------------------
function MessageBoxOKCancelDlg::onWake( %this )
{
   %this.mouseOn = Canvas.isCursorOn();
   if ( !%this.mouseOn )
      CursorOn();
}

//------------------------------------------------------------------------------
function MessageBoxOKCancelDlg::onSleep( %this )
{
   if ( !%this.mouseOn )
      CursorOff();
   %this.callback = "";
}

//------------------------------------------------------------------------------
// MessageBox Yes/No dialog:
//------------------------------------------------------------------------------
function MessageBoxYesNo( %title, %message, %yesCallback, %noCallback )
{
	MBYesNoFrame.setTitle( %title );
   MBYesNoText.setText( "<just:center>" @ %message );
	MessageBoxYesNoDlg.yesCallBack = %yesCallback;
	MessageBoxYesNoDlg.noCallback = %noCallBack;
   Canvas.pushDialog( MessageBoxYesNoDlg );
}

//------------------------------------------------------------------------------
function MessageBoxYesNoDlg::onWake( %this )
{
   %this.mouseOn = Canvas.isCursorOn();
   if ( !%this.mouseOn )
      CursorOn();
}

//------------------------------------------------------------------------------
function MessageBoxYesNoDlg::onSleep( %this )
{
   if ( !%this.mouseOn )
      CursorOff();
   %this.yesCallback = "";
   %this.noCallback = "";
}

//------------------------------------------------------------------------------
// Message popup dialog:
//------------------------------------------------------------------------------
function MessagePopup( %title, %message, %delay )
{
   // Currently two lines max.
   MessagePopFrame.setTitle( %title );
   MessagePopText.setText( "<just:center>" @ %message );
   Canvas.pushDialog( MessagePopupDlg );
   if ( %delay !$= "" )
      schedule( %delay, 0, CloseMessagePopup );
}

//------------------------------------------------------------------------------
function CloseMessagePopup()
{
   Canvas.popDialog( MessagePopupDlg );
}

//------------------------------------------------------------------------------
// Pick Team dialog:
//------------------------------------------------------------------------------
function PickTeamDlg::onWake( %this )
{
   %this.mouseOn = Canvas.isCursorOn();
   if ( !%this.mouseOn )
      CursorOn();
}

//------------------------------------------------------------------------------
function PickTeamDlg::onSleep( %this )
{
   if ( !%this.mouseOn )
      CursorOff();
}

//------------------------------------------------------------------------------
// ex: ShellGetLoadFilename( "stuff\*.*", isLoadable, loadStuff );
//     -- only adds files that pass isLoadable
//     -- calls 'loadStuff(%filename)' on dblclick or ok
//------------------------------------------------------------------------------
function ShellGetLoadFilename( %title, %fileSpec, %validate, %callback )
{
   $loadFileCommand = %callback @ "( getField( LOAD_FileList.getValue(), 0 ) );";
   if ( %title $= "" )
      LOAD_Title.setTitle( "LOAD FILE" );
   else
      LOAD_Title.setTitle( %title );
   LOAD_LoadBtn.setActive( false );
   Canvas.pushDialog( ShellLoadFileDlg );
   fillLoadSaveList( LOAD_FileList, %fileSpec, %validate, false );
}

//------------------------------------------------------------------------------
function fillLoadSaveList( %ctrl, %fileSpec, %validate, %isSave )
{
   %ctrl.clear();
   %id = 0;
   for ( %file = findFirstFile( %fileSpec ); %file !$= ""; %file = findNextFile( %fileSpec ) )
   {
      if ( %validate $= "" || call( %validate, %file ) )
      {
         %ctrl.addRow( %id, fileBase( %file ) TAB %file );
         if ( %isSave )
         {
            if ( !isWriteableFileName( "base/" @ %file ) )
               %ctrl.setRowActive( %id, false );
         }
         %id++;
      }
   }
}

//------------------------------------------------------------------------------
function LOAD_FileList::onSelect( %this, %id, %text )
{
   LOAD_LoadBtn.setActive( true );
}

//------------------------------------------------------------------------------
// ex: ShellGetSaveFilename( "stuff\*.*", isLoadable, saveStuff, currentName );
//     -- only adds files to list that pass isLoadable
//     -- calls 'saveStuff(%filename)' on dblclick or ok
//------------------------------------------------------------------------------
function ShellGetSaveFilename( %title, %fileSpec, %validate, %callback, %current )
{
   SAVE_FileName.setValue( %current );
   $saveFileCommand = "if ( SAVE_FileName.getValue() !$= \"\" ) " @ %callback @ "( SAVE_FileName.getValue() );";
   if ( %title $= "" )
      SAVE_Title.setTitle( "SAVE FILE" );
   else
      SAVE_Title.setTitle( %title );

   // Right now this validation stuff is worthless...
   //SAVE_SaveBtn.setActive( isWriteableFileName( "base/" @ %current @ $loadSaveExt ) );
   Canvas.pushDialog( ShellSaveFileDlg );
   fillLoadSaveList( SAVE_FileList, %fileSpec, %validate, true );
}

//------------------------------------------------------------------------------
function SAVE_FileList::onSelect( %this, %id, %text )
{
   if ( %this.isRowActive( %id ) )
      SAVE_FileName.setValue( getField( %this.getValue(), 0 ) );
}

//------------------------------------------------------------------------------
function SAVE_FileList::onDoubleClick( %this )
{
   %id = %this.getSelectedId();
   if ( %this.isRowActive( %id ) )
   {
      eval( $saveFileCommand ); 
      Canvas.popDialog( ShellSaveFileDlg );
   }
}

//------------------------------------------------------------------------------
function SAVE_FileName::checkValid( %this )
{
   // Right now this validation stuff is worthless...
   //SAVE_SaveBtn.setActive( isWriteableFileName( "base/" @ %this.getValue() @ $loadSaveExt ) );
}
