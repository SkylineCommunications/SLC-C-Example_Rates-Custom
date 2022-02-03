// --- auto-generated code --- do not modify ---
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Skyline.DataMiner.Scripting
{
public static class Parameter
{
	public class Write
	{
	}
	public class Streams
	{
		/// <summary>PID: 1000</summary>
		public const int tablePid = 1000;
		/// <summary>IDX: 0</summary>
		public const int indexColumn = 0;
		/// <summary>PID: 1001</summary>
		public const int indexColumnPid = 1001;
		public class Pid
		{
			/// <summary>PID: 1001 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsindex_1001 = 1001;
			/// <summary>PID: 1001 | Type: read</summary>
			public const int streamsindex = 1001;
			/// <summary>PID: 1002 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsdrescription_1002 = 1002;
			/// <summary>PID: 1002 | Type: read</summary>
			public const int streamsdrescription = 1002;
			/// <summary>PID: 1003 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsoctetscounter_1003 = 1003;
			/// <summary>PID: 1003 | Type: read</summary>
			public const int streamsoctetscounter = 1003;
			/// <summary>PID: 1004 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsbitrateondates_1004 = 1004;
			/// <summary>PID: 1004 | Type: read</summary>
			public const int streamsbitrateondates = 1004;
			/// <summary>PID: 1005 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsbitrateondatesdata_1005 = 1005;
			/// <summary>PID: 1005 | Type: read</summary>
			public const int streamsbitrateondatesdata = 1005;
			/// <summary>PID: 1006 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsbitrateontimes_1006 = 1006;
			/// <summary>PID: 1006 | Type: read</summary>
			public const int streamsbitrateontimes = 1006;
			/// <summary>PID: 1007 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsbitrateontimesdata_1007 = 1007;
			/// <summary>PID: 1007 | Type: read</summary>
			public const int streamsbitrateontimesdata = 1007;
			public class Write
			{
			}
		}
		public class Idx
		{
			/// <summary>IDX: 0 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsindex_1001 = 0;
			/// <summary>IDX: 0 | Type: read</summary>
			public const int streamsindex = 0;
			/// <summary>IDX: 1 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsdrescription_1002 = 1;
			/// <summary>IDX: 1 | Type: read</summary>
			public const int streamsdrescription = 1;
			/// <summary>IDX: 2 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsoctetscounter_1003 = 2;
			/// <summary>IDX: 2 | Type: read</summary>
			public const int streamsoctetscounter = 2;
			/// <summary>IDX: 3 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsbitrateondates_1004 = 3;
			/// <summary>IDX: 3 | Type: read</summary>
			public const int streamsbitrateondates = 3;
			/// <summary>IDX: 4 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsbitrateondatesdata_1005 = 4;
			/// <summary>IDX: 4 | Type: read</summary>
			public const int streamsbitrateondatesdata = 4;
			/// <summary>IDX: 5 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsbitrateontimes_1006 = 5;
			/// <summary>IDX: 5 | Type: read</summary>
			public const int streamsbitrateontimes = 5;
			/// <summary>IDX: 6 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsbitrateontimesdata_1007 = 6;
			/// <summary>IDX: 6 | Type: read</summary>
			public const int streamsbitrateontimesdata = 6;
		}
	}
}
public class WriteParameters
{
	public SLProtocolExt Protocol;
	public WriteParameters(SLProtocolExt protocol)
	{
		Protocol = protocol;
	}
}
public interface SLProtocolExt : SLProtocol
{
	/// <summary>PID: 1000</summary>
	StreamsQActionTable streams { get; set; }
	object Afterstartup_dummy { get; set; }
	object Dummyforqa3_dummy { get; set; }
	object Streamsindex_1001 { get; set; }
	object Streamsindex { get; set; }
	object Streamsdrescription_1002 { get; set; }
	object Streamsdrescription { get; set; }
	object Streamsoctetscounter_1003 { get; set; }
	object Streamsoctetscounter { get; set; }
	object Streamsbitrateondates_1004 { get; set; }
	object Streamsbitrateondates { get; set; }
	object Streamsbitrateondatesdata_1005 { get; set; }
	object Streamsbitrateondatesdata { get; set; }
	object Streamsbitrateontimes_1006 { get; set; }
	object Streamsbitrateontimes { get; set; }
	object Streamsbitrateontimesdata_1007 { get; set; }
	object Streamsbitrateontimesdata { get; set; }
	WriteParameters Write { get; set; }
}
public class ConcreteSLProtocolExt : ConcreteSLProtocol, SLProtocolExt
{
	/// <summary>PID: 1000</summary>
	public StreamsQActionTable streams { get; set; }
	/// <summary>PID: 2  | Type: dummy</summary>
	public System.Object Afterstartup_dummy {get { return GetParameter(2); }set { SetParameter(2, value); }}
	/// <summary>PID: 3  | Type: dummy</summary>
	public System.Object Dummyforqa3_dummy {get { return GetParameter(3); }set { SetParameter(3, value); }}
	/// <summary>PID: 1001  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsindex_1001 {get { return GetParameter(1001); }set { SetParameter(1001, value); }}
	/// <summary>PID: 1001  | Type: read</summary>
	public System.Object Streamsindex {get { return GetParameter(1001); }set { SetParameter(1001, value); }}
	/// <summary>PID: 1002  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsdrescription_1002 {get { return GetParameter(1002); }set { SetParameter(1002, value); }}
	/// <summary>PID: 1002  | Type: read</summary>
	public System.Object Streamsdrescription {get { return GetParameter(1002); }set { SetParameter(1002, value); }}
	/// <summary>PID: 1003  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsoctetscounter_1003 {get { return GetParameter(1003); }set { SetParameter(1003, value); }}
	/// <summary>PID: 1003  | Type: read</summary>
	public System.Object Streamsoctetscounter {get { return GetParameter(1003); }set { SetParameter(1003, value); }}
	/// <summary>PID: 1004  | Type: read | EXCEPTIONS: N/A = -1</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsbitrateondates_1004 {get { return GetParameter(1004); }set { SetParameter(1004, value); }}
	/// <summary>PID: 1004  | Type: read | EXCEPTIONS: N/A = -1</summary>
	public System.Object Streamsbitrateondates {get { return GetParameter(1004); }set { SetParameter(1004, value); }}
	/// <summary>PID: 1005  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsbitrateondatesdata_1005 {get { return GetParameter(1005); }set { SetParameter(1005, value); }}
	/// <summary>PID: 1005  | Type: read</summary>
	public System.Object Streamsbitrateondatesdata {get { return GetParameter(1005); }set { SetParameter(1005, value); }}
	/// <summary>PID: 1006  | Type: read | EXCEPTIONS: N/A = -1</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsbitrateontimes_1006 {get { return GetParameter(1006); }set { SetParameter(1006, value); }}
	/// <summary>PID: 1006  | Type: read | EXCEPTIONS: N/A = -1</summary>
	public System.Object Streamsbitrateontimes {get { return GetParameter(1006); }set { SetParameter(1006, value); }}
	/// <summary>PID: 1007  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsbitrateontimesdata_1007 {get { return GetParameter(1007); }set { SetParameter(1007, value); }}
	/// <summary>PID: 1007  | Type: read</summary>
	public System.Object Streamsbitrateontimesdata {get { return GetParameter(1007); }set { SetParameter(1007, value); }}
	public WriteParameters Write { get; set; }
	public ConcreteSLProtocolExt()
	{
		streams = new StreamsQActionTable(this, 1000, "streams");
		Write = new WriteParameters(this);
	}
}
/// <summary>IDX: 0</summary>
public class StreamsQActionTable : QActionTable, IEnumerable<StreamsQActionRow>
{
	public StreamsQActionTable(SLProtocol protocol, int tableId, string tableName) : base(protocol, tableId, tableName) { }
	IEnumerator IEnumerable.GetEnumerator() { return (IEnumerator) GetEnumerator(); }
	public IEnumerator<StreamsQActionRow> GetEnumerator() { return new QActionTableEnumerator<StreamsQActionRow>(this); }
}
/// <summary>IDX: 0</summary>
public class StreamsQActionRow : QActionTableRow
{
	/// <summary>PID: 1001 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsindex_1001 { get { if (base.Columns.ContainsKey(0)) { return base.Columns[0]; } else { return null; } } set { if (base.Columns.ContainsKey(0)) { base.Columns[0] = value; } else { base.Columns.Add(0, value); } } }
	/// <summary>PID: 1001 | Type: read</summary>
	public System.Object Streamsindex { get { if (base.Columns.ContainsKey(0)) { return base.Columns[0]; } else { return null; } } set { if (base.Columns.ContainsKey(0)) { base.Columns[0] = value; } else { base.Columns.Add(0, value); } } }
	/// <summary>PID: 1002 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsdrescription_1002 { get { if (base.Columns.ContainsKey(1)) { return base.Columns[1]; } else { return null; } } set { if (base.Columns.ContainsKey(1)) { base.Columns[1] = value; } else { base.Columns.Add(1, value); } } }
	/// <summary>PID: 1002 | Type: read</summary>
	public System.Object Streamsdrescription { get { if (base.Columns.ContainsKey(1)) { return base.Columns[1]; } else { return null; } } set { if (base.Columns.ContainsKey(1)) { base.Columns[1] = value; } else { base.Columns.Add(1, value); } } }
	/// <summary>PID: 1003 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsoctetscounter_1003 { get { if (base.Columns.ContainsKey(2)) { return base.Columns[2]; } else { return null; } } set { if (base.Columns.ContainsKey(2)) { base.Columns[2] = value; } else { base.Columns.Add(2, value); } } }
	/// <summary>PID: 1003 | Type: read</summary>
	public System.Object Streamsoctetscounter { get { if (base.Columns.ContainsKey(2)) { return base.Columns[2]; } else { return null; } } set { if (base.Columns.ContainsKey(2)) { base.Columns[2] = value; } else { base.Columns.Add(2, value); } } }
	/// <summary>PID: 1004 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsbitrateondates_1004 { get { if (base.Columns.ContainsKey(3)) { return base.Columns[3]; } else { return null; } } set { if (base.Columns.ContainsKey(3)) { base.Columns[3] = value; } else { base.Columns.Add(3, value); } } }
	/// <summary>PID: 1004 | Type: read</summary>
	public System.Object Streamsbitrateondates { get { if (base.Columns.ContainsKey(3)) { return base.Columns[3]; } else { return null; } } set { if (base.Columns.ContainsKey(3)) { base.Columns[3] = value; } else { base.Columns.Add(3, value); } } }
	/// <summary>PID: 1005 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsbitrateondatesdata_1005 { get { if (base.Columns.ContainsKey(4)) { return base.Columns[4]; } else { return null; } } set { if (base.Columns.ContainsKey(4)) { base.Columns[4] = value; } else { base.Columns.Add(4, value); } } }
	/// <summary>PID: 1005 | Type: read</summary>
	public System.Object Streamsbitrateondatesdata { get { if (base.Columns.ContainsKey(4)) { return base.Columns[4]; } else { return null; } } set { if (base.Columns.ContainsKey(4)) { base.Columns[4] = value; } else { base.Columns.Add(4, value); } } }
	/// <summary>PID: 1006 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsbitrateontimes_1006 { get { if (base.Columns.ContainsKey(5)) { return base.Columns[5]; } else { return null; } } set { if (base.Columns.ContainsKey(5)) { base.Columns[5] = value; } else { base.Columns.Add(5, value); } } }
	/// <summary>PID: 1006 | Type: read</summary>
	public System.Object Streamsbitrateontimes { get { if (base.Columns.ContainsKey(5)) { return base.Columns[5]; } else { return null; } } set { if (base.Columns.ContainsKey(5)) { base.Columns[5] = value; } else { base.Columns.Add(5, value); } } }
	/// <summary>PID: 1007 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsbitrateontimesdata_1007 { get { if (base.Columns.ContainsKey(6)) { return base.Columns[6]; } else { return null; } } set { if (base.Columns.ContainsKey(6)) { base.Columns[6] = value; } else { base.Columns.Add(6, value); } } }
	/// <summary>PID: 1007 | Type: read</summary>
	public System.Object Streamsbitrateontimesdata { get { if (base.Columns.ContainsKey(6)) { return base.Columns[6]; } else { return null; } } set { if (base.Columns.ContainsKey(6)) { base.Columns[6] = value; } else { base.Columns.Add(6, value); } } }
	public StreamsQActionRow() : base(0, 7) { }
	public StreamsQActionRow(System.Object[] oRow) : base(0, 7, oRow) { }
	public static implicit operator StreamsQActionRow(System.Object[] source) { return new StreamsQActionRow(source); }
	public static implicit operator System.Object[](StreamsQActionRow source) { return source.ToObjectArray(); }
}
}
