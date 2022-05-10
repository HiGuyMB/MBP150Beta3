function optionsExtrasDlg::resetControls() {
   // Reset values to their defaults. Small list, I know, but hopefully there will be more options to come :)
   $pref::showFPSCounter = 0;
   $pref::Player::defaultFov = 90;
   OExt_FOVSlider.setValue(90);
   OExt_ShowFPSButton.setValue(0);
	OExt_ShowOOBMessages.setValue(0);
}