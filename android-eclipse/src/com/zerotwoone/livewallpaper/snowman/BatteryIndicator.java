package com.zerotwoone.livewallpaper.snowman;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.os.BatteryManager;

public class BatteryIndicator extends BroadcastReceiver {

	//public static final String KEY_LOW_BATTERY = "lowbatteryKey";
	public static boolean LOW_BATTERY = false;
	
	
	@Override
	public void onReceive(Context arg0, Intent arg1) {
		
		
		int level = arg1.getIntExtra(BatteryManager.EXTRA_LEVEL, -1);
		int scale = arg1.getIntExtra(BatteryManager.EXTRA_SCALE, -1);
		
		int status = arg1.getIntExtra(BatteryManager.EXTRA_STATUS, -1);
		
		float per = (level/(float)scale) * 100f;
		boolean isCharging = status == BatteryManager.BATTERY_STATUS_CHARGING ||
							 status == BatteryManager.BATTERY_STATUS_FULL;
		
		if (isCharging || per > 25){
		/*	PreferenceManager.getDefaultSharedPreferences(arg0)
				.edit().putBoolean(KEY_LOW_BATTERY, false).commit();
			*/
			LOW_BATTERY = false;
		}
		
		if (!isCharging && per <= 25){
			/*PreferenceManager.getDefaultSharedPreferences(arg0)
				.edit().putBoolean(KEY_LOW_BATTERY, true).commit();*/
			LOW_BATTERY = true;
		}
	}

}
