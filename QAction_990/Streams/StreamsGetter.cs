namespace Skyline.Protocol.Streams
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Scripting;

	internal class StreamsGetter
	{
		private readonly SLProtocol protocol;

		internal StreamsGetter(SLProtocol protocol)
		{
			this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
		}

		internal object[] Keys { get; private set; }

		internal object[] InOctetsRateOnDatesData { get; private set; }

		internal object[] InOctetsRateOnTimesData { get; private set; }

		internal void Load()
		{
			uint[] columnsToGet = new uint[]
			{
				Parameter.Streams.Idx.streamsindex,
				Parameter.Streams.Idx.streamsbitrateondatesdata,
				Parameter.Streams.Idx.streamsbitrateontimesdata,
			};

			var tableData = (object[])protocol.NotifyProtocol(321, Parameter.Streams.tablePid, columnsToGet);

			Keys = (object[])tableData[0];
			InOctetsRateOnDatesData = (object[])tableData[2];
			InOctetsRateOnTimesData = (object[])tableData[3];
		}
	}
}
