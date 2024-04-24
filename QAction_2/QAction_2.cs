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
			InitializeStreamRows(protocol);
		}
		catch (Exception ex)
		{
			protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
		}
	}

	/// <summary>
	/// Create the Rows on Table.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	private static void InitializeStreamRows(SLProtocol protocol)
	{
		var rows = new List<object[]>
		{
			new StreamsQActionRow
			{
				Streamsindex = 1,
				Streamsdrescription = "Constantly growing counter",
				Streamsoctetscounter = 0,
				Streamsbitrateondates = -1,
				Streamsbitrateondatesdata = String.Empty,
				Streamsbitrateontimes = -1,
				Streamsbitrateontimesdata = String.Empty,
			}.ToObjectArray(),
			new StreamsQActionRow
			{
				Streamsindex = 2,
				Streamsdrescription = "Randomly ever growing counter",
				Streamsoctetscounter = 0,
				Streamsbitrateondates = -1,
				Streamsbitrateondatesdata = String.Empty,
				Streamsbitrateontimes = -1,
				Streamsbitrateontimesdata = String.Empty,
			}.ToObjectArray(),
			new StreamsQActionRow
			{
				Streamsindex = 3,
				Streamsdrescription = "Randomly growing counters with regular reset",
				Streamsoctetscounter = 0,
				Streamsbitrateondates = -1,
				Streamsbitrateondatesdata = String.Empty,
				Streamsbitrateontimes = -1,
				Streamsbitrateontimesdata = String.Empty,
			}.ToObjectArray(),
			new StreamsQActionRow
			{
				Streamsindex = 4,
				Streamsdrescription = "Completely random counters",
				Streamsoctetscounter = 0,
				Streamsbitrateondates = -1,
				Streamsbitrateondatesdata = String.Empty,
				Streamsbitrateontimes = -1,
				Streamsbitrateontimesdata = String.Empty,
			}.ToObjectArray(),
		};

		protocol.FillArray(Parameter.Streams.tablePid, rows, NotifyProtocol.SaveOption.Full);
	}
}