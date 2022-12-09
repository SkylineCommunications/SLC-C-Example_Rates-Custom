using System;
using System.Collections.Generic;
using System.Linq;

using Skyline.DataMiner.Scripting;
using Skyline.DataMiner.Utils.Rates.Common;

/// <summary>
/// DataMiner QAction Class: Fill Counter.
/// </summary>
public static class QAction
{
	private static TimeSpan minDelta = new TimeSpan(0, 0, 5);
	private static TimeSpan maxDelta = new TimeSpan(0, 10, 0);

	/// <summary>
	/// The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void Run(SLProtocol protocol)
	{
		try
		{
			// Get new counter from data source
			UInt32 counter = GetNewCounterFromDataSource(protocol);

			// Get buffered data and make a helper instance from it
			string bufferedData = Convert.ToString(protocol.GetParameter(Parameter.counterrateondatesdata));
			Rate32OnDateTime rate32OnDatesHelper = Rate32OnDateTime.FromJsonString(bufferedData, minDelta, maxDelta);

			// Calculate rate
			double rate = rate32OnDatesHelper.Calculate(counter, DateTime.UtcNow);

			// Save results and buffered data
			Dictionary<int, object> paramsToSet = new Dictionary<int, object>
			{
				{ Parameter.counter, counter },
				{ Parameter.counterrateondates, rate },
				{ Parameter.counterrateondatesdata, rate32OnDatesHelper.ToJsonString() },
			};

			protocol.SetParameters(paramsToSet.Keys.ToArray(), paramsToSet.Values.ToArray());
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|" + protocol.GetTriggerParameter() + "|Run|Exception thrown:" + Environment.NewLine + ex, LogType.Error, LogLevel.NoLogging);
		}
	}

	/// <summary>
	/// Simulates dynamic counters from data source.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	/// <returns>The new value for the counter.</returns>
	public static UInt32 GetNewCounterFromDataSource(SLProtocol protocol)
	{
		UInt32 previousCounter = SafeConvert.ToUInt32(Convert.ToDouble(protocol.GetParameter(Parameter.counter)));
		Random random = new Random();

		return previousCounter + (UInt32)random.Next(1000);
	}
}