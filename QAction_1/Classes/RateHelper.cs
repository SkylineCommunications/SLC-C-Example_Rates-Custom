namespace Skyline.Protocol.Rates
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Newtonsoft.Json;

	#region RateHelpers
	/// <summary>
	/// Class <c>RateHelper</c> helps calculating rates no matter its type (bit rates, counter rates, etc)
	/// </summary>
	[Serializable]
	public class RateHelper<T, U> where T : RateCounter<U>
	{
		[JsonProperty]
		protected readonly TimeSpan minDelta;

		[JsonProperty]
		protected readonly TimeSpan maxDelta;

		[JsonProperty]
		protected readonly List<T> counters;

		/// <summary>
		/// This is a constructor for the <c>RateHelper</c> class.
		/// </summary>
		/// <param name="minDelta">Minimum timespan necessary between 2 counters when calculating a rate. Counters will be buffered until this minimum delta is met.</param>
		/// <param name="minDelta">Maximum timespan allowed between 2 counters when calculating a rate.</param>
		private protected RateHelper(TimeSpan minDelta, TimeSpan maxDelta)
		{
			this.minDelta = minDelta;
			this.maxDelta = maxDelta;
		}

		protected bool TryCalculate(dynamic newCounter, DateTime time, double faultyReturn, out double rate)
		{
			rate = faultyReturn;

			// Sanity checks
			if (counters.Any() && (time <= counters[counters.Count - 1].Time || time - counters[counters.Count - 1].Time > maxDelta))
			{
				Reset();
			}

			if (newCounter < 0)
			{
				return false;
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
			if (oldCounterPos > -1)
			{
				unchecked
				{
					rate = (newCounter - counters[oldCounterPos].Counter) / (time - counters[oldCounterPos].Time).TotalMinutes;
				}

				counters.RemoveRange(0, oldCounterPos);
			}
			else
			{
				rate = faultyReturn;
			}

			return true;
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

	/// <inheritdoc/>
	[Serializable]
	public class RateHelper32 : RateHelper<RateCounter32, uint>
	{
		/// <inheritdoc/>
		public RateHelper32(TimeSpan minDelta, TimeSpan maxDelta) : base(minDelta, maxDelta)
		{
		}

		public double Calculate(uint newCounter, DateTime time, double faultyReturn = -1)
		{
			if (base.TryCalculate(newCounter, time, faultyReturn, out double rate))
			{
				counters.Add(new RateCounter32(newCounter, time));
			}

			return rate;
		}

		public static RateHelper32 FromJsonString(string rateHelperSerialized, TimeSpan minDelta, TimeSpan maxDelta)
		{
			return !String.IsNullOrWhiteSpace(rateHelperSerialized) ?
				JsonConvert.DeserializeObject<RateHelper32>(rateHelperSerialized) :
				new RateHelper32(minDelta, maxDelta);
		}
	}

	/// <inheritdoc/>
	[Serializable]
	public class RateHelper64 : RateHelper<RateCounter64, ulong>
	{
		/// <inheritdoc/>
		public RateHelper64(TimeSpan minDelta, TimeSpan maxDelta) : base(minDelta, maxDelta)
		{
		}

		public double Calculate(ulong newCounter, DateTime time, double faultyReturn = -1)
		{
			if (base.TryCalculate(newCounter, time, faultyReturn, out double rate))
			{
				counters.Add(new RateCounter64(newCounter, time));
			}

			return rate;
		}

		public static RateHelper64 FromJsonString(string rateHelperSerialized, TimeSpan minDelta, TimeSpan maxDelta)
		{
			return !String.IsNullOrWhiteSpace(rateHelperSerialized) ?
				JsonConvert.DeserializeObject<RateHelper64>(rateHelperSerialized) :
				new RateHelper64(minDelta, maxDelta);
		}
	}
	#endregion

	#region RateCounters
	public class RateCounter<U>
	{
		public DateTime Time { get; set; }
		public U Counter { get; set; }

		protected RateCounter(DateTime time)
		{
			Time = time;
		}
	}

	public class RateCounter32 : RateCounter<uint>
	{
		public RateCounter32(uint counter, DateTime time) : base(time)
		{
			Counter = counter;
		}
	}

	public class RateCounter64 : RateCounter<ulong>
	{
		public RateCounter64(ulong counter, DateTime time) : base(time)
		{
			Counter = counter;
		}
	}
	#endregion
}