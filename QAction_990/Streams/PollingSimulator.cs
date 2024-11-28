namespace Skyline.Protocol.Streams
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Scripting;
	using Skyline.DataMiner.Utils.Protocol.Extension;

	internal class PollingSimulator
	{
		private readonly SLProtocol protocol;

		public PollingSimulator(SLProtocol protocol)
		{
			this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
		}

		internal object[] Keys { get; private set; }

		internal object[] InOctets { get; private set; }

		internal void FakePoll()
		{
			uint[] columnsToGet = new uint[]
			{
				Parameter.Streams.Idx.streamsindex,
				Parameter.Streams.Idx.streamsoctetscounter,
			};

			var tableData = protocol.GetColumns(Parameter.Streams.tablePid, columnsToGet);

			Keys = (object[])tableData[0];
			InOctets = FakeNewOctetCounterValues((object[])tableData[1]);
		}

		/// <summary>
		/// Method used to simulate device data with random data.
		/// </summary>
		/// <param name="previous">Previous octets values used to calculate next ones.</param>
		/// <returns>New octets values.</returns>
		private static object[] FakeNewOctetCounterValues(object[] previous)
		{
			Random random = new Random();
			ulong[] predifined = new ulong[] { 48, 24, 96, 128, 246, 8 };

			ulong constantGrow = 24;
			ulong randomGrow = Convert.ToUInt64(random.Next(24));
			ulong randomPredefined = predifined[random.Next(predifined.Length - 1)];

			List<object> octetsValues;
			unchecked
			{
				// unchecked is there to make sure that when overflowing, a wrap around is happening instead of throwing an OverflowException
				octetsValues = new List<object>
				{
					Convert.ToUInt64(previous[0]) + constantGrow,
					Convert.ToUInt64(previous[1]) + randomGrow,
					Convert.ToUInt64(previous[2]) + randomPredefined,
					random.Next(UInt16.MaxValue),
				};
			}

			return octetsValues.ToArray();
		}
	}
}
