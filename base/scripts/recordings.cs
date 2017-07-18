function RecordingsDlg::onWake(%gui)
{
   RecordingsDlgList.clear();
   %search = "recordings/*.rec";
   %ct = 0;
   for(%file = findFirstFile(%search); %file !$= ""; %file = findNextFile(%search))
   {
      %fileName = fileBase(%file);
      RecordingsDlgList.addRow(%ct++, %fileName);
   }
   RecordingsDlgList.sort(0);

	if ( RecordingsDlgList.rowCount() == 0 )
	{
		PR_StartDemoBtn.setActive( false );
		PR_CancelBtn.makeFirstResponder( 1 );
	}
	else
	{
		RecordingsDlgList.setSelectedById( 1 );
		PR_StartDemoBtn.setActive( true );
		PR_StartDemoBtn.makeFirstResponder( 1 );
	}
}

function StartSelectedDemo()
{
   %sel = RecordingsDlgList.getSelectedId();
   %file = RecordingsDlgList.getRowTextById(%sel);
   playDemo("recordings/" @ %file @ ".rec");
   Canvas.setContent(PlayGui);
}

function LoopDemos()
{
   $demoCount = 0;

   for($demoFile[$demoCount] = findFirstFile("recordings/*.rec"); $demoFile[$demoCount] !$= ""; $demoFile[$demoCount++] = findNextFile("recordings/*.rec") )
   {
   }
   $currentDemo = $demoCount - 1;
   if($demoCount == 0)
      return;

   demoPlaybackComplete();
}

function beginDemoRecord()
{
   stopRecord();
   for(%i = 0; %i < 1000; %i++)
   {
      %num = %i;
      if(%num < 10)
         %num = "0" @ %num;
      if(%num < 100)
         %num = "0" @ %num;
      %file = "recordings/demo" @ %num @ ".rec";
      if(!isfile(%file))
         break;
   }
   if(%i == 1000)
      return;
   echo("Recording demo: "@ %file);
   startRecord(%file);
}

function demoPlaybackComplete()
{
   Canvas.setContent("LaunchGui");
   Canvas.pushDialog(RecordingsDlg);
   purgeResources();
}
