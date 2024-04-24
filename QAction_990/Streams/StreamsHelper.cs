namespace Skyline.Protocol.Streams
{
	using System;

	using Skyline.DataMiner.Scripting;
	using Skyline.DataMiner.Utils.Rates.Common;
	using Skyline.DataMiner.Utils.SafeConverters;

	public partial class StreamsHelper
	{
		private readonly SLProtocol protocol;
		private readonly DateTime now;

		private readonly StreamsGetter getter;
		private readonly StreamsSetter setter;

		private readonly PollingSimulator pollingSimulator;

		private readonly TimeSpan minDelta = new TimeSpan(0, 0, 20);
		private readonly TimeSpan maxDelta = new TimeSpan(0, 10, 0);

		internal StreamsHelper(SLProtocol protocol, DateTime now)
		{
			this.protocol = protocol;
			this.now = now;

			getter = new StreamsGetter(protocol);
			setter = new StreamsSetter(protocol);

			pollingSimulator = new PollingSimulator(protocol);
		}

		internal void GetPreviousData()
		{
			getter.Load();
		}

		internal void PollDataFromDevice()
		{
			pollingSimulator.FakePoll();
		}

		internal void ProcessData()
		{
			for (int i = 0; i < getter.Keys.Length; i++)
			{
				// Process Key
				setter.SetColumnsData[Parameter.Streams.tablePid].Add(Convert.ToString(getter.Keys[i]));

				/* Process new octet count
				 *  Note that in this fake example,
				 *  we can rely on the fact the position of the corresponding data in pollingSimulator and in the previous data getter will be the same
				 *  but on a real use-case, the data polled will have to be mapped to the corresponding previous data
				 */
				ulong newOctetCount = SafeConvert.ToUInt64(Convert.ToDouble(pollingSimulator.InOctets[i]));
				setter.SetColumnsData[Parameter.Streams.Pid.streamsoctetscounter].Add(newOctetCount);

				// Process rates
				ProcessDateTimeRate(newOctetCount, Convert.ToString(getter.InOctetsRateOnDatesData[i]));
				ProcessTimeSpanRate(newOctetCount, Convert.ToString(getter.InOctetsRateOnTimesData[i]));
			}
		}

		internal void UpdateProtocol()
		{
			setter.SetColumns();
		}

		private void ProcessDateTimeRate(ulong newOctetCount, string previousRateData)
		{
			// Based on DateTime (typically used with HTTP, serial, SNMP over SLScripting...)
			Rate64OnDateTime rate64OnDatesHelper = Rate64OnDateTime.FromJsonString(previousRateData, minDelta, maxDelta);
			double octetRate = rate64OnDatesHelper.Calculate(newOctetCount, now);
			double bitRate = octetRate > 0 ? octetRate * 8 : octetRate;

			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitrateondates].Add(bitRate);
			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitrateondatesdata].Add(rate64OnDatesHelper.ToJsonString());
		}

		private void ProcessTimeSpanRate(ulong newOctetCount, string previousRateData)
		{
			// Based on TimeSpan (typically used with SNMP over SLProtocol-SLSNMPManager)
			Rate64OnTimeSpan rate64OnTimesHelper = Rate64OnTimeSpan.FromJsonString(previousRateData, minDelta, maxDelta);
			double octetRate = rate64OnTimesHelper.Calculate(newOctetCount, new TimeSpan(0, 0, 10));
			double bitRate = octetRate > 0 ? octetRate * 8 : octetRate;

			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitrateontimes].Add(bitRate);
			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitrateontimesdata].Add(rate64OnTimesHelper.ToJsonString());
		}
	}
}
