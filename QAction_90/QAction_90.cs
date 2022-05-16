using System;
using System.Collections.Generic;
using System.Linq;

using Skyline.DataMiner.Library.Common.Rates;
using Skyline.DataMiner.Library.Common.SafeConverters;
using Skyline.DataMiner.Scripting;

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
			UInt32 counter = SimulateCounter(protocol);

			string bufferedData = Convert.ToString(protocol.GetParameter(Parameter.counterrateondatesdata));
			Rate32OnDateTime rate32OnDatesHelper = Rate32OnDateTime.FromJsonString(bufferedData, minDelta, maxDelta);
			double rate = rate32OnDatesHelper.Calculate(counter, DateTime.Now);

			Dictionary<int, object> paramsToSet = new Dictionary<int, object>
			{
				{Parameter.counter, counter },
				{Parameter.counterrateondates, rate },
				{Parameter.counterrateondatesdata, rate32OnDatesHelper.ToJsonString() },
			};

			protocol.SetParameters(paramsToSet.Keys.ToArray(), paramsToSet.Values.ToArray());
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|" + protocol.GetTriggerParameter() + "|Run|Exception thrown:" + Environment.NewLine + ex, LogType.Error, LogLevel.NoLogging);
		}
	}

	public static UInt32 SimulateCounter(SLProtocol protocol)
	{
		UInt32 previousCounter = SafeConvert.ToUInt32(Convert.ToDouble(protocol.GetParameter(Parameter.counter)));
		Random random = new Random();

		return previousCounter + (UInt32)random.Next();
	}
}