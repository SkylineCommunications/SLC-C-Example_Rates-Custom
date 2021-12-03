using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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
	public static void Run(SLProtocolExt protocol)
	{
		try
		{
			if (protocol.streams.Keys.Length == 0)
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
	private static void CreateRows(SLProtocolExt protocol)
	{
		var rows = new List<StreamsQActionRow>
		{
			new StreamsQActionRow
			{
				Streamsindex_1001 = 1,
				Streamsdrescription_1002 = "Constantly growing counter",
				Streamsoctetscounter_1003 = 0,
				Streamsbitrate_1004 = 0,
				Streamsbitratedata_1005 = String.Empty
			},
			new StreamsQActionRow
			{
				Streamsindex_1001 = 2,
				Streamsdrescription_1002 = "Randomly ever growing counter",
				Streamsoctetscounter_1003 = 0,
				Streamsbitrate_1004 = 0,
				Streamsbitratedata_1005 = String.Empty
			},
			new StreamsQActionRow
			{
				Streamsindex_1001 = 3,
				Streamsdrescription_1002 = "Randomly growing counters with regular reset",
				Streamsoctetscounter_1003 = 0,
				Streamsbitrate_1004 = 0,
				Streamsbitratedata_1005 = String.Empty
			},
			new StreamsQActionRow
			{
				Streamsindex_1001 = 4,
				Streamsdrescription_1002 = "Completely random counters",
				Streamsoctetscounter_1003 = 0,
				Streamsbitrate_1004 = 0,
				Streamsbitratedata_1005 = String.Empty
			}
		};

		protocol.streams.FillArray(rows.ToArray());
	}
}
