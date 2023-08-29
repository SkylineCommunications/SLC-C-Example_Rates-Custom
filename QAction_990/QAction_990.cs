using System;

using Skyline.DataMiner.Scripting;
using Skyline.Protocol.Streams;

/// <summary>
/// DataMiner QAction Class: Fill Streams Table.
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
			DateTime now = DateTime.UtcNow;

			StreamsHelper streamsHelper = new StreamsHelper(protocol);
			streamsHelper.ProcessData(now);
			streamsHelper.UpdateProtocol();
		}
		catch (Exception ex)
		{
			protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
		}
	}
}