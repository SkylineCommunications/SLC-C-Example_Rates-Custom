namespace Skyline.Protocol.Streams
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.Extensions;
	using Skyline.Protocol.Rates;

	public class StreamsHelper
	{
		private readonly StreamsGetter getter;
		private readonly StreamsSetter setter;

		internal StreamsHelper(SLProtocol protocol)
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

				ProcessBitRates(i, now, minDelta: new TimeSpan(0, 1, 0), maxDelta: new TimeSpan(0, 10, 0));
			}
		}

		internal void UpdateProtocol()
		{
			setter.SetColumns();
		}

		private void ProcessBitRates(int getPosition, DateTime now, TimeSpan minDelta, TimeSpan maxDelta)
		{
			RateHelper64 rateHelper = RateHelper64.FromJsonString(Convert.ToString(getter.InOctetsRateData[getPosition]), minDelta, maxDelta);

			// TODO: Safe conversion ?
			//ulong inOctets = Convert.ToUInt64(getter.InOctets[getPosition]);
			ulong inOctets = Convert.ToUInt64(getter.InOctets[getPosition]);
			ulong inBytes = inOctets / 8;

			setter.SetColumnsData[Parameter.Streams.Pid.streamsoctetscounter_1003].Add(inOctets);
			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitrate_1004].Add(rateHelper.Calculate(inBytes, now));
			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitratedata_1005].Add(rateHelper.ToJsonString());
		}

		private class StreamsGetter
		{
			private readonly SLProtocol protocol;

			internal StreamsGetter(SLProtocol protocol)
			{
				this.protocol = protocol;
			}

			public object[] Keys { get; private set; }

			public object[] InOctets { get; private set; }

			public object[] InOctetsRateData { get; private set; }

			internal void Load()
			{
				var tableData = (object[])protocol.NotifyProtocol(321, Parameter.Streams.tablePid, new uint[] {
					Parameter.Streams.Idx.streamsindex_1001,
					Parameter.Streams.Idx.streamsoctetscounter_1003,
					Parameter.Streams.Idx.streamsbitratedata_1005, });

				Keys = (object[])tableData[0];
				InOctets = FillOctets((object[])tableData[1]);
				InOctetsRateData = (object[])tableData[2];
			}

			/// <summary>
			/// Method used to simulate device data with random data.
			/// </summary>
			/// <param name="previous">Previous octets values used to calculate next ones.</param>
			/// <returns></returns>
			private static object[] FillOctets(object[] previous)
			{
				Random random = new Random();
				ulong[] predifined = new ulong[] { 48, 24, 96, 128, 246, 8 };

				ulong constantGrow = 24;
				ulong randomGrow = Convert.ToUInt64(random.Next(24));
				ulong randomPredifined = predifined[random.Next(predifined.Length - 1)];

				var octetsValues = new List<object>
				{
					Convert.ToUInt64(previous[0]) + constantGrow > UInt64.MaxValue ? 0 : Convert.ToUInt64(previous[0]) + constantGrow,
					Convert.ToUInt64(previous[1]) + randomGrow > UInt64.MaxValue ? 0 : Convert.ToUInt64(previous[1]) + randomGrow,
					Convert.ToUInt64(previous[2]) + randomPredifined > UInt64.MaxValue ? 0 : Convert.ToUInt64(previous[2]) + randomPredifined,
					random.Next(UInt16.MaxValue),
				};

				return octetsValues.ToArray();
			}
		}

		private class StreamsSetter
		{
			internal readonly Dictionary<object, List<object>> SetColumnsData;

			private readonly SLProtocol protocol;

			internal StreamsSetter(SLProtocol protocol)
			{
				this.protocol = protocol;

				SetColumnsData = new Dictionary<object, List<object>>
				{
					{ Parameter.Streams.tablePid, new List<object>() },
					{ Parameter.Streams.Pid.streamsoctetscounter_1003, new List<object>() },
					{ Parameter.Streams.Pid.streamsbitrate_1004, new List<object>() },
					{ Parameter.Streams.Pid.streamsbitratedata_1005, new List<object>() },
				};
			}

			internal void SetColumns()
			{
				protocol.SetColumns(SetColumnsData);
			}
		}
	}
}
