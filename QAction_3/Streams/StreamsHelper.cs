namespace Skyline.Protocol.Streams
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Library.Common.Rates;
	using Skyline.DataMiner.Library.Common.SafeConverters;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.Extensions;

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
			ulong inOctets = SafeConvert.ToUInt64(Convert.ToDouble(getter.InOctets[getPosition]));
			ulong inBytes = inOctets / 8;

			setter.SetColumnsData[Parameter.Streams.Pid.streamsoctetscounter].Add(inOctets);

			// Based on DateTime (typically used with HTTP, serial...)
			Rate64OnDateTime rate64OnDatesHelper = Rate64OnDateTime.FromJsonString(Convert.ToString(getter.InOctetsRateOnDatesData[getPosition]), minDelta, maxDelta);
			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitrateondates].Add(rate64OnDatesHelper.Calculate(inBytes, now));
			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitrateondatesdata].Add(rate64OnDatesHelper.ToJsonString());

			// Based on TimeSpan (typically used with SNMP)
			Rate64OnTimeSpan rate64OnTimesHelper = Rate64OnTimeSpan.FromJsonString(Convert.ToString(getter.InOctetsRateOnTimesData[getPosition]), minDelta, maxDelta);
			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitrateontimes].Add(rate64OnTimesHelper.Calculate(inBytes, new TimeSpan(0, 0, 10)));
			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitrateontimesdata].Add(rate64OnTimesHelper.ToJsonString());
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

			public object[] InOctetsRateOnDatesData { get; private set; }

			public object[] InOctetsRateOnTimesData { get; private set; }

			internal void Load()
			{
				var tableData = (object[])protocol.NotifyProtocol(321, Parameter.Streams.tablePid, new uint[]
					{
						Parameter.Streams.Idx.streamsindex,
						Parameter.Streams.Idx.streamsoctetscounter,
						Parameter.Streams.Idx.streamsbitrateondatesdata,
						Parameter.Streams.Idx.streamsbitrateontimesdata,
					});

				Keys = (object[])tableData[0];
				InOctets = BuildOctectsColumn((object[])tableData[1]);
				InOctetsRateOnDatesData = (object[])tableData[2];
				InOctetsRateOnTimesData = (object[])tableData[3];
			}

			/// <summary>
			/// Method used to simulate device data with random data.
			/// </summary>
			/// <param name="previous">Previous octets values used to calculate next ones.</param>
			/// <returns>New octets values.</returns>
			private static object[] BuildOctectsColumn(object[] previous)
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
			private readonly SLProtocol protocol;

			internal StreamsSetter(SLProtocol protocol)
			{
				this.protocol = protocol;
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
}
