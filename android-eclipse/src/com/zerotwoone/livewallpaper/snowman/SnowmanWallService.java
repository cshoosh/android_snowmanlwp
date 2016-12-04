package com.zerotwoone.livewallpaper.snowman;

import java.util.Calendar;
import java.util.Locale;

import android.content.Context;
import android.content.SharedPreferences;
import android.content.SharedPreferences.OnSharedPreferenceChangeListener;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Rect;
import android.graphics.drawable.Drawable;
import android.opengl.GLSurfaceView;
import android.os.Handler;
import android.preference.PreferenceManager;
import android.service.wallpaper.WallpaperService;
import android.view.GestureDetector;
import android.view.GestureDetector.OnGestureListener;
import android.view.MotionEvent;
import android.view.SurfaceHolder;

import com.unity3d.player.UnityPlayer;

/***
 * 
 * @see http 
 *      ://developer.android.com/reference/android/service/wallpaper/package-
 *      summary.html
 */
public class SnowmanWallService extends WallpaperService {
	private CustomPlayer player;
	private int glesVersion = 2;

	/**
	 * Called by the system when the service is first created. Do not call this
	 * method directly.
	 */
	public void onCreate() {

		super.onCreate();
		player = CustomPlayer.getInstance(this);
	}

	/**
	 * Called by the system to notify a Service that it is no longer used and is
	 * being removed.
	 */
	public void onDestroy() {

		super.onDestroy();

		if (player != null)
			player.quit();

	}

