namespace Skyline.Protocol.Streams
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.Extensions;

	internal class StreamsSetter
	{
		private readonly SLProtocol protocol;

		internal StreamsSetter(SLProtocol protocol)
		{
			this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
		}

		internal Dictionary<object, List<object>> SetColumnsData { get; } = new Dictionary<object, List<object>>
		{
			{ Parameter.Streams.tablePid, new List<object>() },
			{ Parameter.Streams.Pid.streamsoctetscounter, new List<object>() },
			{ Parameter.Streams.Pid.streamsbitrateondates, new List<object>() },
			{ Parameter.Streams.Pid.streamsbitrateondatesdata, new List<object>() },
			{ Parameter.Streams.Pid.streamsbitrateontimes, new List<object>() },
			{ Parameter.Streams.Pid.streamsbitrateontimesdata, new List<object>() },
		};

		internal void SetColumns()
		{
			protocol.SetColumns(SetColumnsData);
		}
	}
}
