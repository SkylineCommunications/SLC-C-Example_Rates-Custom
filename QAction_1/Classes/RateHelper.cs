namespace Skyline.Protocol.Rates
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Newtonsoft.Json;

	#region RateHelpers
	/// <summary>
	/// Class <see cref="RateHelper"/> class helps calculating rates of all sorts (bit rates, counter rates, etc) based on counters.
	/// This class is meant to be used as base class for more specific RateHelpers depending on the range of counters (<see cref="System.UInt32"/>, <see cref="System.UInt64"/>, etc).
	/// </summary>
	[Serializable]
	public class RateHelper<T, U> where T : RateCounter<U>/*, new()*/
	{
		/// <summary>
		/// The minimum delta.
		/// </summary>
		[JsonProperty]
		protected readonly TimeSpan minDelta;

		/// <summary>
		/// The maximum delta.
		/// </summary>
		[JsonProperty]
		protected readonly TimeSpan maxDelta;

		/// <summary>
		/// The list of counter values.
		/// </summary>
		[JsonProperty]
		protected readonly List<T> counters = new List<T>();

		private protected RateHelper(TimeSpan minDelta, TimeSpan maxDelta)
		{
			this.minDelta = minDelta;
			this.maxDelta = maxDelta;
		}

		/// <summary>
		/// Tries calculating the rate based on the provided counter value in <paramref name="newCounter"/> 
		/// </summary>
		/// <param name="newCounter">The new counter value.</param>
		/// <param name="time">The time stamp of the new counter value.</param>
		/// <param name="faultyReturn">The value that should be returned in case the rate can not be calculated.</param>
		/// <param name="rate">The calculated rate.</param>
		/// <returns><c>true</c> if the calculation succeeded; otherwise, <c>false</c>.</returns>
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
					// Note that the use of var without casting here only works cause currently, generic type U can only be uint or ulong and :
					// - subtracting 2 uint implicitly returns a uint and handles wrap around nicely.
					// - subtracting 2 ulong implicitly returns a ulong and handles wrap around nicely.
					// If generic type U could be of other types, then an explicit casting could be required. Example:
					// - subtracting 2 ushort implicitly first converts both values to int, subtract them and returns an int and won't handle wrap around properly.
					//    In such case, an explicit cast to ushort would be required for the wrap around to be properly handled.
					var counterIncrease = newCounter - counters[oldCounterPos].Counter;

					rate = counterIncrease / (time - counters[oldCounterPos].Time).TotalMinutes;
				}

				counters.RemoveRange(0, oldCounterPos);
			}
			else
			{
				rate = faultyReturn;
			}

			return true;
		}

		/// <summary>
		/// Resets the currently buffered data of this <see cref="RateHelper"/> instance.
		/// </summary>
		public void Reset()
		{
			counters.Clear();
		}

		/// <summary>
		/// Serializes the currently buffered data of this <see cref="RateHelper"/> instance.
		/// </summary>
		/// <returns>A JSON string containing the serialized data of this <see cref="RateHelper"/> instance.</returns>
		public string ToJsonString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}

	/// <summary>
	/// Allows calculating rates of all sorts (bit rates, counter rates, etc) based on 32 bit <see cref="System.UInt32"/> counters.
	/// </summary>
	[Serializable]
	public class RateHelper32 : RateHelper<RateCounter32, uint>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RateHelper32"/> class.
		/// </summary>
		/// <param name="minDelta">The minimum delta.</param>
		/// <param name="maxDelta">The maximum delta.</param>
		public RateHelper32(TimeSpan minDelta, TimeSpan maxDelta) : base(minDelta, maxDelta) { }

		/// <summary>
		/// Calculates the rate using provided <paramref name="newCounter"/> against previous counters buffered in this <see cref="RateHelper32"/> instance.
		/// </summary>
		/// <param name="newCounter">The latest known counter value.</param>
		/// <param name="time">The DateTime at which <paramref name="newCounter"/> value was taken.</param>
		/// <param name="faultyReturn">The value to be returned in case a correct rate could not be calculated.</param>
		/// <returns>The calculated rate or the value specified in <paramref name="faultyReturn"/> in case the rate can not be calculated.</returns>
		public double Calculate(uint newCounter, DateTime time, double faultyReturn = -1)
		{
			if (base.TryCalculate(newCounter, time, faultyReturn, out double rate))
			{
				counters.Add(new RateCounter32(newCounter, time));
			}

			return rate;
		}

		/// <summary>
		/// Deserializes a JSON <typeparamref name="string" /> to a <c>RateHelper32</c> instance.
		/// </summary>
		/// <param name="rateHelperSerialized">Serialized <c>RateHelper32</c> instance.</param>
		/// <param name="minDelta">Minimum timespan necessary between 2 counters when calculating a rate. Counters will be buffered until this minimum delta is met.</param>
		/// <param name="minDelta">Maximum timespan allowed between 2 counters when calculating a rate.</param>
		/// <returns></returns>
		public static RateHelper32 FromJsonString(string rateHelperSerialized, TimeSpan minDelta, TimeSpan maxDelta)
		{
			return !String.IsNullOrWhiteSpace(rateHelperSerialized) ?
				JsonConvert.DeserializeObject<RateHelper32>(rateHelperSerialized) :
				new RateHelper32(minDelta, maxDelta);
		}
	}

	/// <summary>
	/// Allows calculating rates of all sorts (bit rates, counter rates, etc) based on 32 bit <see cref="System.UInt64"/> counters.
	/// </summary>
	[Serializable]
	public class RateHelper64 : RateHelper<RateCounter64, ulong>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RateHelper64"/> class.
		/// </summary>
		/// <param name="minDelta">The minimum delta.</param>
		/// <param name="maxDelta">The maximum delta.</param>
		public RateHelper64(TimeSpan minDelta, TimeSpan maxDelta) : base(minDelta, maxDelta) { }

		/// <summary>
		/// Calculates a rate using provided <paramref name="newCounter"/> against previous counters buffered in this <c>RateHelper32</c> instance. 
		/// </summary>
		/// <param name="newCounter">The latest known counter value.</param>
		/// <param name="time">The DateTime at which <paramref name="newCounter"/> value was taken.</param>
		/// <param name="faultyReturn">The value to be returned in case a correct rate could not be calculated.</param>
		/// <returns></returns>
		public double Calculate(ulong newCounter, DateTime time, double faultyReturn = -1)
		{
			if (base.TryCalculate(newCounter, time, faultyReturn, out double rate))
			{
				counters.Add(new RateCounter64(newCounter, time));
			}

			return rate;
		}

		/// <summary>
		/// Deserializes a JSON string to a <c>RateHelper64</c> instance.
		/// </summary>
		/// <param name="rateHelperSerialized">Serialized <c>RateHelper64</c> instance.</param>
		/// <param name="minDelta">Minimum timespan necessary between 2 counters when calculating a rate. Counters will be buffered until this minimum delta is met.</param>
		/// <param name="minDelta">Maximum timespan allowed between 2 counters when calculating a rate.</param>
		/// <returns></returns>
		public static RateHelper64 FromJsonString(string rateHelperSerialized, TimeSpan minDelta, TimeSpan maxDelta)
		{
			return !String.IsNullOrWhiteSpace(rateHelperSerialized) ?
				JsonConvert.DeserializeObject<RateHelper64>(rateHelperSerialized) :
				new RateHelper64(minDelta, maxDelta);
		}
	}
	#endregion

	#region RateCounters
	/// <summary>
	/// Represents a rate counter.
	/// </summary>
	/// <typeparam name="U"></typeparam>
	public class RateCounter<U>
	{
		/// <summary>
		/// Gets or sets the time stamp.
		/// </summary>
		/// <value>The time stamp.</value>
		public DateTime Time { get; set; }

		/// <summary>
		/// Gets or sets the counter value.
		/// </summary>
		/// <value>The counter value.</value>
		public U Counter { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="RateCounter{U}"/> class.
		/// </summary>
		/// <param name="time">The time stamp.</param>
		protected RateCounter(DateTime time)
		{
			Time = time;
		}
	}

	/// <summary>
	/// Represents a rate counter based on <see cref="System.UInt32"/> counter values.
	/// </summary>
	public class RateCounter32 : RateCounter<uint>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RateCounter32"/> class.
		/// </summary>
		/// <param name="counter">The counter value.</param>
		/// <param name="time">The time value.</param>
		public RateCounter32(uint counter, DateTime time) : base(time)
		{
			Counter = counter;
		}
	}

	/// <summary>
	/// Represents a rate counter based on <see cref="System.UInt64"/> counter values.
	/// </summary>
	public class RateCounter64 : RateCounter<ulong>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RateCounter64"/> class.
		/// </summary>
		/// <param name="counter">The counter value.</param>
		/// <param name="time">The time value.</param>
		public RateCounter64(ulong counter, DateTime time) : base(time)
		{
			Counter = counter;
		}
	}
	#endregion
}