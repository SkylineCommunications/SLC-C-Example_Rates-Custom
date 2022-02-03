using System;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Skyline.Protocol.Rates.Tests
{
	[TestClass()]
	public class Rate64OnDatesTests
	{
		private readonly TimeSpan minDelta = new TimeSpan(0, 1, 0);
		private readonly TimeSpan maxDelta = new TimeSpan(1, 0, 0);
		private const double faultyReturn = -1;

		#region Calculate

		[TestMethod()]
		public void CalculateInvalid_BackInTime()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper = Rate64OnDates.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(10, start);

			// Act
			double rate = helper.Calculate(20, start.AddSeconds(-10));

			// Assert
			double expectedRate = faultyReturn;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void CalculateInvalid_TooLate()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper = Rate64OnDates.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(10, start);

			// Act
			double rate = helper.Calculate(20, start.AddHours(2));

			// Assert
			double expectedRate = faultyReturn;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void CalculateInvalid_TooSoon()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper = Rate64OnDates.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(10, start);

			// Act
			double rate = helper.Calculate(20, start.AddSeconds(5));

			// Assert
			double expectedRate = faultyReturn;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void CalculateValid_ToOlderCounter()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper = Rate64OnDates.FromJsonString("", minDelta, maxDelta);

			// Old counters
			helper.Calculate(0, start.AddSeconds(-200));
			helper.Calculate(1, start.AddSeconds(-100));

			helper.Calculate(5, start);     // Counter to be used

			// Recent counters
			helper.Calculate(10, start.AddSeconds(90));
			helper.Calculate(20, start.AddSeconds(91));
			helper.Calculate(30, start.AddSeconds(92));
			helper.Calculate(40, start.AddSeconds(93));

			// Act
			double rate = helper.Calculate(50, start.AddSeconds(100));

			// Assert
			double expectedRate = (50.0 - 5.0) / 100d;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void CalculateValid_ToPreviousCounter()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper = Rate64OnDates.FromJsonString("", minDelta, maxDelta);

			// Old counters
			helper.Calculate(0, start.AddSeconds(-200));
			helper.Calculate(1, start.AddSeconds(-100));

			helper.Calculate(5, start);     // Counter to be used

			// Act
			double rate = helper.Calculate(50, start.AddSeconds(100));

			// Assert
			double expectedRate = (50.0 - 5.0) / 100d;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void CalculateValid_WithOverflow()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper = Rate64OnDates.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(UInt64.MaxValue - 10, start);

			// Act
			double rate = helper.Calculate(9, start.AddSeconds(100));

			// Assert
			double expectedRate = 20 / 100d;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		#endregion

		#region SerializeTests

		[TestMethod()]
		public void Serialize_Invalid_DifferentCounter()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper1 = Rate64OnDates.FromJsonString("", minDelta, maxDelta);
			helper1.Calculate(5, start);
			helper1.Calculate(10, start.AddSeconds(10));

			string serializedTemp = helper1.ToJsonString();
			var helper2 = Rate64OnDates.FromJsonString(serializedTemp, minDelta, maxDelta);

			// Different counter, same timing
			helper1.Calculate(20, start.AddSeconds(19));
			helper2.Calculate(21, start.AddSeconds(19));

			// Act
			string serialized1 = helper1.ToJsonString();
			string serialized2 = helper2.ToJsonString();

			// Assert
			serialized1.Should().NotBeEquivalentTo(serialized2);
		}

		[TestMethod()]
		public void Serialize_Invalid_DifferentDateTime()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper1 = Rate64OnDates.FromJsonString("", minDelta, maxDelta);
			helper1.Calculate(5, start);
			helper1.Calculate(10, start.AddSeconds(10));

			string serializedTemp = helper1.ToJsonString();
			var helper2 = Rate64OnDates.FromJsonString(serializedTemp, minDelta, maxDelta);

			// same counter, different timing
			helper1.Calculate(20, start.AddSeconds(19));
			helper2.Calculate(20, start.AddSeconds(21));

			// Act
			string serialized1 = helper1.ToJsonString();
			string serialized2 = helper2.ToJsonString();

			// Assert
			serialized1.Should().NotBeEquivalentTo(serialized2);
		}

		[TestMethod()]
		public void Serialize_Valid_DateTime()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper1 = Rate64OnDates.FromJsonString("", minDelta, maxDelta);
			helper1.Calculate(5, start);
			helper1.Calculate(10, start.AddSeconds(10));

			string serializedTemp = helper1.ToJsonString();
			var helper2 = Rate64OnDates.FromJsonString(serializedTemp, minDelta, maxDelta);

			AddSameToBoth(helper1, helper2, 20, start.AddSeconds(19));
			AddSameToBoth(helper1, helper2, 30, start.AddSeconds(27));

			// Act
			string serialized1 = helper1.ToJsonString();
			string serialized2 = helper2.ToJsonString();

			// Assert
			serialized1.Should().BeEquivalentTo(serialized2);
		}

		#endregion

		#region HelperMethods
		private static void AddSameToBoth(Rate64OnDates helper1, Rate64OnDates helper2, ulong newCounter, DateTime time)
		{
			helper1.Calculate(newCounter, time);
			helper2.Calculate(newCounter, time);
		}
		#endregion
	}
}