	class WallpaperEngine extends Engine implements
			OnSharedPreferenceChangeListener, OnGestureListener {

		private Handler mHandler;
		private boolean isInverse, isNewYear;
		private GestureDetector mGesture;
		
		private WallSurface mSurfaceView;
		
		public void onVisibilityChanged(boolean visible) {

			super.onVisibilityChanged(visible);
			changePlayerState(visible);
		}

		@Override
		public void onCreate(SurfaceHolder surfaceHolder) {
			super.onCreate(surfaceHolder);
			mHandler = new Handler();
			mGesture = new GestureDetector(getApplicationContext(), this);
			
			PreferenceManager.getDefaultSharedPreferences(
					getApplicationContext())
					.registerOnSharedPreferenceChangeListener(this);
			
			if (!isPreview() && player != null){
				mSurfaceView = new WallSurface(SnowmanWallService.this);
				mSurfaceView.setRenderer(player);
				
				mHandler.postDelayed(new Runnable() {

					@Override
					public void run() {

						if (player != null) {
							player.setWaitTimeOverCheck(true);
							initPreferences(PreferenceManager
									.getDefaultSharedPreferences(getApplicationContext()));
						}
					}
				}, 5000L);
			}
		}
		
		@Override
		public void onSurfaceCreated(SurfaceHolder holder) {
			super.onSurfaceCreated(holder);
			
			if (isPreview())
				drawBitmap();
		}
		@Override
		public void onSurfaceDestroyed(SurfaceHolder arg0) {
			super.onSurfaceDestroyed(arg0);

			PreferenceManager.getDefaultSharedPreferences(
					getApplicationContext())
					.unregisterOnSharedPreferenceChangeListener(this);
		}

		@Override
		public void onOffsetsChanged(float xOffset, float yOffset,
				float xOffsetStep, float yOffsetStep, int xPixelOffset,
				int yPixelOffset) {

			super.onOffsetsChanged(xOffset, yOffset, xOffsetStep, yOffsetStep,
					xPixelOffset, yPixelOffset);

			float minValue = -1.08f;
			float maxValue = 1.821f;

			float offset = Math.abs(minValue) + Math.abs(maxValue);
			if (isInverse)
				offset *= xOffset;
			else
				offset *= 1 - xOffset;
			offset += minValue;
			
			if (player != null)
			UnityPlayer.UnitySendMessage("Main Camera", "SetCamOffsetFactor",
					"" + offset);
		}

		@Override
		public void onTouchEvent(MotionEvent event) {
			super.onTouchEvent(event);
			mGesture.onTouchEvent(event);
		}

		@Override
		public void onSharedPreferenceChanged(
				SharedPreferences sharedPreferences, String key) {
			if (key == getString(R.string.key_inverse)) {
				isInverse = sharedPreferences.getBoolean(key, false);
			} else if (key == getString(R.string.key_isnewyear)) {
				isNewYear = sharedPreferences.getBoolean(key, false);
			} else if (key == getString(R.string.key_snowfall)) {
				boolean snow = sharedPreferences.getBoolean(key, true);
				if (player != null)
					UnityPlayer.UnitySendMessage("Light Snow", "FallSnow", ""
							+ snow);
			} else if (key == getString(R.string.key_photoframe)) {
				String photo = sharedPreferences.getString(key, "default");
				if (player != null)
					UnityPlayer.UnitySendMessage("photo frame", "ChangePhoto",
							photo);

			} else if (key == getString(R.string.key_lowbattery)) {
				if (player != null)
					player.setBatteryCheck(sharedPreferences.getBoolean(key,
							false));
			}
		}

		private void sendDate() {
			Calendar now = Calendar.getInstance(Locale.US);
			int date = 0;
			if (!isNewYear) {
				Calendar christmas = Calendar.getInstance(Locale.US);
				christmas.set(now.get(Calendar.YEAR), Calendar.DECEMBER, 25);

				int nowDays = now.get(Calendar.DAY_OF_YEAR);
				int chrDays = christmas.get(Calendar.DAY_OF_YEAR);

				int diff = chrDays - nowDays;

				if (diff > 0)
					date = diff;
				else if (diff < 0) {
					int max = now.getActualMaximum(Calendar.DAY_OF_YEAR);
					date = max + diff;
				}
			} else
				date = now.getActualMaximum(Calendar.DAY_OF_YEAR)
						- now.get(Calendar.DAY_OF_YEAR);

			if (player != null && (date > 0 && date < 1000))
				UnityPlayer.UnitySendMessage("555", "ChangeDate", "" + date);
		}

		private void initPreferences(SharedPreferences sharedPreferences) {
			SharedPreferences pref = PreferenceManager
					.getDefaultSharedPreferences(getApplicationContext());
			if (player != null)
				player.setBatteryCheck(pref.getBoolean(
						getString(R.string.key_lowbattery), false));

			isInverse = pref.getBoolean(getString(R.string.key_inverse), false);
			isNewYear = pref.getBoolean(getString(R.string.key_isnewyear),
					false);

			if (player != null) {
				boolean snow = sharedPreferences.getBoolean(
						getString(R.string.key_snowfall), true);
				UnityPlayer.UnitySendMessage("Light Snow", "FallSnow", ""
						+ snow);

				String photo = sharedPreferences.getString(
						getString(R.string.key_photoframe), "default");
				UnityPlayer.UnitySendMessage("photo frame", "ChangePhoto",
						photo);
			}
		}

		private void changePlayerState(boolean state) {
			if (player != null) {
				if (state) {
					player.resume();
					sendDate();
					
					if (mSurfaceView != null)
						mSurfaceView.onResume();
				} else{
					player.pause();
					
					if (mSurfaceView != null)
						mSurfaceView.onPause();
				}
			}
		}

		@Override
		public boolean onDown(MotionEvent e) {
			return false;
		}

		@Override
		public boolean onFling(MotionEvent e1, MotionEvent e2, float velocityX,
				float velocityY) {
			return false;
		}

		@Override
		public void onLongPress(MotionEvent e) {

		}

		@Override
		public boolean onScroll(MotionEvent e1, MotionEvent e2,
				float distanceX, float distanceY) {
			return false;
		}

		@Override
		public void onShowPress(MotionEvent e) {

		}

		@Override
		public boolean onSingleTapUp(MotionEvent e) {
			if (player != null) {
				UnityPlayer.UnitySendMessage("snowman", "setTouch", ""
						+ (int) e.getX() + ":" + (int) e.getY());
				UnityPlayer.UnitySendMessage("Deer", "setTouch",
						"" + (int) e.getX() + ":" + (int) e.getY());
				UnityPlayer.UnitySendMessage("Deer001", "setTouch", ""
						+ (int) e.getX() + ":" + (int) e.getY());
				return true;
			}
			return false;
		}
		
		private boolean drawBitmap() {
			Canvas c = null;
			try {
				c = getSurfaceHolder().lockCanvas();				
				if (c != null) {
					Bitmap bmp = BitmapFactory
							.decodeResource(getResources(), R.drawable.page);
					Rect boundsframe = getSurfaceHolder().getSurfaceFrame();
					
					Drawable d = getResources().getDrawable(R.drawable.bg_gradient);
					d.setBounds(boundsframe);
					d.draw(c);
					c.drawBitmap(bmp, boundsframe.width()/2f - bmp.getWidth()/2f, 
							boundsframe.height()/2f - bmp.getHeight()/2f, null);
					bmp.recycle();
				}
			} catch (NullPointerException e) {
				e.printStackTrace();
			} finally {
				if (c != null){
					getSurfaceHolder().unlockCanvasAndPost(c);
					return true;
				}
			}
			
			return false;
		}
		
		public class WallSurface extends GLSurfaceView{
			public WallSurface(Context context) {
				super(context);
				setEGLContextClientVersion(glesVersion);
				setPreserveEGLContextOnPause(true);
			}
			
			@Override
			public SurfaceHolder getHolder() {
				return getSurfaceHolder();
			}
		}

	}

	/**
	 * Return a new instance of the wallpaper's engine.
	 */
	@Override
	public Engine onCreateEngine() {
		final Engine myEngine = new WallpaperEngine();
		myEngine.setTouchEventsEnabled(true);
		return myEngine;
	}

	/**
	 * Starts new activity (settings menu).
	 */
	/*
	 * public void StartActivity() {
	 * 
	 * WallpaperInfo localWallpaperInfo = ((WallpaperManager)
	 * getSystemService("wallpaper")) .getWallpaperInfo(); if
	 * (localWallpaperInfo != null) { String str1 =
	 * localWallpaperInfo.getSettingsActivity();
	 * 
	 * if (str1 != null) { String str2 = localWallpaperInfo.getPackageName();
	 * Intent localIntent1 = new Intent(); ComponentName localComponentName =
	 * new ComponentName(str2, str1);
	 * localIntent1.setComponent(localComponentName);
	 * localIntent1.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
	 * startActivity(localIntent1); } }
	 * 
	 * }
	 */
}
