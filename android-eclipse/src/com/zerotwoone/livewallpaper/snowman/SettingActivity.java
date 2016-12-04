package com.zerotwoone.livewallpaper.snowman;

import java.io.FileNotFoundException;
import java.io.InputStream;

import android.app.Activity;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.Bitmap.Config;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Rect;
import android.net.Uri;
import android.os.Bundle;
import android.preference.Preference;
import android.preference.Preference.OnPreferenceClickListener;
import android.preference.PreferenceActivity;
import android.widget.Toast;


public class SettingActivity extends PreferenceActivity {
	public static int BitmapHeight = 150;
	public static int BitmapWidth = 150;
	@SuppressWarnings("deprecation")
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
				
		addPreferencesFromResource(R.xml.settings);
		findPreference(getString(R.string.key_photoframe)).setOnPreferenceClickListener(new OnPreferenceClickListener() {
			
			@Override
			public boolean onPreferenceClick(Preference preference) {
				Intent intent = new Intent();
				intent.setType("image/*");
				intent.setAction(Intent.ACTION_GET_CONTENT);
				startActivityForResult(Intent.createChooser(intent, "Select Gallery"), 1);
				
				return true;
			}
		});
		
		try{
		findPreference(getString(R.string.key_rate)).setOnPreferenceClickListener(new OnPreferenceClickListener() {
			
			@Override
			public boolean onPreferenceClick(Preference preference) {
				
				final Uri uri = Uri.parse("https://play.google.com/store/apps/details?id=" + getApplicationContext().getPackageName());
				final Intent rateAppIntent = new Intent(Intent.ACTION_VIEW, uri);

				if (getPackageManager().queryIntentActivities(rateAppIntent, 0).size() > 0)
				{
				    startActivity(rateAppIntent);
				    return true;
				}
				else
				{
					Toast.makeText(getApplicationContext(), "Android market not found", Toast.LENGTH_SHORT).show();
					return false;
				}
			}
		});
				
		/*findPreference(getString(R.string.key_getmore)).setOnPreferenceClickListener(new OnPreferenceClickListener() {
			
			@Override
			public boolean onPreferenceClick(Preference preference) {
				
				final Uri uri = Uri.parse("http://tik.ee/livewall021labs");
				final Intent rateAppIntent = new Intent(Intent.ACTION_VIEW, uri);

				if (getPackageManager().queryIntentActivities(rateAppIntent, 0).size() > 0)
				{
				    startActivity(rateAppIntent);
				    return true;
				}
				else
				{
					Toast.makeText(getApplicationContext(), "Android market not found", Toast.LENGTH_SHORT).show();
					return false;
				}
			}
		});*/
		}catch (NullPointerException e){
			
		}
	}
	
	@SuppressWarnings("deprecation")
	@Override
	public void onActivityResult(int requestCode, int resultCode, Intent data) {
		super.onActivityResult(requestCode, resultCode, data);
		if (resultCode == Activity.RESULT_OK && requestCode == 1){
			Uri uri = data.getData();
			
			try {
				InputStream in = getContentResolver().openInputStream(uri);
				Bitmap bm = BitmapFactory.decodeStream(in);
				Bitmap snd = Bitmap.createBitmap(BitmapWidth, BitmapHeight, Config.ARGB_8888);
				
				Canvas c = new Canvas(snd);
				c.drawBitmap(bm, null, new Rect(0, 0, BitmapWidth, BitmapHeight), null);
				
				String toSend = ParseBitmap(snd);
				
				getPreferenceManager().getSharedPreferences().edit().putString(getString(R.string.key_photoframe), toSend).commit();
			} catch (FileNotFoundException e) {
				e.printStackTrace();
			}
		}
	}
	
	private String ParseBitmap(Bitmap bm){
		
		int w = bm.getWidth();
		int h = bm.getHeight();
		StringBuilder retBuild = new StringBuilder();
		retBuild.append(w);
		retBuild.append('?');
		retBuild.append(h);
		retBuild.append('?');
		int[] pixels = new int[w * h];
		bm.getPixels(pixels, 0, w, 0, 0, w, h);
	
		for (int i = h - 1; i >= 0;i--){
			for (int j = 0; j < w;j++){
				if (i == 0 && j == w - 1)
					retBuild.append(pixels[i * w + j] & 0x01FFFFFF);
				else{
					retBuild.append(pixels[i * w + j] & 0x01FFFFFF);
					retBuild.append(':');
				}
			}
		}
	
		return retBuild.toString();
	}
}
