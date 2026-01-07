using System;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Media.Control;

namespace InfoSkull.builtin;

public class WindowsAPI
{
	public static GlobalSystemMediaTransportControlsSessionManager sessionManager;
	public static GlobalSystemMediaTransportControlsSession currentSession;
	public static GlobalSystemMediaTransportControlsSessionMediaProperties currentMediaProperties;

	public static void init()
	{
		sessionManager =
			GlobalSystemMediaTransportControlsSessionManager.RequestAsync().GetAwaiter().GetResult();

		sessionManager.CurrentSessionChanged += onCurrentSessionChanged;
	}

	public async static void onCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
	{
		if (currentSession != null)
		{
			currentSession.MediaPropertiesChanged -= onMediaPropertiesChanged;
		}

		currentSession = sender.GetCurrentSession();

		if (currentSession != null)
		{
			currentSession.MediaPropertiesChanged += onMediaPropertiesChanged;
		}
	}

	public async static void onMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
	{
		try
		{
			var mediaProperties = await sender.TryGetMediaPropertiesAsync();
			currentMediaProperties = mediaProperties;
		}
		catch (Exception) { }
	}
}
