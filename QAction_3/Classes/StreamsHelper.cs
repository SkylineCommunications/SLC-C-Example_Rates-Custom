namespace Skyline.Protocol.Streams
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.Extensions;
	using Skyline.Protocol.Rates;
	using SLNetMessages = Skyline.DataMiner.Net.Messages;

	public class StreamsHelper
	{
		private readonly StreamsGetter getter;
		private readonly StreamsSetter setter;

		internal StreamsHelper(SLProtocolExt protocol)
		{
			getter = new StreamsGetter(protocol);
			getter.Load();

			setter = new StreamsSetter(protocol);
		}

		internal void ProcessData(DateTime now)
		{
			for (int i = 0; i < getter.Keys.Length; i++)
			{
				setter.SetColumnsData[Parameter.Streams.tablePid].Add(Convert.ToString(getter.Keys[i]));

				ProcessBitRates(i, now, minDelta: new TimeSpan(0, 1, 30), maxDelta: new TimeSpan(0, 10, 0));
			}
		}

		internal void UpdateProtocol()
		{
			setter.SetColumns();
		}

		private void ProcessBitRates(int getPosition, DateTime now, TimeSpan minDelta, TimeSpan maxDelta)
		{
			RateHelper rateHelper;

			rateHelper = RateHelper.FromJsonString(Convert.ToString(getter.InOctetsBuffer[getPosition]), minDelta, maxDelta);
			setter.SetColumnsData[Parameter.Streams.Pid.streamsoctetscounter_1003].Add(Convert.ToUInt64(getter.InOctets[getPosition]));
			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitrate_1004].Add(rateHelper.Calculate(Convert.ToUInt64(getter.InOctets[getPosition]), now, true));
			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitratedata_1005].Add(rateHelper.ToJsonString());
		}

		private class StreamsGetter
		{
			private readonly SLProtocolExt protocol;

			internal StreamsGetter(SLProtocolExt protocol)
			{
				this.protocol = protocol;
			}

			public object[] Keys { get; private set; }

			public object[] InOctets { get; private set; }

			public object[] InOctetsBuffer { get; private set; }

			internal void Load()
			{
				var tableData = (object[])protocol.NotifyProtocol(
					(int)SLNetMessages.NotifyType.NT_GET_TABLE_COLUMNS,
					Parameter.Streams.tablePid,
					new uint[] { Parameter.Streams.Idx.streamsindex_1001, Parameter.Streams.Idx.streamsoctetscounter_1003, Parameter.Streams.Idx.streamsbitratedata_1005 });

				Keys = (object[])tableData[0];
				InOctetsBuffer = (object[])tableData[2];
				InOctets = FillOctets((object[])tableData[1]);
			}

			private static object[] FillOctets(object[] previous)
			{
				var predifined = new object[] { 48, 24, 96, 128, 246, 8 };

				Random random = new Random();
				var randGrow = Convert.ToUInt64(random.Next(24));
				var randPredifined = Convert.ToUInt64(predifined[random.Next(predifined.Length - 1)]);

				var values = new List<object>
				{
					Convert.ToUInt64(previous[0]) + 24 > UInt64.MaxValue ? 0 : Convert.ToUInt64(previous[0]) + 24,
					Convert.ToUInt64(previous[1]) + randGrow > UInt64.MaxValue ? 0 : Convert.ToUInt64(previous[1]) + randGrow,
					Convert.ToUInt64(previous[2]) + randPredifined > UInt64.MaxValue ? 0 : Convert.ToUInt64(previous[2]) + randPredifined,
					random.Next(UInt16.MaxValue)
				};

				return values.ToArray();
			}
		}

		private class StreamsSetter
		{
			internal readonly Dictionary<object, List<object>> SetColumnsData;

			private readonly SLProtocolExt protocol;

			internal StreamsSetter(SLProtocolExt protocol)
			{
				this.protocol = protocol;

				SetColumnsData = new Dictionary<object, List<object>>
				{
					{ Parameter.Streams.tablePid, new List<object>() },
					{ Parameter.Streams.Pid.streamsoctetscounter_1003, new List<object>() },
					{ Parameter.Streams.Pid.streamsbitrate_1004, new List<object>() },
					{ Parameter.Streams.Pid.streamsbitratedata_1005, new List<object>() }
				};
			}

			internal void SetColumns()
			{
				protocol.SetColumns(SetColumnsData);
			}
		}
	}
}
