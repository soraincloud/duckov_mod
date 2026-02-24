namespace UnityEngine.Analytics;

public interface UGSAnalyticsInternalTools
{
	static void SetPrivacyStatus(bool status)
	{
		AnalyticsCommon.ugsAnalyticsEnabled = status;
	}
}
