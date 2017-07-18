//--------------------------------------------------------------------------
//
// joystickBind.cs
//
//--------------------------------------------------------------------------

// Joystick functions:
function joystickMoveX(%val)
{
   $mvLeftAction = ( %val < 0.0 );
   $mvRightAction = ( %val > 0.0 );
}

function joystickMoveY(%val)
{
   $mvForwardAction = ( %val < 0.0 );
   $mvBackwardAction = ( %val > 0.0 );
}

function joystickYaw(%val)
{
   // Let the mapping system do all the scaling/inversion...
   $mvYaw += %val;
}

function joystickPitch(%val)
{
   $mvPitch += %val;
}
