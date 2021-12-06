namespace Skyline.Protocol.Rates
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Runtime.Serialization;

	using Newtonsoft.Json;

	[Serializable]
	public class RateHelper
	{
		[JsonProperty]
		private readonly TimeSpan minDelta;

		[JsonProperty]
		private readonly TimeSpan maxDelta;

		[JsonProperty]
		private readonly List<RateCounter> counters = new List<RateCounter>();

		public RateHelper(TimeSpan minDelta, TimeSpan maxDelta)
		{
			this.minDelta = minDelta;
			this.maxDelta = maxDelta;
		}

		[IgnoreDataMember]
		public IReadOnlyCollection<RateCounter> Counters
		{
			get { return new ReadOnlyCollection<RateCounter>(counters); }
		}

		public static RateHelper FromJsonString(string rateHelperSerialized, TimeSpan minDelta, TimeSpan maxDelta)
		{
			return !String.IsNullOrWhiteSpace(rateHelperSerialized) ? JsonConvert.DeserializeObject<RateHelper>(rateHelperSerialized) : new RateHelper(minDelta, maxDelta);
		}

		public double Calculate(ulong newCounter, DateTime time, double faultyReturn = -1)
		{
			// Sanity checks
			if (counters.Any() && (time <= counters[counters.Count - 1].Time || time - counters[counters.Count - 1].Time > maxDelta))
			{
				Reset();
			}

			if (newCounter < 0)
			{
				return faultyReturn;
			}

			// Find previous counter to be used
			int oldCounterPos = -1;

			for (int i = counters.Count - 1; i > -1; i--)
			{
				if (time - counters[i].Time >= minDelta)
				{
					oldCounterPos = i;
					break;
				}
			}

			// Calculate
			double rate;

			if (oldCounterPos > -1)
			{
				rate = (newCounter - counters[oldCounterPos].Counter) / (time - counters[oldCounterPos].Time).TotalMinutes;

				counters.RemoveRange(0, oldCounterPos);
			}
			else
			{
				rate = faultyReturn;
			}

			// Add new counter
			counters.Add(new RateCounter(newCounter, time));

			return rate;
		}

		public double Calculate(ulong newCounter, DateTime time, bool isOctet = false, double faultyReturn = -1)
		{
			return isOctet ? Calculate(newCounter / 8, time, faultyReturn) : Calculate(newCounter, time, faultyReturn);
		}

		public void Reset()
		{
			counters.Clear();
		}

		public string ToJsonString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}

	public class RateCounter
	{
		public RateCounter(ulong counter, DateTime time)
		{
			Counter = counter;
			Time = time;
		}

		public ulong Counter { get; set; }

		public DateTime Time { get; set; }
	}
}