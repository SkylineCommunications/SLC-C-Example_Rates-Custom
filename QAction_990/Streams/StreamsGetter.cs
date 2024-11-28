namespace Skyline.Protocol.Streams
{
	using System;

	using Skyline.DataMiner.Scripting;
	using Skyline.DataMiner.Utils.Protocol.Extension;

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

			var tableData = protocol.GetColumns(Parameter.Streams.tablePid, columnsToGet);

			Keys = (object[])tableData[0];
			InOctetsRateOnDatesData = (object[])tableData[1];
			InOctetsRateOnTimesData = (object[])tableData[2];
		}
	}
}
