//--------------------------------------------------------------------------
// Pathed objects:
//
//  accept the following commands:
//    goForward()
//    goBackward()
//    goToWaypoint(waypoint)
//    doCollideReverse()
//
//  respond to the following queries
//    getNumWaypoints()
// 
//  generate the following events
//    onTriggerTick
//    onStop
//    onCollide (For non-displacing objects only)
// 
//--------------------------------------------------------------------------

datablock PathedObjectData(defaultPathFollow)
{
   displaceObjects = "false";
   reversalPauseMS = 1000;
};

datablock PathedObjectData(displaceOnCollision)
{
   displaceObjects = "true";
   reversalPauseMS = 1000;
};

function PathedObject::onTrigger(%this, %triggerId, %on)
{
   // Default behavior for a door:
   //  if triggered:   go to open state (last waypoint)
   //  if untriggered: go to closed state (first waypoint)

   if (%on $= "true") {
      %this.triggerCount++;
   } else {
      if (%this.triggerCount > 0)
         %this.triggerCount--;
   }

   if (%this.triggerCount > 0)
      %this.goForward();
   else
      %this.goBackward();
}

// Commented out functions are implemented in code for efficiency, placed here for
//  parameter description...
//
//function PathedObject::onTriggerTick(%this, %triggerId)
//{
//   // Do nothing
//}
//
//function PathedObject::onStop(%this, %position)
//{
//   // Do nothing by default.
//   //  Possible values for %position:
//   //    Start
//   //    End
//   //    # (Waypoint number)
//}
//
//function PathedObject::onCollide(%this, %position)
//{
//   %myBlock = %this.getDataBlock();
//
//   if (%myBlock.displaceObjects) {
//      error(%this @ ": displacing pathed objects should never call onCollide");
//      return;
//   }
//
//   %this.doCollideReverse();
//}\
