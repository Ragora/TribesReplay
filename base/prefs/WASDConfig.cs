// Tribes 2 Input Map File
moveMap.delete();
new ActionMap(moveMap);
moveMap.bindCmd(keyboard, "escape", "", "escapeFromGame();");
moveMap.bind(keyboard, "s", movebackward);
moveMap.bind(keyboard, "space", jump);
moveMap.bind(keyboard, "pageup", pageMessageHudUp);
moveMap.bind(keyboard, "pagedown", pageMessageHudDown);
moveMap.bind(keyboard, "w", moveforward);
moveMap.bind(keyboard, "b", placeBeacon);
moveMap.bind(keyboard, "q", nextWeapon);
moveMap.bind(keyboard, "1", useBlaster);
moveMap.bind(keyboard, "2", usePlasma);
moveMap.bind(keyboard, "3", useChaingun);
moveMap.bind(keyboard, "4", useDisc);
moveMap.bind(keyboard, "5", useGrenadeLauncher);
moveMap.bind(keyboard, "6", useSniperRifle);
moveMap.bind(keyboard, "g", throwGrenade);
moveMap.bind(keyboard, "h", useRepairKit);
moveMap.bind(keyboard, "l", useTargetingLaser);
moveMap.bind(keyboard, "ctrl w", throwWeapon);
moveMap.bind(keyboard, "ctrl f", throwFlag);
moveMap.bind(keyboard, "z", setZoomFOV);
moveMap.bind(keyboard, "a", moveleft);
moveMap.bind(keyboard, "numpadenter", toggleInventoryHud);
moveMap.bind(keyboard, "numpad1", selectFavorite1);
moveMap.bind(keyboard, "numpad2", selectFavorite2);
moveMap.bind(keyboard, "numpad3", selectFavorite3);
moveMap.bind(keyboard, "numpad4", selectFavorite4);
moveMap.bind(keyboard, "numpad5", selectFavorite5);
moveMap.bind(keyboard, "numpad6", selectFavorite6);
moveMap.bind(keyboard, "numpad7", selectFavorite7);
moveMap.bind(keyboard, "numpad8", selectFavorite8);
moveMap.bind(keyboard, "numpad9", selectFavorite9);
moveMap.bind(keyboard, "numpad0", selectFavorite10);
moveMap.bind(keyboard, "alt numpad1", selectFavorite11);
moveMap.bind(keyboard, "alt numpad2", selectFavorite12);
moveMap.bind(keyboard, "alt numpad3", selectFavorite13);
moveMap.bind(keyboard, "alt numpad4", selectFavorite14);
moveMap.bind(keyboard, "alt numpad5", selectFavorite15);
moveMap.bind(keyboard, "alt numpad6", selectFavorite16);
moveMap.bind(keyboard, "alt numpad7", selectFavorite17);
moveMap.bind(keyboard, "alt numpad8", selectFavorite18);
moveMap.bind(keyboard, "alt numpad9", selectFavorite19);
moveMap.bind(keyboard, "alt numpad0", selectFavorite20);
moveMap.bind(keyboard, "ctrl numpad0", quickPackEnergyPack);
moveMap.bind(keyboard, "ctrl numpad1", quickPackRepairPack);
moveMap.bind(keyboard, "ctrl numpad2", quickPackShieldPack);
moveMap.bind(keyboard, "ctrl numpad3", quickPackCloakPack);
moveMap.bind(keyboard, "ctrl numpad4", quickPackJammerPack);
moveMap.bind(keyboard, "ctrl numpad5", quickPackAmmoPack);
moveMap.bind(keyboard, "ctrl numpad6", quickPackSatchelCharge);
moveMap.bind(keyboard, "ctrl numpad7", quickPackDeployableStation);
moveMap.bind(keyboard, "ctrl numpad8", quickPackIndoorTurret);
moveMap.bind(keyboard, "ctrl numpad9", quickPackOutdoorTurret);
moveMap.bind(keyboard, "ctrl numpaddivide", quickPackMotionSensor);
moveMap.bind(keyboard, "ctrl *", quickPackPulse);
moveMap.bind(keyboard, "y", TeamMessageHud);
moveMap.bind(keyboard, "v", activateChatMenuHud);
moveMap.bind(keyboard, "c", toggleCommanderMap);
moveMap.bind(keyboard, "ctrl k", suicide);
moveMap.bind(keyboard, "f1", toggleHelpGui);
moveMap.bind(keyboard, "f2", toggleScoreScreen);
moveMap.bind(keyboard, "f6", toggleHudWaypoints);
moveMap.bind(keyboard, "f7", toggleHudMarkers);
moveMap.bind(keyboard, "f8", toggleHudTargets);
moveMap.bind(keyboard, "f9", toggleHudCommands);
moveMap.bind(keyboard, "n", toggleTaskListDlg);
moveMap.bind(keyboard, "shift c", fnTaskCompleted);
moveMap.bind(keyboard, "shift x", fnResetTaskList);
moveMap.bind(keyboard, "insert", voteYes);
moveMap.bind(keyboard, "delete", voteNo);
moveMap.bind(keyboard, "d", moveright);
moveMap.bind(keyboard, "right", turnRight);
moveMap.bind(keyboard, "left", turnLeft);
moveMap.bind(keyboard, "up", panUp);
moveMap.bind(keyboard, "down", panDown);
moveMap.bind(keyboard, "e", toggleZoom);
moveMap.bind(keyboard, "7", useELFGun);
moveMap.bind(keyboard, "8", useMortar);
moveMap.bind(keyboard, "9", useMissileLauncher);
moveMap.bind(keyboard, "0", useShockLance);
moveMap.bind(keyboard, "shift q", prevWeapon);
moveMap.bind(keyboard, "m", placeMine);
moveMap.bind(keyboard, "p", useBackPack);
moveMap.bind(keyboard, "t", ToggleMessageHud);
moveMap.bind(keyboard, "r", toggleFirstPerson);
moveMap.bind(keyboard, "f", toggleFreeLook);
moveMap.bind(keyboard, "ctrl p", throwPack);
moveMap.bind(keyboard, "return", fnAcceptTask);
moveMap.bind(keyboard, "backspace", fnDeclineTask);
moveMap.bind(keyboard, "x", voiceCapture);
moveMap.bind(keyboard, "u", resizeChatHud);
moveMap.bind(mouse0, "xaxis", yaw);
moveMap.bind(mouse0, "yaxis", pitch);
moveMap.bind(mouse0, "button0", mouseFire);
moveMap.bind(mouse0, "button1", mouseJet);
moveMap.bind(mouse0, "zaxis", cycleWeaponAxis);
observerMap.delete();
new ActionMap(observerMap);
observerMap.bind(keyboard, "space", jump);
observerMap.bind(keyboard, "e", moveup);
observerMap.bind(keyboard, "c", movedown);
observerMap.bind(mouse0, "button1", mouseJet);
if (!isDemo())
   GlobalActionMap.bind(keyboard, "grave", toggleConsole);
