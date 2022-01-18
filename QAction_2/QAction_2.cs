using System;
using System.Collections.Generic;

using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class: After Startup.
/// </summary>
public static class QAction
{
	/// <summary>
	/// The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void Run(SLProtocol protocol)
	{
		try
		{
			object[] streamKeys = (object[])((object[])protocol.NotifyProtocol(321, Parameter.Streams.tablePid, new uint[] { 0 }))[0];
			if (streamKeys == null || streamKeys.Length == 0)
			{
				CreateRows(protocol);
			}
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|" + protocol.GetTriggerParameter() + "|Run|Exception thrown:" + Environment.NewLine + ex, LogType.Error, LogLevel.NoLogging);
		}
	}

	/// <summary>
	/// Create the Rows on Table.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	private static void CreateRows(SLProtocol protocol)
	{
		var rows = new List<object[]>
		{
			new StreamsQActionRow
			{
				Streamsindex_1001 = 1,
				Streamsdrescription_1002 = "Constantly growing counter",
				Streamsoctetscounter_1003 = 0,
				Streamsbitrate_1004 = 0,
				Streamsbitratedata_1005 = String.Empty
			}.ToObjectArray(),
			new StreamsQActionRow
			{
				Streamsindex_1001 = 2,
				Streamsdrescription_1002 = "Randomly ever growing counter",
				Streamsoctetscounter_1003 = 0,
				Streamsbitrate_1004 = 0,
				Streamsbitratedata_1005 = String.Empty
			}.ToObjectArray(),
			new StreamsQActionRow
			{
				Streamsindex_1001 = 3,
				Streamsdrescription_1002 = "Randomly growing counters with regular reset",
				Streamsoctetscounter_1003 = 0,
				Streamsbitrate_1004 = 0,
				Streamsbitratedata_1005 = String.Empty
			}.ToObjectArray(),
			new StreamsQActionRow
			{
				Streamsindex_1001 = 4,
				Streamsdrescription_1002 = "Completely random counters",
				Streamsoctetscounter_1003 = 0,
				Streamsbitrate_1004 = 0,
				Streamsbitratedata_1005 = String.Empty
			}.ToObjectArray(),
		};

		protocol.FillArray(Parameter.Streams.tablePid, rows, NotifyProtocol.SaveOption.Full);
	}
}