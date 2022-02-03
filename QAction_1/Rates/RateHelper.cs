namespace Skyline.Protocol.Rates
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Newtonsoft.Json;

	public enum RateBase { Second, Minute, Hour, Day };

	#region RateHelperBaseClasses
	/// <summary>
	/// Class <see cref="RateHelper"/> helps calculating rates of all sorts (bit rates, counter rates, etc) based on counters.
	/// This class is meant to be used as base class for more specific RateHelpers depending on the range of counters (<see cref="System.UInt32"/>, <see cref="System.UInt64"/>, etc).
	/// </summary>
	[Serializable]
	public class RateHelper<T, U> where T : RateCounter<U>
	{
		[JsonProperty]
		public RateBase RateBase { get; set; }

		[JsonProperty]
		protected readonly TimeSpan minDelta;

		[JsonProperty]
		protected readonly TimeSpan maxDelta;

		[JsonProperty]
		protected readonly List<T> counters = new List<T>();

		[JsonConstructor]
		private protected RateHelper(TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase)
		{
			this.minDelta = minDelta;
			this.maxDelta = maxDelta;
			this.RateBase = rateBase;
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

		protected static void ValidateMinAndMaxDeltas(TimeSpan minDelta, TimeSpan maxDelta)
		{
			if (minDelta < TimeSpan.Zero)
			{
				throw new ArgumentException("minDelta is a negative TimeSpan.", "minDelta");
			}

			if (maxDelta < TimeSpan.Zero)
			{
				throw new ArgumentException("maxDelta is a negative TimeSpan.", "maxDelta");
			}

			if (maxDelta <= minDelta)
			{
				throw new ArgumentException("minDelta is bigger than maxDelta.");
			}
		}

		protected double Calculate(T newRateCounter, int oldCounterPos, TimeSpan rateTimeSpan, double faultyReturn)
		{
			// Calculate
			double rate;
			if (oldCounterPos > -1)
			{
				rate = CalculateRate(newRateCounter.Counter, counters[oldCounterPos].Counter, rateTimeSpan);
				counters.RemoveRange(0, oldCounterPos);
			}
			else
			{
				rate = faultyReturn;
			}

			// Add new counter
			counters.Add(newRateCounter);

			return rate;
		}

		private double CalculateRate(dynamic newCounter, dynamic oldCounter, TimeSpan timeSpan)
		{
			unchecked
			{
				// Note that the use of var without casting here only works cause currently, generic type U can only be uint or ulong and :
				// - subtracting 2 uint implicitly returns a uint and handles wrap around nicely.
				// - subtracting 2 ulong implicitly returns a ulong and handles wrap around nicely.
				// If generic type U could be of other types, then an explicit casting could be required. Example:
				// - subtracting 2 ushort implicitly first converts both values to int, subtract them and returns an int and won't handle wrap around properly.
				//    In such case, an explicit cast to ushort would be required for the wrap around to be properly handled.

				var counterIncrease = newCounter - oldCounter;

				switch (RateBase)
				{
					case RateBase.Second:
						return counterIncrease / timeSpan.TotalSeconds;
					case RateBase.Minute:
						return counterIncrease / timeSpan.TotalMinutes;
					case RateBase.Hour:
						return counterIncrease / timeSpan.TotalHours;
					case RateBase.Day:
						return counterIncrease / timeSpan.TotalDays;
					default:
						return counterIncrease / timeSpan.TotalSeconds;
				}
			}
		}
	}

	/// <summary>
	/// Class <see cref="RateHelper"/> helps calculating rates of all sorts (bit rates, counter rates, etc) based on counters and DateTimes.
	/// This class is meant to be used as base class for more specific RateHelpers depending on the range of counters (<see cref="System.UInt32"/>, <see cref="System.UInt64"/>, etc).
	/// </summary>
	[Serializable]
	public class RateOnDates<T, U> : RateHelper<T, U> where T : CounterWithDate<U>
	{
		[JsonConstructor]
		private protected RateOnDates(TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase) : base(minDelta, maxDelta, rateBase) { }

		protected double Calculate(T newRateCounter, double faultyReturn)
		{
			// Sanity Checks
			if (counters.Any() && (newRateCounter.DateTime <= counters[counters.Count - 1].DateTime || newRateCounter.DateTime - counters[counters.Count - 1].DateTime > maxDelta))
			{
				Reset();
			}

			// Find previous counter to be used
			int oldCounterPos = -1;
			TimeSpan rateTimeSpan = new TimeSpan();
			for (int i = counters.Count - 1; i > -1; i--)
			{
				rateTimeSpan = newRateCounter.DateTime - counters[i].DateTime;
				if (rateTimeSpan >= minDelta)
				{
					oldCounterPos = i;
					break;
				}
			}

			return base.Calculate(newRateCounter, oldCounterPos, rateTimeSpan, faultyReturn);
		}
	}

	/// <summary>
	/// Class <see cref="RateHelper"/> helps calculating rates of all sorts (bit rates, counter rates, etc) based on counters and TimeSpans.
	/// This class is meant to be used as base class for more specific RateHelpers depending on the range of counters (<see cref="System.UInt32"/>, <see cref="System.UInt64"/>, etc).
	/// </summary>
	[Serializable]
	public class RateOnTimes<T, U> : RateHelper<T, U> where T : CounterWithTime<U>
	{
		[JsonConstructor]
		private protected RateOnTimes(TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase) : base(minDelta, maxDelta, rateBase) { }

		protected double Calculate(T newRateCounter, double faultyReturn)
		{
			// Sanity Checks
			if (counters.Any() && newRateCounter.TimeSpan > maxDelta)
			{
				Reset();
			}

			if (newRateCounter.TimeSpan < TimeSpan.Zero)
			{
				return faultyReturn;
			}

			// Find previous counter to be used
			int oldCounterPos = -1;
			TimeSpan rateTimeSpan = newRateCounter.TimeSpan;
			for (int i = counters.Count - 1; i > -1; i--)
			{
				if (rateTimeSpan >= minDelta)
				{
					oldCounterPos = i;
					break;
				}

				rateTimeSpan += counters[i].TimeSpan;
			}

			return Calculate(newRateCounter, oldCounterPos, rateTimeSpan, faultyReturn);
		}
	}
	#endregion

	#region RateHelperRealClasses
	/// <summary>
	/// Allows calculating rates of all sorts (bit rates, counter rates, etc) based on <see cref="System.UInt32"/> counters and <see cref="System.DateTime"/> values.
	/// </summary>
	[Serializable]
	public class Rate32OnDates : RateOnDates<Counter32WithDate, uint>
	{
		[JsonConstructor]
		private Rate32OnDates(TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase) : base(minDelta, maxDelta, rateBase) { }

		/// <summary>
		/// Calculates the rate using provided <paramref name="newCounter"/> against previous counters buffered in this <see cref="Rate32OnDates"/> instance.
		/// </summary>
		/// <param name="newCounter">The latest known counter value.</param>
		/// <param name="time">The <see cref="System.DateTime"/> at which <paramref name="newCounter"/> value was taken.</param>
		/// <param name="faultyReturn">The value to be returned in case a correct rate could not be calculated.</param>
		/// <returns>The calculated rate or the value specified in <paramref name="faultyReturn"/> in case the rate can not be calculated.</returns>
		public double Calculate(uint newCounter, DateTime time, double faultyReturn = -1)
		{
			var rateCounter = new Counter32WithDate(newCounter, time);
			return Calculate(rateCounter, faultyReturn);
		}

		/// <summary>
		/// Deserializes a JSON <see cref="System.String"/> to a <see cref="Rate32OnDates"/> instance.
		/// </summary>
		/// <param name="rateHelperSerialized">Serialized <see cref="Rate32OnDates"/> instance.</param>
		/// <param name="minDelta">Minimum <see cref="System.TimeSpan"/> necessary between 2 counters when calculating a rate. Counters will be buffered until this minimum delta is met.</param>
		/// <param name="maxDelta">Maximum <see cref="System.TimeSpan"/> allowed between 2 counters when calculating a rate.</param>
		/// <returns>A new instance of the <see cref="Rate32OnDates"/> class with all data found in <paramref name="rateHelperSerialized"/>.</returns>
		public static Rate32OnDates FromJsonString(string rateHelperSerialized, TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase = RateBase.Second)
		{
			ValidateMinAndMaxDeltas(minDelta, maxDelta);

			return !String.IsNullOrWhiteSpace(rateHelperSerialized) ?
				JsonConvert.DeserializeObject<Rate32OnDates>(rateHelperSerialized) :
				new Rate32OnDates(minDelta, maxDelta, rateBase);
		}
	}

	/// <summary>
	/// Allows calculating rates of all sorts (bit rates, counter rates, etc) based on <see cref="System.UInt32"/> counters and <see cref="System.TimeSpan"/> values.
	/// </summary>
	[Serializable]
	public class Rate32OnTimes : RateOnTimes<Counter32WithTime, uint>
	{
		[JsonConstructor]
		private Rate32OnTimes(TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase) : base(minDelta, maxDelta, rateBase) { }

		/// <summary>
		/// Calculates the rate using provided <paramref name="newCounter"/> against previous counters buffered in this <see cref="Rate32OnTimes"/> instance.
		/// </summary>
		/// <param name="newCounter">The latest known counter value.</param>
		/// <param name="delta">The elapse <see cref="System.TimeSpan"/> between this new counter and previous one.</param>
		/// <param name="faultyReturn">The value to be returned in case a correct rate could not be calculated.</param>
		/// <returns>The calculated rate or the value specified in <paramref name="faultyReturn"/> in case the rate can not be calculated.</returns>
		public double Calculate(uint newCounter, TimeSpan delta, double faultyReturn = -1)
		{
			var rateCounter = new Counter32WithTime(newCounter, delta);
			return Calculate(rateCounter, faultyReturn);
		}

		/// <summary>
		/// Deserializes a JSON <see cref="System.String"/> to a <see cref="Rate32OnTimes"/> instance.
		/// </summary>
		/// <param name="rateHelperSerialized">Serialized <see cref="Rate32OnTimes"/> instance.</param>
		/// <param name="minDelta">Minimum <see cref="System.TimeSpan"/> necessary between 2 counters when calculating a rate. Counters will be buffered until this minimum delta is met.</param>
		/// <param name="maxDelta">Maximum <see cref="System.TimeSpan"/> allowed between 2 counters when calculating a rate.</param>
		/// <returns>A new instance of the <see cref="Rate32OnTimes"/> class with all data found in <paramref name="rateHelperSerialized"/>.</returns>
		public static Rate32OnTimes FromJsonString(string rateHelperSerialized, TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase = RateBase.Second)
		{
			ValidateMinAndMaxDeltas(minDelta, maxDelta);

			return !String.IsNullOrWhiteSpace(rateHelperSerialized) ?
				JsonConvert.DeserializeObject<Rate32OnTimes>(rateHelperSerialized) :
				new Rate32OnTimes(minDelta, maxDelta, rateBase);
		}
	}

	/// <summary>
	/// Allows calculating rates of all sorts (bit rates, counter rates, etc) based on <see cref="System.UInt64"/> counters and <see cref="System.DateTime"/> values.
	/// </summary>
	[Serializable]
	public class Rate64OnDates : RateOnDates<Counter64WithDate, ulong>
	{
		[JsonConstructor]
		private Rate64OnDates(TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase) : base(minDelta, maxDelta, rateBase) { }

		/// <summary>
		/// Calculates the rate using provided <paramref name="newCounter"/> against previous counters buffered in this <see cref="Rate64OnDates"/> instance.
		/// </summary>
		/// <param name="newCounter">The latest known counter value.</param>
		/// <param name="time">The <see cref="System.DateTime"/> at which <paramref name="newCounter"/> value was taken.</param>
		/// <param name="faultyReturn">The value to be returned in case a correct rate could not be calculated.</param>
		/// <returns>The calculated rate or the value specified in <paramref name="faultyReturn"/> in case the rate can not be calculated.</returns>
		public double Calculate(ulong newCounter, DateTime time, double faultyReturn = -1)
		{
			var rateCounter = new Counter64WithDate(newCounter, time);
			return Calculate(rateCounter, faultyReturn);
		}

		/// <summary>
		/// Deserializes a JSON <see cref="System.String"/> to a <see cref="Rate64OnDates"/> instance.
		/// </summary>
		/// <param name="rateHelperSerialized">Serialized <see cref="Rate64OnDates"/> instance.</param>
		/// <param name="minDelta">Minimum <see cref="System.TimeSpan"/> necessary between 2 counters when calculating a rate. Counters will be buffered until this minimum delta is met.</param>
		/// <param name="maxDelta">Maximum <see cref="System.TimeSpan"/> allowed between 2 counters when calculating a rate.</param>
		/// <param name="rateBase">Choose whether the rate should be calculated per second, minute, hour or day.</param>
		/// <returns>A new instance of the <see cref="Rate64OnDates"/> class with all data found in <paramref name="rateHelperSerialized"/>.</returns>
		public static Rate64OnDates FromJsonString(string rateHelperSerialized, TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase = RateBase.Second)
		{
			ValidateMinAndMaxDeltas(minDelta, maxDelta);

			return !String.IsNullOrWhiteSpace(rateHelperSerialized) ?
				JsonConvert.DeserializeObject<Rate64OnDates>(rateHelperSerialized) :
				new Rate64OnDates(minDelta, maxDelta, rateBase);
		}
	}

	/// <summary>
	/// Allows calculating rates of all sorts (bit rates, counter rates, etc) based on <see cref="System.UInt64"/> counters and <see cref="System.TimeSpan"/> values.
	/// </summary>
	[Serializable]
	public class Rate64OnTimes : RateOnTimes<Counter64WithTime, ulong>
	{
		[JsonConstructor]
		private Rate64OnTimes(TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase) : base(minDelta, maxDelta, rateBase) { }

		/// <summary>
		/// Calculates the rate using provided <paramref name="newCounter"/> against previous counters buffered in this <see cref="Rate64OnTimes"/> instance.
		/// </summary>
		/// <param name="newCounter">The latest known counter value.</param>
		/// <param name="delta">The elapse <see cref="System.TimeSpan"/> between this new counter and previous one.</param>
		/// <param name="faultyReturn">The value to be returned in case a correct rate could not be calculated.</param>
		/// <returns>The calculated rate or the value specified in <paramref name="faultyReturn"/> in case the rate can not be calculated.</returns>
		public double Calculate(ulong newCounter, TimeSpan delta, double faultyReturn = -1)
		{
			var rateCounter = new Counter64WithTime(newCounter, delta);
			return Calculate(rateCounter, faultyReturn);
		}

		/// <summary>
		/// Deserializes a JSON <see cref="System.String"/> to a <see cref="Rate64OnTimes"/> instance.
		/// </summary>
		/// <param name="rateHelperSerialized">Serialized <see cref="Rate64OnTimes"/> instance.</param>
		/// <param name="minDelta">Minimum <see cref="System.TimeSpan"/> necessary between 2 counters when calculating a rate. Counters will be buffered until this minimum delta is met.</param>
		/// <param name="maxDelta">Maximum <see cref="System.TimeSpan"/> allowed between 2 counters when calculating a rate.</param>
		/// <param name="rateBase">Choose whether the rate should be calculated per second, minute, hour or day.</param>
		/// <returns>A new instance of the <see cref="Rate64OnTimes"/> class with all data found in <paramref name="rateHelperSerialized"/>.</returns>
		public static Rate64OnTimes FromJsonString(string rateHelperSerialized, TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase = RateBase.Second)
		{
			ValidateMinAndMaxDeltas(minDelta, maxDelta);

			return !String.IsNullOrWhiteSpace(rateHelperSerialized) ?
				JsonConvert.DeserializeObject<Rate64OnTimes>(rateHelperSerialized) :
				new Rate64OnTimes(minDelta, maxDelta, rateBase);
		}
	}
	#endregion

	#region RateCounterBaseClasses
	public class RateCounter<U>
	{
		public U Counter { get; set; }

		private protected RateCounter() { }     // Default constructor for Deserializer
	}

	public class CounterWithDate<U> : RateCounter<U>
	{
		public DateTime DateTime { get; set; }

		private protected CounterWithDate() { }     // Default constructor for Deserializer

		internal CounterWithDate(DateTime dateTime)
		{
			DateTime = dateTime;
		}
	}

	public class CounterWithTime<U> : RateCounter<U>
	{
		public TimeSpan TimeSpan { get; set; }

		private protected CounterWithTime() { }     // Default constructor for Deserializer

		internal CounterWithTime(TimeSpan timeSpan)
		{
			TimeSpan = timeSpan;
		}
	}
	#endregion

	#region RateCounterRealClasses
	public class Counter32WithDate : CounterWithDate<uint>
	{
		private Counter32WithDate() { }     // Default constructor for Deserializer

		internal Counter32WithDate(uint counter, DateTime dateTime) : base(dateTime)
		{
			Counter = counter;
		}
	}

	public class Counter32WithTime : CounterWithTime<uint>
	{
		private Counter32WithTime() { }     // Default constructor for Deserializer

		internal Counter32WithTime(uint counter, TimeSpan timeSpan) : base(timeSpan)
		{
			Counter = counter;
		}
	}

	public class Counter64WithDate : CounterWithDate<ulong>
	{
		private Counter64WithDate() { }     // Default constructor for Deserializer

		internal Counter64WithDate(ulong counter, DateTime dateTime) : base(dateTime)
		{
			Counter = counter;
		}
	}

	public class Counter64WithTime : CounterWithTime<ulong>
	{
		private Counter64WithTime() { }     // Default constructor for Deserializer

		internal Counter64WithTime(ulong counter, TimeSpan timeSpan) : base(timeSpan)
		{
			Counter = counter;
		}
	}
	#endregion
}