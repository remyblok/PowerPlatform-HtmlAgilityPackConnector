// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// Website & Documentation: https://html-agility-pack.net
// Forum & Issues: https://github.com/zzzprojects/html-agility-pack
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// More projects: https://zzzprojects.com/
// Copyright � ZZZ Projects Inc. All rights reserved.
internal class EncodingFoundException : Exception
{
	private Encoding _encoding;
	internal EncodingFoundException(Encoding encoding)
	{
		_encoding = encoding;
	}
	internal Encoding Encoding
	{
		get { return _encoding; }
	}
}
[DebuggerDisplay("Name: {OriginalName}, Value: {Value}")]
public class HtmlAttribute : IComparable
{
	private int _line;
	internal int _lineposition;
	internal string _name;
	internal int _namelength;
	internal int _namestartindex;
	internal HtmlDocument _ownerdocument; // attribute can exists without a node
	internal HtmlNode _ownernode;
	private AttributeValueQuote _quoteType = AttributeValueQuote.DoubleQuote;
	internal int _streamposition;
	internal string _value;
	internal int _valuelength;
	internal int _valuestartindex; 
	internal bool _isFromParse;
	internal bool _hasEqual;
	private bool? _localUseOriginalName;
	internal HtmlAttribute(HtmlDocument ownerdocument)
	{
		_ownerdocument = ownerdocument;
	}
	public int Line
	{
		get { return _line; }
		internal set { _line = value; }
	}
	public int LinePosition
	{
		get { return _lineposition; }
	}
	public int ValueStartIndex
	{
		get { return _valuestartindex; }
	}
	public int ValueLength
	{
		get { return _valuelength; }
	}
	public bool UseOriginalName
	{
		get
		{
			var useOriginalName = false;
			if (this._localUseOriginalName.HasValue)
			{
				useOriginalName = this._localUseOriginalName.Value;
			}
			else if (this.OwnerDocument != null)
			{
				useOriginalName = this.OwnerDocument.OptionDefaultUseOriginalName;
			}
			return useOriginalName;
		}
		set
		{
			this._localUseOriginalName = value;
		}
	}
	public string Name
	{
		get
		{
			if (_name == null)
			{
				_name = _ownerdocument.Text.Substring(_namestartindex, _namelength);
			}
			return UseOriginalName ? _name : _name.ToLowerInvariant();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_name = value;
			if (_ownernode != null)
			{
				_ownernode.SetChanged();
			}
		}
	}
	public string OriginalName
	{
		get { return _name; }
	}
	public HtmlDocument OwnerDocument
	{
		get { return _ownerdocument; }
	}
	public HtmlNode OwnerNode
	{
		get { return _ownernode; }
	}
	public AttributeValueQuote QuoteType
	{
		get { return _quoteType; }
		set { _quoteType = value; }
	}
	internal AttributeValueQuote InternalQuoteType { get; set; }
	public int StreamPosition
	{
		get { return _streamposition; }
	}
	public string Value
	{
		get
		{
			if (_value == null && _ownerdocument.Text == null && _valuestartindex == 0 && _valuelength == 0)
			{
				return null;
			}
			if (_value == null)
			{
				_value = _ownerdocument.Text.Substring(_valuestartindex, _valuelength);
				if (!_ownerdocument.BackwardCompatibility)
				{
					_value = HtmlEntity.DeEntitize(_value);
				}
			}
			return _value;
		}
		set
		{
			_value = value;
			if (_ownernode != null)
			{
				_ownernode.SetChanged();
			}
		}
	}
	public string DeEntitizeValue
	{
		get { return HtmlEntity.DeEntitize(Value); }
	}
	internal string XmlName
	{
		get { return HtmlDocument.GetXmlName(Name, true, OwnerDocument.OptionPreserveXmlNamespaces); }
	}
	internal string XmlValue
	{
		get { return Value; }
	}
	public string XPath
	{
		get
		{
			string basePath = (OwnerNode == null) ? "/" : OwnerNode.XPath + "/";
			return basePath + GetRelativeXpath();
		}
	}
	public int CompareTo(object obj)
	{
		HtmlAttribute att = obj as HtmlAttribute;
		if (att == null)
		{
			throw new ArgumentException("obj");
		}
		return Name.CompareTo(att.Name);
	}
	public HtmlAttribute Clone()
	{
		HtmlAttribute att = new HtmlAttribute(_ownerdocument);
		att.Name = OriginalName;
		att.Value = Value;
		att.QuoteType = QuoteType;
		att.InternalQuoteType = InternalQuoteType;
		att._isFromParse = _isFromParse;
		att._hasEqual = _hasEqual;
		return att;
	}
	public void Remove()
	{
		_ownernode.Attributes.Remove(this);
	}
	private string GetRelativeXpath()
	{
		if (OwnerNode == null)
			return Name;
		int i = 1;
		foreach (HtmlAttribute node in OwnerNode.Attributes)
		{
			if (node.Name != Name) continue;
			if (node == this)
				break;
			i++;
		}
		return "@" + Name + "[" + i + "]";
	}
}
public enum AttributeValueQuote
{
	SingleQuote,
	DoubleQuote,
	None,
	WithoutValue,
	Initial
}
public class HtmlAttributeCollection : IList<HtmlAttribute>
{
	internal Dictionary<string, HtmlAttribute> Hashitems = new Dictionary<string, HtmlAttribute>(StringComparer.OrdinalIgnoreCase);
	private HtmlNode _ownernode;
	internal List<HtmlAttribute> items = new List<HtmlAttribute>();
	internal HtmlAttributeCollection(HtmlNode ownernode)
	{
		_ownernode = ownernode;
	}
	public int Count
	{
		get { return items.Count; }
	}
	public bool IsReadOnly
	{
		get { return false; }
	}
	public HtmlAttribute this[int index]
	{
		get { return items[index]; }
		set
		{
			var oldValue = items[index];
			items[index] = value;
			if (oldValue.Name != value.Name)
			{
				Hashitems.Remove(oldValue.Name);
			}
			Hashitems[value.Name] = value;
			value._ownernode = _ownernode;
			_ownernode.SetChanged();
		}
	}
	public HtmlAttribute this[string name]
	{
		get
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			HtmlAttribute value;
			return Hashitems.TryGetValue(name, out value) ? value : null;
		}
		set
		{
			HtmlAttribute currentValue;
			if (!Hashitems.TryGetValue(name, out currentValue))
			{
				Append(value);
			}
			else
			{
				this[items.IndexOf(currentValue)] = value;
			}
		}
	}
	public void Add(string name, string value)
	{
		Append(name, value);
	}
	public void Add(HtmlAttribute item)
	{
		Append(item);
	}
	public void AddRange(IEnumerable<HtmlAttribute> items)
	{
		foreach (var item in items)
		{ 
			Append(item);
		}
	}
	public void AddRange(Dictionary<string, string> items)
	{
		foreach (var item in items)
		{
			Add(item.Key, item.Value);
		}
	}
	void ICollection<HtmlAttribute>.Clear()
	{
		Clear();
	}
	public bool Contains(HtmlAttribute item)
	{
		return items.Contains(item);
	}
	public void CopyTo(HtmlAttribute[] array, int arrayIndex)
	{
		items.CopyTo(array, arrayIndex);
	}
	IEnumerator<HtmlAttribute> IEnumerable<HtmlAttribute>.GetEnumerator()
	{
		return items.GetEnumerator();
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		return items.GetEnumerator();
	}
	public int IndexOf(HtmlAttribute item)
	{
		return items.IndexOf(item);
	}
	public void Insert(int index, HtmlAttribute item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		Hashitems[item.Name] = item;
		item._ownernode = _ownernode;
		items.Insert(index, item);
		_ownernode.SetChanged();
	}
	bool ICollection<HtmlAttribute>.Remove(HtmlAttribute item)
	{
		if (item == null)
		{
			return false;
		}
		int index = GetAttributeIndex(item);
		if (index == -1)
		{
			return false;
		}
		RemoveAt(index);
		return true;
	}
	public void RemoveAt(int index)
	{
		HtmlAttribute att = items[index];
		Hashitems.Remove(att.Name);
		items.RemoveAt(index);
		_ownernode.SetChanged();
	}
	public HtmlAttribute Append(HtmlAttribute newAttribute)
	{
		if (_ownernode.NodeType == HtmlNodeType.Text || _ownernode.NodeType == HtmlNodeType.Comment)
		{
			throw new Exception("A Text or Comment node cannot have attributes.");
		}
		if (newAttribute == null)
		{
			throw new ArgumentNullException("newAttribute");
		}
		Hashitems[newAttribute.Name] = newAttribute;
		newAttribute._ownernode = _ownernode;
		items.Add(newAttribute);
		_ownernode.SetChanged();
		return newAttribute;
	}
	public HtmlAttribute Append(string name)
	{
		HtmlAttribute att = _ownernode._ownerdocument.CreateAttribute(name);
		return Append(att);
	}
	public HtmlAttribute Append(string name, string value)
	{
		HtmlAttribute att = _ownernode._ownerdocument.CreateAttribute(name, value);
		return Append(att);
	}
	public bool Contains(string name)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (String.Equals(items[i].Name, name, StringComparison.OrdinalIgnoreCase))
				return true;
		}
		return false;
	}
	public HtmlAttribute Prepend(HtmlAttribute newAttribute)
	{
		Insert(0, newAttribute);
		return newAttribute;
	}
	public void Remove(HtmlAttribute attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		int index = GetAttributeIndex(attribute);
		if (index == -1)
		{
			throw new IndexOutOfRangeException();
		}
		RemoveAt(index);
	}
	public void Remove(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		for (int i = items.Count - 1; i >= 0; i--)
		{
			HtmlAttribute att = items[i];
			if (String.Equals(att.Name, name, StringComparison.OrdinalIgnoreCase))
			{
				RemoveAt(i);
			}
		}
	}
	public void RemoveAll()
	{
		Hashitems.Clear();
		items.Clear();
		_ownernode.SetChanged();
	}
	public IEnumerable<HtmlAttribute> AttributesWithName(string attributeName)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (String.Equals(items[i].Name, attributeName, StringComparison.OrdinalIgnoreCase))
				yield return items[i];
		}
	}
	public void Remove()
	{
		items.Clear();
	}
	internal void Clear()
	{
		Hashitems.Clear();
		items.Clear();
	}
	internal int GetAttributeIndex(HtmlAttribute attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		for (int i = 0; i < items.Count; i++)
		{
			if ((items[i]) == attribute)
				return i;
		}
		return -1;
	}
	internal int GetAttributeIndex(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		for (int i = 0; i < items.Count; i++)
		{
			if (String.Equals((items[i]).Name, name, StringComparison.OrdinalIgnoreCase))
				return i;
		}
		return -1;
	}
}
public class HtmlCommentNode : HtmlNode
{
	private string _comment;
	internal HtmlCommentNode(HtmlDocument ownerdocument, int index)
		:
		base(HtmlNodeType.Comment, ownerdocument, index)
	{
	}
	public string Comment
	{
		get
		{
			if (_comment == null)
			{
				return base.InnerHtml;
			}
			return _comment;
		}
		set { _comment = value; }
	}
	public override string InnerHtml
	{
		get
		{
			if (_comment == null)
			{
				return base.InnerHtml;
			}
			return _comment;
		}
		set { _comment = value; }
	}
	public override string OuterHtml
	{
		get
		{
			if (_comment == null)
			{
				return base.OuterHtml;
			}
			if(_comment.StartsWith("<!--") && _comment.EndsWith("-->"))
			{
				return _comment;
			}
			return "<!--" + _comment + "-->";
		}
	}
}
public partial class HtmlDocument
{
	internal static bool _disableBehaviorTagP = true;
	public static bool DisableBehaviorTagP
	{
		get => _disableBehaviorTagP;
		set
		{
			if (value)
			{
				if (HtmlNode.ElementsFlags.ContainsKey("p"))
				{
					HtmlNode.ElementsFlags.Remove("p");
				}
			}
			else
			{
				if (!HtmlNode.ElementsFlags.ContainsKey("p"))
				{
					HtmlNode.ElementsFlags.Add("p", HtmlElementFlag.Empty | HtmlElementFlag.Closed);
				}
			}
			_disableBehaviorTagP = value;
		}
	}
	public static Action<HtmlDocument> DefaultBuilder { get; set; }
	public Action<HtmlDocument> ParseExecuting { get; set; }
	private static int _maxDepthLevel = int.MaxValue;
	private int _c;
	private Crc32 _crc32;
	private HtmlAttribute _currentattribute;
	private HtmlNode _currentnode;
	private Encoding _declaredencoding;
	private HtmlNode _documentnode;
	private bool _fullcomment;
	private int _index;
	internal Dictionary<string, HtmlNode> Lastnodes = new Dictionary<string, HtmlNode>();
	private HtmlNode _lastparentnode;
	private int _line;
	private int _lineposition, _maxlineposition;
	internal Dictionary<string, HtmlNode> Nodesid;
	private ParseState _oldstate;
	private bool _onlyDetectEncoding;
	internal Dictionary<int, HtmlNode> Openednodes;
	private List<HtmlParseError> _parseerrors = new List<HtmlParseError>();
	private string _remainder;
	private int _remainderOffset;
	private ParseState _state;
	private Encoding _streamencoding;
	private bool _useHtmlEncodingForStream;
	public string Text;
	public bool BackwardCompatibility = true;
	public bool OptionAddDebuggingAttributes;
	public bool OptionAutoCloseOnEnd; // close errors at the end
	public bool OptionCheckSyntax = true;
	public bool OptionComputeChecksum;
	public bool OptionEmptyCollection = false;
	public bool DisableServerSideCode = false;
	public Encoding OptionDefaultStreamEncoding;
	public bool OptionXmlForceOriginalComment;
	public bool OptionExtractErrorSourceText;
	public int OptionExtractErrorSourceTextMaxLength = 100;
	public bool OptionFixNestedTags; // fix li, tr, th, td tags
	public bool OptionOutputAsXml;
	public bool DisableImplicitEnd;
	public bool OptionPreserveXmlNamespaces;
	public bool OptionOutputOptimizeAttributeValues;
	public AttributeValueQuote? GlobalAttributeValueQuote;
	public bool OptionOutputOriginalCase;
	public bool OptionOutputUpperCase;
	public bool OptionReadEncoding = true;
	public string OptionStopperNodeName;
	public bool OptionDefaultUseOriginalName;
	public bool OptionUseIdAttribute = true;
	public bool OptionWriteEmptyNodes;
	public int OptionMaxNestedChildNodes = 0;
	internal static readonly string HtmlExceptionRefNotChild = "Reference node must be a child of this node";
	internal static readonly string HtmlExceptionUseIdAttributeFalse = "You need to set UseIdAttribute property to true to enable this feature";
	internal static readonly string HtmlExceptionClassDoesNotExist = "Class name doesn't exist";
	internal static readonly string HtmlExceptionClassExists = "Class name already exists";
	internal static readonly Dictionary<string, string[]> HtmlResetters = new Dictionary<string, string[]>()
	{
		{"li", new[] {"ul", "ol"}},
		{"tr", new[] {"table"}},
		{"th", new[] {"tr", "table"}},
		{"td", new[] {"tr", "table"}},
	};
	public HtmlDocument()
	{
		if (DefaultBuilder != null)
		{
			DefaultBuilder(this);
		}
		_documentnode = CreateNode(HtmlNodeType.Document, 0);
#if SILVERLIGHT || METRO || NETSTANDARD1_3 || NETSTANDARD1_6
		OptionDefaultStreamEncoding = Encoding.UTF8;
#else
		OptionDefaultStreamEncoding = Encoding.Default;
#endif
	}
	public string ParsedText
	{
		get { return Text; }
	}
	public static int MaxDepthLevel
	{
		get { return _maxDepthLevel; }
		set { _maxDepthLevel = value; }
	}
	public int CheckSum
	{
		get { return _crc32 == null ? 0 : (int) _crc32.CheckSum; }
	}
	public Encoding DeclaredEncoding
	{
		get { return _declaredencoding; }
	}
	public HtmlNode DocumentNode
	{
		get { return _documentnode; }
	}
	public Encoding Encoding
	{
		get { return GetOutEncoding(); }
	}
	public IEnumerable<HtmlParseError> ParseErrors
	{
		get { return _parseerrors; }
	}
	public string Remainder
	{
		get { return _remainder; }
	}
	public int RemainderOffset
	{
		get { return _remainderOffset; }
	}
	public Encoding StreamEncoding
	{
		get { return _streamencoding; }
	}
	public static string GetXmlName(string name)
	{
		return GetXmlName(name, false, false);
	}
#if !METRO
	public void UseAttributeOriginalName(string tagName)
	{
		foreach (var nod in this.DocumentNode.SelectNodes("//" + tagName))
		{
			foreach (var attribut in nod.Attributes)
			{
				attribut.UseOriginalName = true;
			}
		}
	}
#endif
	public static string GetXmlName(string name, bool isAttribute, bool preserveXmlNamespaces)
	{
		string xmlname = string.Empty;
		bool nameisok = true;
		for (int i = 0; i < name.Length; i++)
		{
			if (((name[i] >= 'a') && (name[i] <= 'z')) ||
				((name[i] >= 'A') && (name[i] <= 'Z')) ||
				((name[i] >= '0') && (name[i] <= '9')) ||
				((isAttribute || preserveXmlNamespaces) && name[i] == ':') ||
				(name[i] == '_') || (name[i] == '-') || (name[i] == '.'))
			{
				xmlname += name[i];
			}
			else
			{
				nameisok = false;
				byte[] bytes = Encoding.UTF8.GetBytes(new char[] {name[i]});
				for (int j = 0; j < bytes.Length; j++)
				{
					xmlname += bytes[j].ToString("x2");
				}
				xmlname += "_";
			}
		}
		if (nameisok)
		{
			return xmlname;
		}
		return "_" + xmlname;
	}
	public static string HtmlEncode(string html)
	{
		return HtmlEncodeWithCompatibility(html, true);
	}
	internal static string HtmlEncodeWithCompatibility(string html, bool backwardCompatibility = true)
	{
		if (html == null)
		{
			throw new ArgumentNullException("html");
		}
		Regex rx = backwardCompatibility ? new Regex("&(?!(amp;)|(lt;)|(gt;)|(quot;))", RegexOptions.IgnoreCase) : new Regex("&(?!(amp;)|(lt;)|(gt;)|(quot;)|(nbsp;)|(reg;))", RegexOptions.IgnoreCase);
		return rx.Replace(html, "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
	}
	public static bool IsWhiteSpace(int c)
	{
		if ((c == 10) || (c == 13) || (c == 32) || (c == 9))
		{
			return true;
		}
		return false;
	}
	public HtmlAttribute CreateAttribute(string name)
	{
		if (name == null)
			throw new ArgumentNullException("name");
		HtmlAttribute att = CreateAttribute();
		att.Name = name;
		return att;
	}
	public HtmlAttribute CreateAttribute(string name, string value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		HtmlAttribute att = CreateAttribute(name);
		att.Value = value;
		return att;
	}
	public HtmlCommentNode CreateComment()
	{
		return (HtmlCommentNode) CreateNode(HtmlNodeType.Comment);
	}
	public HtmlCommentNode CreateComment(string comment)
	{
		if (comment == null)
		{
			throw new ArgumentNullException("comment");
		}
		if (!comment.StartsWith("<!--") && !comment.EndsWith("-->"))
		{
			comment = "<!--" + comment + "-->";
		}
		HtmlCommentNode c = CreateComment();
		c.Comment = comment;
		return c;
	}
	public HtmlNode CreateElement(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		HtmlNode node = CreateNode(HtmlNodeType.Element);
		node.SetName(name);
		return node;
	}
	public HtmlTextNode CreateTextNode()
	{
		return (HtmlTextNode) CreateNode(HtmlNodeType.Text);
	}
	public HtmlTextNode CreateTextNode(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		HtmlTextNode t = CreateTextNode();
		t.Text = text;
		return t;
	}
	public Encoding DetectEncoding(Stream stream)
	{
		return DetectEncoding(stream, false);
	}
	public Encoding DetectEncoding(Stream stream, bool checkHtml)
	{
		_useHtmlEncodingForStream = checkHtml;
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return DetectEncoding(new StreamReader(stream));
	}
	public Encoding DetectEncoding(TextReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		_onlyDetectEncoding = true;
		if (OptionCheckSyntax)
		{
			Openednodes = new Dictionary<int, HtmlNode>();
		}
		else
		{
			Openednodes = null;
		}
		if (OptionUseIdAttribute)
		{
			Nodesid = new Dictionary<string, HtmlNode>(StringComparer.OrdinalIgnoreCase);
		}
		else
		{
			Nodesid = null;
		}
		StreamReader sr = reader as StreamReader;
		if (sr != null && !_useHtmlEncodingForStream)
		{
			Text = sr.ReadToEnd();
			_streamencoding = sr.CurrentEncoding;
			return _streamencoding;
		}
		_streamencoding = null;
		_declaredencoding = null;
		Text = reader.ReadToEnd();
		_documentnode = CreateNode(HtmlNodeType.Document, 0);
		try
		{
			Parse();
		}
		catch (EncodingFoundException ex)
		{
			return ex.Encoding;
		}
		return _streamencoding;
	}
	public Encoding DetectEncodingHtml(string html)
	{
		if (html == null)
		{
			throw new ArgumentNullException("html");
		}
		using (StringReader sr = new StringReader(html))
		{
			Encoding encoding = DetectEncoding(sr);
			return encoding;
		}
	}
	public HtmlNode GetElementbyId(string id)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (Nodesid == null)
		{
			throw new Exception(HtmlExceptionUseIdAttributeFalse);
		}
		return Nodesid.ContainsKey(id) ? Nodesid[id] : null;
	}
	public void Load(Stream stream)
	{
		Load(new StreamReader(stream, OptionDefaultStreamEncoding));
	}
	public void Load(Stream stream, bool detectEncodingFromByteOrderMarks)
	{
		Load(new StreamReader(stream, detectEncodingFromByteOrderMarks));
	}
	public void Load(Stream stream, Encoding encoding)
	{
		Load(new StreamReader(stream, encoding));
	}
	public void Load(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
	{
		Load(new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks));
	}
	public void Load(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
	{
		Load(new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks, buffersize));
	}
	public void Load(TextReader reader)
	{
		if (reader == null)
			throw new ArgumentNullException("reader");
		_onlyDetectEncoding = false;
		if (OptionCheckSyntax)
			Openednodes = new Dictionary<int, HtmlNode>();
		else
			Openednodes = null;
		if (OptionUseIdAttribute)
		{
			Nodesid = new Dictionary<string, HtmlNode>(StringComparer.OrdinalIgnoreCase);
		}
		else
		{
			Nodesid = null;
		}
		StreamReader sr = reader as StreamReader;
		if (sr != null)
		{
			try
			{
				sr.Peek();
			}
			catch (Exception)
			{
			}
			_streamencoding = sr.CurrentEncoding;
		}
		else
		{
			_streamencoding = null;
		}
		_declaredencoding = null;
		Text = reader.ReadToEnd();
		_documentnode = CreateNode(HtmlNodeType.Document, 0);
		Parse();
		if (!OptionCheckSyntax || Openednodes == null) return;
		foreach (HtmlNode node in Openednodes.Values)
		{
			if (!node._starttag) // already reported
			{
				continue;
			}
			string html;
			if (OptionExtractErrorSourceText)
			{
				html = node.OuterHtml;
				if (html.Length > OptionExtractErrorSourceTextMaxLength)
				{
					html = html.Substring(0, OptionExtractErrorSourceTextMaxLength);
				}
			}
			else
			{
				html = string.Empty;
			}
			AddError(
				HtmlParseErrorCode.TagNotClosed,
				node._line, node._lineposition,
				node._streamposition, html,
				"End tag </" + node.Name + "> was not found");
		}
		Openednodes.Clear();
	}
	public void LoadHtml(string html)
	{
		if (html == null)
		{
			throw new ArgumentNullException("html");
		}
		using (StringReader sr = new StringReader(html))
		{
			Load(sr);
		}
	}
	public void Save(Stream outStream)
	{
		StreamWriter sw = new StreamWriter(outStream, GetOutEncoding());
		Save(sw);
	}
	public void Save(Stream outStream, Encoding encoding)
	{
		if (outStream == null)
		{
			throw new ArgumentNullException("outStream");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		StreamWriter sw = new StreamWriter(outStream, encoding);
		Save(sw);
	}
	public void Save(StreamWriter writer)
	{
		Save((TextWriter) writer);
	}
	public void Save(TextWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		DocumentNode.WriteTo(writer);
		writer.Flush();
	}
	public void Save(XmlWriter writer)
	{
		DocumentNode.WriteTo(writer);
		writer.Flush();
	}
	internal HtmlAttribute CreateAttribute()
	{
		return new HtmlAttribute(this);
	}
	internal HtmlNode CreateNode(HtmlNodeType type)
	{
		return CreateNode(type, -1);
	}
	internal HtmlNode CreateNode(HtmlNodeType type, int index)
	{
		switch (type)
		{
			case HtmlNodeType.Comment:
				return new HtmlCommentNode(this, index);
			case HtmlNodeType.Text:
				return new HtmlTextNode(this, index);
			default:
				return new HtmlNode(type, this, index);
		}
	}
	internal Encoding GetOutEncoding()
	{
		return _declaredencoding ?? (_streamencoding ?? OptionDefaultStreamEncoding);
	}
	internal HtmlNode GetXmlDeclaration()
	{
		if (!_documentnode.HasChildNodes)
			return null;
		foreach (HtmlNode node in _documentnode._childnodes)
			if (node.Name == "?xml") // it's ok, names are case sensitive
				return node;
		return null;
	}
	internal void SetIdForNode(HtmlNode node, string id)
	{
		if (!OptionUseIdAttribute)
			return;
		if ((Nodesid == null) || (id == null))
			return;
		if (node == null)
			Nodesid.Remove(id);
		else
			Nodesid[id] = node;
	}
	internal void UpdateLastParentNode()
	{
		do
		{
			if (_lastparentnode.Closed)
				_lastparentnode = _lastparentnode.ParentNode;
		} while ((_lastparentnode != null) && (_lastparentnode.Closed));
		if (_lastparentnode == null)
			_lastparentnode = _documentnode;
	}
	private void AddError(HtmlParseErrorCode code, int line, int linePosition, int streamPosition, string sourceText, string reason)
	{
		HtmlParseError err = new HtmlParseError(code, line, linePosition, streamPosition, sourceText, reason);
		_parseerrors.Add(err);
		return;
	}
	private void CloseCurrentNode()
	{
		if (_currentnode.Closed) // text or document are by def closed
			return;
		bool error = false;
		HtmlNode prev = Utilities.GetDictionaryValueOrDefault(Lastnodes, _currentnode.Name);
		if (prev == null)
		{
			if (HtmlNode.IsClosedElement(_currentnode.Name))
			{
				_currentnode.CloseNode(_currentnode);
				if (_lastparentnode != null)
				{
					HtmlNode foundNode = null;
					Stack<HtmlNode> futureChild = new Stack<HtmlNode>();
					if (!_currentnode.Name.Equals("br"))
					{
						for (HtmlNode node = _lastparentnode.LastChild; node != null; node = node.PreviousSibling)
						{
							if ((node.Name == _currentnode.Name) && (!node.HasChildNodes))
							{
								foundNode = node;
								break;
							}
							futureChild.Push(node);
						}
					}
					if (foundNode != null)
					{
						while (futureChild.Count != 0)
						{
							HtmlNode node = futureChild.Pop();
							_lastparentnode.RemoveChild(node);
							foundNode.AppendChild(node);
						}
					}
					else
					{
						_lastparentnode.AppendChild(_currentnode);
					}
				}
			}
			else
			{
				if (HtmlNode.CanOverlapElement(_currentnode.Name))
				{
					HtmlNode closenode = CreateNode(HtmlNodeType.Text, _currentnode._outerstartindex);
					closenode._outerlength = _currentnode._outerlength;
					((HtmlTextNode) closenode).Text = ((HtmlTextNode) closenode).Text.ToLowerInvariant();
					if (_lastparentnode != null)
					{
						_lastparentnode.AppendChild(closenode);
					}
				}
				else
				{
					if (HtmlNode.IsEmptyElement(_currentnode.Name))
					{
						AddError(
							HtmlParseErrorCode.EndTagNotRequired,
							_currentnode._line, _currentnode._lineposition,
							_currentnode._streamposition, _currentnode.OuterHtml,
							"End tag </" + _currentnode.Name + "> is not required");
					}
					else
					{
						AddError(
							HtmlParseErrorCode.TagNotOpened,
							_currentnode._line, _currentnode._lineposition,
							_currentnode._streamposition, _currentnode.OuterHtml,
							"Start tag <" + _currentnode.Name + "> was not found");
						error = true;
					}
				}
			}
		}
		else
		{
			if (OptionFixNestedTags)
			{
				if (FindResetterNodes(prev, GetResetters(_currentnode.Name)))
				{
					AddError(
						HtmlParseErrorCode.EndTagInvalidHere,
						_currentnode._line, _currentnode._lineposition,
						_currentnode._streamposition, _currentnode.OuterHtml,
						"End tag </" + _currentnode.Name + "> invalid here");
					error = true;
				}
			}
			if (!error)
			{
				Lastnodes[_currentnode.Name] = prev._prevwithsamename;
				prev.CloseNode(_currentnode);
			}
		}
		if (!error)
		{
			if ((_lastparentnode != null) &&
				((!HtmlNode.IsClosedElement(_currentnode.Name)) ||
				 (_currentnode._starttag)))
			{
				UpdateLastParentNode();
			}
		}
	}
	private string CurrentNodeName()
	{
		return Text.Substring(_currentnode._namestartindex, _currentnode._namelength);
	}
	private void DecrementPosition()
	{
		_index--;
		if (_lineposition == 0)
		{
			_lineposition = _maxlineposition;
			_line--;
		}
		else
		{
			_lineposition--;
		}
	}
	private HtmlNode FindResetterNode(HtmlNode node, string name)
	{
		HtmlNode resetter = Utilities.GetDictionaryValueOrDefault(Lastnodes, name);
		if (resetter == null)
			return null;
		if (resetter.Closed)
			return null;
		if (resetter._streamposition < node._streamposition)
		{
			return null;
		}
		return resetter;
	}
	private bool FindResetterNodes(HtmlNode node, string[] names)
	{
		if (names == null)
			return false;
		for (int i = 0; i < names.Length; i++)
		{
			if (FindResetterNode(node, names[i]) != null)
				return true;
		}
		return false;
	}
	private void FixNestedTag(string name, string[] resetters)
	{
		if (resetters == null)
			return;
		HtmlNode prev = Utilities.GetDictionaryValueOrDefault(Lastnodes, _currentnode.Name);
		if (prev == null || (Lastnodes[name].Closed)) return;
		if (FindResetterNodes(prev, resetters))
		{
			return;
		}
		HtmlNode close = new HtmlNode(prev.NodeType, this, -1);
		close._endnode = close;
		prev.CloseNode(close);
	}
	private void FixNestedTags()
	{
		if (!_currentnode._starttag)
			return;
		string name = CurrentNodeName();
		FixNestedTag(name, GetResetters(name));
	}
	private string[] GetResetters(string name)
	{
		string[] resetters;
		if (!HtmlResetters.TryGetValue(name, out resetters))
		{
			return null;
		}
		return resetters;
	}
	private void IncrementPosition()
	{
		if (_crc32 != null)
		{
			_crc32.AddToCRC32(_c);
		}
		_index++;
		_maxlineposition = _lineposition;
		if (_c == 10)
		{
			_lineposition = 0;
			_line++;
		}
		else
		{
			_lineposition++;
		}
	}
	private bool IsValidTag()
	{
		bool isValidTag = _c == '<' && _index < Text.Length && (Char.IsLetter(Text[_index]) || Text[_index] == '/' || Text[_index] == '?' || Text[_index] == '!' || Text[_index] == '%');
		return isValidTag;
	}
	private bool NewCheck()
	{
		if (_c != '<' || !IsValidTag())
		{
			return false;
		}
		if (_index < Text.Length)
		{
			if (Text[_index] == '%')
			{
				if (DisableServerSideCode)
				{
					return false;
				}
				switch (_state)
				{
					case ParseState.AttributeAfterEquals:
						PushAttributeValueStart(_index - 1);
						break;
					case ParseState.BetweenAttributes:
						PushAttributeNameStart(_index - 1, _lineposition -1);
						break;
					case ParseState.WhichTag:
						PushNodeNameStart(true, _index - 1);
						_state = ParseState.Tag;
						break;
				}
				_oldstate = _state;
				_state = ParseState.ServerSideCode;
				return true;
			}
		}
		if (!PushNodeEnd(_index - 1, true))
		{
			_index = Text.Length;
			return true;
		}
		_state = ParseState.WhichTag;
		if ((_index - 1) <= (Text.Length - 2))
		{
			if (Text[_index] == '!' || Text[_index] == '?')
			{
				PushNodeStart(HtmlNodeType.Comment, _index - 1, _lineposition -1);
				PushNodeNameStart(true, _index);
				PushNodeNameEnd(_index + 1);
				_state = ParseState.Comment;
				if (_index < (Text.Length - 2))
				{
					if ((Text[_index + 1] == '-') &&
						(Text[_index + 2] == '-'))
					{
						_fullcomment = true;
					}
					else
					{
						_fullcomment = false;
					}
				}
				return true;
			}
		}
		PushNodeStart(HtmlNodeType.Element, _index - 1,  _lineposition - 1);
		return true;
	}
	private void Parse()
	{
		if (ParseExecuting != null)
		{
			ParseExecuting(this);
		}
		int lastquote = 0;
		if (OptionComputeChecksum)
		{
			_crc32 = new Crc32();
		}
		Lastnodes = new Dictionary<string, HtmlNode>();
		_c = 0;
		_fullcomment = false;
		_parseerrors = new List<HtmlParseError>();
		_line = 1;
		_lineposition = 0;
		_maxlineposition = 0;
		_state = ParseState.Text;
		_oldstate = _state;
		_documentnode._innerlength = Text.Length;
		_documentnode._outerlength = Text.Length;
		_remainderOffset = Text.Length;
		_lastparentnode = _documentnode;
		_currentnode = CreateNode(HtmlNodeType.Text, 0);
		_currentattribute = null;
		_index = 0;
		PushNodeStart(HtmlNodeType.Text, 0, _lineposition);
		while (_index < Text.Length)
		{
			_c = Text[_index];
			IncrementPosition();
			switch (_state)
			{
				case ParseState.Text:
					if (NewCheck())
						continue;
					break;
				case ParseState.WhichTag:
					if (NewCheck())
						continue;
					if (_c == '/')
					{
						PushNodeNameStart(false, _index);
					}
					else
					{
						PushNodeNameStart(true, _index - 1);
						DecrementPosition();
					}
					_state = ParseState.Tag;
					break;
				case ParseState.Tag:
					if (NewCheck())
						continue;
					if (IsWhiteSpace(_c))
					{
						CloseParentImplicitExplicitNode();
						PushNodeNameEnd(_index - 1);
						if (_state != ParseState.Tag)
							continue;
						_state = ParseState.BetweenAttributes;
						continue;
					}
					if (_c == '/')
					{
						CloseParentImplicitExplicitNode();
						PushNodeNameEnd(_index - 1);
						if (_state != ParseState.Tag)
							continue;
						_state = ParseState.EmptyTag;
						continue;
					}
					if (_c == '>')
					{
						CloseParentImplicitExplicitNode();
						PushNodeNameEnd(_index - 1);
						if (_state != ParseState.Tag)
							continue;
						if (!PushNodeEnd(_index, false))
						{
							_index = Text.Length;
							break;
						}
						if (_state != ParseState.Tag)
							continue;
						_state = ParseState.Text;
						PushNodeStart(HtmlNodeType.Text, _index, _lineposition);
					}
					break;
				case ParseState.BetweenAttributes:
					if (NewCheck())
						continue;
					if (IsWhiteSpace(_c))
						continue;
					if ((_c == '/') || (_c == '?'))
					{
						_state = ParseState.EmptyTag;
						continue;
					}
					if (_c == '>')
					{
						if (!PushNodeEnd(_index, false))
						{
							_index = Text.Length;
							break;
						}
						if (_state != ParseState.BetweenAttributes)
							continue;
						_state = ParseState.Text;
						PushNodeStart(HtmlNodeType.Text, _index, _lineposition);
						continue;
					}
					PushAttributeNameStart(_index - 1, _lineposition -1);
					_state = ParseState.AttributeName;
					break;
				case ParseState.EmptyTag:
					if (NewCheck())
						continue;
					if (_c == '>')
					{
						if (!PushNodeEnd(_index, true))
						{
							_index = Text.Length;
							break;
						}
						if (_state != ParseState.EmptyTag)
							continue;
						_state = ParseState.Text;
						PushNodeStart(HtmlNodeType.Text, _index, _lineposition);
						continue;
					}
					if (!IsWhiteSpace(_c))
					{
						DecrementPosition();
						_state = ParseState.BetweenAttributes;
						continue;
					}
					else
					{
						_state = ParseState.BetweenAttributes;
					}
					break;
				case ParseState.AttributeName:
					if (NewCheck())
						continue;
					_currentattribute._isFromParse = true;
					if (_c == '/')
					{
						PushAttributeNameEnd(_index - 1);
						_state = ParseState.AttributeBeforeEquals;
						continue;
					}
					if (IsWhiteSpace(_c))
					{
						PushAttributeNameEnd(_index - 1);
						_state = ParseState.AttributeBeforeEquals;
						continue;
					}
					if (_c == '=')
					{
						PushAttributeNameEnd(_index - 1);
						_currentattribute._hasEqual = true;
						_state = ParseState.AttributeAfterEquals;
						continue;
					}
					if (_c == '>')
					{
						PushAttributeNameEnd(_index - 1);
						if (!PushNodeEnd(_index, false))
						{
							_index = Text.Length;
							break;
						}
						if (_state != ParseState.AttributeName)
							continue;
						_state = ParseState.Text;
						PushNodeStart(HtmlNodeType.Text, _index, _lineposition);
						continue;
					}
					break;
				case ParseState.AttributeBeforeEquals:
					if (NewCheck())
						continue;
					if (IsWhiteSpace(_c))
						continue;
					if (_c == '>')
					{
						if (!PushNodeEnd(_index, false))
						{
							_index = Text.Length;
							break;
						}
						if (_state != ParseState.AttributeBeforeEquals)
							continue;
						_state = ParseState.Text;
						PushNodeStart(HtmlNodeType.Text, _index, _lineposition);
						continue;
					}
					if (_c == '=')
					{
						_currentattribute._hasEqual = true;
						_state = ParseState.AttributeAfterEquals;
						continue;
					}
					_state = ParseState.BetweenAttributes;
					DecrementPosition();
					break;
				case ParseState.AttributeAfterEquals:
					if (NewCheck())
						continue;
					if (IsWhiteSpace(_c))
						continue;
					if ((_c == '\'') || (_c == '"'))
					{
						_state = ParseState.QuotedAttributeValue;
						PushAttributeValueStart(_index, _c);
						lastquote = _c;
						continue;
					}
					if (_c == '>')
					{
						if (!PushNodeEnd(_index, false))
						{
							_index = Text.Length;
							break;
						}
						if (_state != ParseState.AttributeAfterEquals)
							continue;
						_state = ParseState.Text;
						PushNodeStart(HtmlNodeType.Text, _index, _lineposition);
						continue;
					}
					PushAttributeValueStart(_index - 1);
					_state = ParseState.AttributeValue;
					break;
				case ParseState.AttributeValue:
					if (NewCheck())
						continue;
					if (IsWhiteSpace(_c))
					{
						PushAttributeValueEnd(_index - 1);
						_state = ParseState.BetweenAttributes;
						continue;
					}
					if (_c == '>')
					{
						PushAttributeValueEnd(_index - 1);
						if (!PushNodeEnd(_index, false))
						{
							_index = Text.Length;
							break;
						}
						if (_state != ParseState.AttributeValue)
							continue;
						_state = ParseState.Text;
						PushNodeStart(HtmlNodeType.Text, _index, _lineposition);
						continue;
					}
					break;
				case ParseState.QuotedAttributeValue:
					if (_c == lastquote)
					{
						PushAttributeValueEnd(_index - 1);
						_state = ParseState.BetweenAttributes;
						continue;
					}
					if (_c == '<')
					{
						if (_index < Text.Length)
						{
							if (Text[_index] == '%')
							{
								_oldstate = _state;
								_state = ParseState.ServerSideCode;
								continue;
							}
						}
					}
					break;
				case ParseState.Comment:
					if (_c == '>')
					{
						if (_fullcomment)
						{
							if (((Text[_index - 2] != '-') || (Text[_index - 3] != '-')) 
								&&  
								((Text[_index - 2] != '!') || (Text[_index - 3] != '-') ||
								 (Text[_index - 4] != '-')))
							{
								continue;
							}
						}
						if (!PushNodeEnd(_index, false))
						{
							_index = Text.Length;
							break;
						}
						_state = ParseState.Text;
						PushNodeStart(HtmlNodeType.Text, _index, _lineposition);
						continue;
					}
					break;
				case ParseState.ServerSideCode:
					if (_c == '%')
					{
						if (_index < Text.Length)
						{
							if (Text[_index] == '>')
							{
								switch (_oldstate)
								{
									case ParseState.AttributeAfterEquals:
										_state = ParseState.AttributeValue;
										break;
									case ParseState.BetweenAttributes:
										PushAttributeNameEnd(_index + 1);
										_state = ParseState.BetweenAttributes;
										break;
									default:
										_state = _oldstate;
										break;
								}
								IncrementPosition();
							}
						}
					}
					else if (_oldstate == ParseState.QuotedAttributeValue
							 && _c == lastquote)
					{
						_state = _oldstate;
						DecrementPosition();
					}
					break;
				case ParseState.PcData:
					if ((_currentnode._namelength + 3) <= (Text.Length - (_index - 1)))
					{
						var tagStartMatching = Text[_index - 1] == '<' && Text[_index] == '/';
						var tagMatching = tagStartMatching && string.Compare(
								Text,
								_index + 1,
								_currentnode.Name,
								0,
								_currentnode._namelength,
								StringComparison.OrdinalIgnoreCase) == 0;
						if (tagMatching)
						{
							int c = Text[_index - 1 + 2 + _currentnode.Name.Length];
							if ((c == '>') || (IsWhiteSpace(c)))
							{
								HtmlNode script = CreateNode(HtmlNodeType.Text,
									_currentnode._outerstartindex +
									_currentnode._outerlength);
								script._outerlength = _index - 1 - script._outerstartindex;
								script._streamposition = script._outerstartindex;
								script._line = _currentnode.Line;
								script._lineposition = _currentnode.LinePosition + _currentnode._namelength + 2;
								_currentnode.AppendChild(script);
								if (_currentnode.Name.ToLowerInvariant().Equals("script")  || _currentnode.Name.ToLowerInvariant().Equals("style"))
								{
									_currentnode._isHideInnerText = true;
								}
								PushNodeStart(HtmlNodeType.Element, _index - 1, _lineposition -1);
								PushNodeNameStart(false, _index - 1 + 2);
								_state = ParseState.Tag;
								IncrementPosition();
							}
						}
					}
					break;
			}
		}
		if (_currentnode._namestartindex > 0)
		{
			PushNodeNameEnd(_index);
		}
		PushNodeEnd(_index, false);
		Lastnodes.Clear();
	}
	private static List<string> BlockAttributes = new List<string>() { "\"", "'" };
	private void PushAttributeNameEnd(int index)
	{
		_currentattribute._namelength = index - _currentattribute._namestartindex;
		if (_currentattribute.Name != null && !BlockAttributes.Contains(_currentattribute.Name))
		{
			_currentnode.Attributes.Append(_currentattribute);
		}
	}
	private void PushAttributeNameStart(int index, int lineposition)
	{
		_currentattribute = CreateAttribute();
		_currentattribute._namestartindex = index;
		_currentattribute.Line = _line;
		_currentattribute._lineposition = lineposition;
		_currentattribute._streamposition = index;
	}
	private void PushAttributeValueEnd(int index)
	{
		_currentattribute._valuelength = index - _currentattribute._valuestartindex;
	}
	private void PushAttributeValueStart(int index)
	{
		PushAttributeValueStart(index, 0);
	}
	private void CloseParentImplicitExplicitNode()
	{
		bool hasNodeToClose = true;
		while(hasNodeToClose && !_lastparentnode.Closed)
		{
			hasNodeToClose = false;
			bool forceExplicitEnd = false;
			if (IsParentImplicitEnd())
			{
				if (OptionOutputAsXml || DisableImplicitEnd)
				{
					forceExplicitEnd = true;
				}
				else
				{
					CloseParentImplicitEnd();
					hasNodeToClose = true;
				}
			}
			if (forceExplicitEnd || IsParentExplicitEnd())
			{
				CloseParentExplicitEnd();
				hasNodeToClose = true;
			}
		}		   
	}
	private bool IsParentImplicitEnd()
	{
		if (!_currentnode._starttag) return false;
		bool isImplicitEnd = false;
		var parent = _lastparentnode.Name;
		var nodeName = Text.Substring(_currentnode._namestartindex, _index - _currentnode._namestartindex - 1).ToLowerInvariant();
		switch (parent)
		{
			case "a":
				isImplicitEnd = nodeName == "a";
				break;
			case "dd":
				isImplicitEnd = nodeName == "dt" || nodeName == "dd";
				break;
			case "dt":
				isImplicitEnd = nodeName == "dt" || nodeName == "dd";
				break;
			case "li":
				isImplicitEnd = nodeName == "li";
				break;
			case "p":
				if (DisableBehaviorTagP)
				{
					isImplicitEnd = nodeName == "address"
									|| nodeName == "article"
									|| nodeName == "aside"
									|| nodeName == "blockquote"
									|| nodeName == "dir"
									|| nodeName == "div"
									|| nodeName == "dl"
									|| nodeName == "fieldset"
									|| nodeName == "footer"
									|| nodeName == "form"
									|| nodeName == "h1"
									|| nodeName == "h2"
									|| nodeName == "h3"
									|| nodeName == "h4"
									|| nodeName == "h5"
									|| nodeName == "h6"
									|| nodeName == "header"
									|| nodeName == "hr"
									|| nodeName == "li"
									|| nodeName == "menu"
									|| nodeName == "nav"
									|| nodeName == "ol"
									|| nodeName == "p"
									|| nodeName == "pre"
									|| nodeName == "section"
									|| nodeName == "table"
									|| nodeName == "ul";
				}
				else
				{
					isImplicitEnd = nodeName == "p";
				}
				break;
			case "option":
				isImplicitEnd = nodeName == "option";
				break;
		}
		return isImplicitEnd;
	}
	private bool IsParentExplicitEnd()
	{
		if (!_currentnode._starttag) return false;
		bool isExplicitEnd = false;
		var parent = _lastparentnode.Name;
		var nodeName = Text.Substring(_currentnode._namestartindex, _index - _currentnode._namestartindex - 1).ToLowerInvariant();
		switch (parent)
		{
			case "title":
				isExplicitEnd = nodeName == "title";
				break;
			case "p":
				isExplicitEnd = nodeName == "div";
				break;
			case "table":
				isExplicitEnd = nodeName == "table";
				break;
			case "tr":
				isExplicitEnd = nodeName == "tr" || nodeName == "tbody";
				break;
			case "thead":
				isExplicitEnd = nodeName == "tbody";
				break;
			case "tbody":
				isExplicitEnd = nodeName == "tbody";
				break;
			case "td":
				isExplicitEnd = nodeName == "td" || nodeName == "th" || nodeName == "tr" || nodeName == "tbody";
				break;
			case "th":
				isExplicitEnd = nodeName == "td" || nodeName == "th" || nodeName == "tr" || nodeName == "tbody";
				break;
			case "h1":
				isExplicitEnd = nodeName == "h2" || nodeName == "h3" || nodeName == "h4" || nodeName == "h5";
				break;
			case "h2":
				isExplicitEnd = nodeName == "h1" || nodeName == "h3" || nodeName == "h4" || nodeName == "h5";
				break;
			case "h3":
				isExplicitEnd = nodeName == "h1" || nodeName == "h2" || nodeName == "h4" || nodeName == "h5";
				break;
			case "h4":
				isExplicitEnd = nodeName == "h1" || nodeName == "h2" || nodeName == "h3" || nodeName == "h5";
				break;
			case "h5":
				isExplicitEnd = nodeName == "h1" || nodeName == "h2" || nodeName == "h3" || nodeName == "h4";
				break;
		}
		return isExplicitEnd;
	}
	private void CloseParentImplicitEnd()
	{
		HtmlNode close = new HtmlNode(_lastparentnode.NodeType, this, -1);
		close._endnode = close;
		close._isImplicitEnd = true;
		_lastparentnode._isImplicitEnd = true;
		_lastparentnode.CloseNode(close);
	}
	private void CloseParentExplicitEnd()
	{
		HtmlNode close = new HtmlNode(_lastparentnode.NodeType, this, -1);
		close._endnode = close;
		_lastparentnode.CloseNode(close);
	}
	private void PushAttributeValueStart(int index, int quote)
	{
		_currentattribute._valuestartindex = index;
		if (quote == '\'')
		{
			_currentattribute.QuoteType = AttributeValueQuote.SingleQuote;
		}
		_currentattribute.InternalQuoteType = _currentattribute.QuoteType;
		if (quote == 0)
		{
			_currentattribute.InternalQuoteType = AttributeValueQuote.None;
		}
	}
	private bool PushNodeEnd(int index, bool close)
	{
		_currentnode._outerlength = index - _currentnode._outerstartindex;
		if ((_currentnode._nodetype == HtmlNodeType.Text) ||
			(_currentnode._nodetype == HtmlNodeType.Comment))
		{
			if (_currentnode._outerlength > 0)
			{
				_currentnode._innerlength = _currentnode._outerlength;
				_currentnode._innerstartindex = _currentnode._outerstartindex;
				if (_lastparentnode != null)
				{
					_lastparentnode.AppendChild(_currentnode);
				}
			}
		}
		else
		{
			if ((_currentnode._starttag) && (_lastparentnode != _currentnode))
			{
				if (_lastparentnode != null)
				{
					_lastparentnode.AppendChild(_currentnode);
				}
				ReadDocumentEncoding(_currentnode);
				HtmlNode prev = Utilities.GetDictionaryValueOrDefault(Lastnodes, _currentnode.Name);
				_currentnode._prevwithsamename = prev;
				Lastnodes[_currentnode.Name] = _currentnode;
				if ((_currentnode.NodeType == HtmlNodeType.Document) ||
					(_currentnode.NodeType == HtmlNodeType.Element))
				{
					_lastparentnode = _currentnode;
				}
				if (HtmlNode.IsCDataElement(CurrentNodeName()))
				{
					_state = ParseState.PcData;
					return true;
				}
				if ((HtmlNode.IsClosedElement(_currentnode.Name)) ||
					(HtmlNode.IsEmptyElement(_currentnode.Name)))
				{
					close = true;
				}
			}
		}
		if ((close) || (!_currentnode._starttag))
		{
			if ((OptionStopperNodeName != null) && (_remainder == null) &&
				(string.Compare(_currentnode.Name, OptionStopperNodeName, StringComparison.OrdinalIgnoreCase) == 0))
			{
				_remainderOffset = index;
				_remainder = Text.Substring(_remainderOffset);
				CloseCurrentNode();
				return false; // stop parsing
			}
			CloseCurrentNode();
		}
		return true;
	}
	private void PushNodeNameEnd(int index)
	{
		_currentnode._namelength = index - _currentnode._namestartindex;
		if (OptionFixNestedTags)
		{
			FixNestedTags();
		}
	}
	private void PushNodeNameStart(bool starttag, int index)
	{
		_currentnode._starttag = starttag;
		_currentnode._namestartindex = index;
	}
	private void PushNodeStart(HtmlNodeType type, int index, int lineposition)
	{
		_currentnode = CreateNode(type, index);
		_currentnode._line = _line;
		_currentnode._lineposition = lineposition;
		_currentnode._streamposition = index;
	}
	private void ReadDocumentEncoding(HtmlNode node)
	{
		if (!OptionReadEncoding)
			return;
		if (node._namelength != 4) // quick check, avoids string alloc
			return;
		if (node.Name != "meta") // all nodes names are lowercase
			return;
		string charset = null;
		HtmlAttribute att = node.Attributes["http-equiv"];
		if (att != null)
		{
			if (string.Compare(att.Value, "content-type", StringComparison.OrdinalIgnoreCase) != 0)
				return;
			HtmlAttribute content = node.Attributes["content"];
			if (content != null)
				charset = NameValuePairList.GetNameValuePairsValue(content.Value, "charset");
		}
		else
		{
			att = node.Attributes["charset"];
			if (att != null)
				charset = att.Value;
		}
		if (!string.IsNullOrEmpty(charset))
		{
			if (string.Equals(charset, "utf8", StringComparison.OrdinalIgnoreCase))
				charset = "utf-8";
			try
			{
				_declaredencoding = Encoding.GetEncoding(charset);
			}
			catch (ArgumentException)
			{
				_declaredencoding = null;
			}
			if (_onlyDetectEncoding)
			{
				throw new EncodingFoundException(_declaredencoding);
			}
			if (_streamencoding != null)
			{
#if SILVERLIGHT || PocketPC || METRO || NETSTANDARD1_3 || NETSTANDARD1_6
				if (_declaredencoding.WebName != _streamencoding.WebName)
#else
				if (_declaredencoding != null)
					if (_declaredencoding.CodePage != _streamencoding.CodePage)
#endif
					{
						AddError(
							HtmlParseErrorCode.CharsetMismatch,
							_line, _lineposition,
							_index, node.OuterHtml,
							"Encoding mismatch between StreamEncoding: " +
							_streamencoding.WebName + " and DeclaredEncoding: " +
							_declaredencoding.WebName);
					}
			}
		}
	}
	private enum ParseState
	{
		Text,
		WhichTag,
		Tag,
		BetweenAttributes,
		EmptyTag,
		AttributeName,
		AttributeBeforeEquals,
		AttributeAfterEquals,
		AttributeValue,
		Comment,
		QuotedAttributeValue,
		ServerSideCode,
		PcData
	}
}
public partial class HtmlDocument
{
	public void DetectEncodingAndLoad(string path)
	{
		DetectEncodingAndLoad(path, true);
	}
	public void DetectEncodingAndLoad(string path, bool detectEncoding)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		Encoding enc;
		if (detectEncoding)
		{
			enc = DetectEncoding(path);
		}
		else
		{
			enc = null;
		}
		if (enc == null)
		{
			Load(path);
		}
		else
		{
			Load(path, enc);
		}
	}
	public Encoding DetectEncoding(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
#if NETSTANDARD1_3 || NETSTANDARD1_6
		using (StreamReader sr = new StreamReader(File.OpenRead(path), OptionDefaultStreamEncoding))
#else
		using (StreamReader sr = new StreamReader(path, OptionDefaultStreamEncoding))
#endif
		{
			Encoding encoding = DetectEncoding(sr);
			return encoding;
		}
	}
	public void Load(string path)
	{
		if (path == null)
			throw new ArgumentNullException("path");
#if NETSTANDARD1_3 || NETSTANDARD1_6
		using (StreamReader sr = new StreamReader(File.OpenRead(path), OptionDefaultStreamEncoding))
#else
		using (StreamReader sr = new StreamReader(path, OptionDefaultStreamEncoding))
#endif
		{
			Load(sr);
		}
	}
	public void Load(string path, bool detectEncodingFromByteOrderMarks)
	{
		if (path == null)
			throw new ArgumentNullException("path");
#if NETSTANDARD1_3 || NETSTANDARD1_6
		using (StreamReader sr = new StreamReader(File.OpenRead(path), detectEncodingFromByteOrderMarks))
#else
		using (StreamReader sr = new StreamReader(path, detectEncodingFromByteOrderMarks))
#endif
		{
			Load(sr);
		}
	}
	public void Load(string path, Encoding encoding)
	{
		if (path == null)
			throw new ArgumentNullException("path");
		if (encoding == null)
			throw new ArgumentNullException("encoding");
#if NETSTANDARD1_3 || NETSTANDARD1_6
		using (StreamReader sr = new StreamReader(File.OpenRead(path), encoding))
#else
		using (StreamReader sr = new StreamReader(path, encoding))
#endif
		{
			Load(sr);
		}
	}
	public void Load(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
	{
		if (path == null)
			throw new ArgumentNullException("path");
		if (encoding == null)
			throw new ArgumentNullException("encoding");
#if NETSTANDARD1_3 || NETSTANDARD1_6
		using (StreamReader sr = new StreamReader(File.OpenRead(path), encoding, detectEncodingFromByteOrderMarks))
#else
		using (StreamReader sr = new StreamReader(path, encoding, detectEncodingFromByteOrderMarks))
#endif
		{
			Load(sr);
		}
	}
	public void Load(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
	{
		if (path == null)
			throw new ArgumentNullException("path");
		if (encoding == null)
			throw new ArgumentNullException("encoding");
#if NETSTANDARD1_3 || NETSTANDARD1_6
		using (StreamReader sr = new StreamReader(File.OpenRead(path), encoding, detectEncodingFromByteOrderMarks, buffersize))
#else
		using (StreamReader sr = new StreamReader(path, encoding, detectEncodingFromByteOrderMarks, buffersize))
#endif
		{
			Load(sr);
		}
	}
	public void Save(string filename)
	{
#if NETSTANDARD1_3 || NETSTANDARD1_6
		using (StreamWriter sw = new StreamWriter(File.OpenWrite(filename), GetOutEncoding()))
#else
		using (StreamWriter sw = new StreamWriter(filename, false, GetOutEncoding()))
#endif
		{
			Save(sw);
		}
	}
	public void Save(string filename, Encoding encoding)
	{
		if (filename == null)
		{
			throw new ArgumentNullException("filename");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
#if NETSTANDARD1_3 || NETSTANDARD1_6
		using (StreamWriter sw = new StreamWriter(File.OpenWrite(filename), encoding))
#else
		using (StreamWriter sw = new StreamWriter(filename, false, encoding))
#endif
		{
			Save(sw);
		}
	}
}
public partial class HtmlDocument : IXPathNavigable
{
	public XPathNavigator CreateNavigator()
	{
		return new HtmlNodeNavigator(this, _documentnode);
	}
}
[Flags]
public enum HtmlElementFlag
{
	CData = 1,
	Empty = 2,
	Closed = 4,
	CanOverlap = 8
}
public class HtmlEntity
{
#if !FX20 && !FX35
	public static bool UseWebUtility { get; set; }
#endif
	private static readonly int _maxEntitySize;
	private static Dictionary<int, string> _entityName;
	private static Dictionary<string, int> _entityValue;
	public static Dictionary<int, string> EntityName
	{
		get { return _entityName; }
	}
	public static Dictionary<string, int> EntityValue
	{
		get { return _entityValue; }
	}
	static HtmlEntity()
	{
		_entityName = new Dictionary<int, string>();
		_entityValue = new Dictionary<string, int>();
		_entityValue.Add("quot", 34); // quotation mark = APL quote, U+0022 ISOnum 
		_entityName.Add(34, "quot");
		_entityValue.Add("amp", 38); // ampersand, U+0026 ISOnum 
		_entityName.Add(38, "amp");
		_entityValue.Add("apos", 39); // apostrophe-quote	 U+0027 (39)
		_entityName.Add(39, "apos");
		_entityValue.Add("lt", 60); // less-than sign, U+003C ISOnum 
		_entityName.Add(60, "lt");
		_entityValue.Add("gt", 62); // greater-than sign, U+003E ISOnum 
		_entityName.Add(62, "gt");
		_entityValue.Add("nbsp", 160); // no-break space = non-breaking space, U+00A0 ISOnum 
		_entityName.Add(160, "nbsp");
		_entityValue.Add("iexcl", 161); // inverted exclamation mark, U+00A1 ISOnum 
		_entityName.Add(161, "iexcl");
		_entityValue.Add("cent", 162); // cent sign, U+00A2 ISOnum 
		_entityName.Add(162, "cent");
		_entityValue.Add("pound", 163); // pound sign, U+00A3 ISOnum 
		_entityName.Add(163, "pound");
		_entityValue.Add("curren", 164); // currency sign, U+00A4 ISOnum 
		_entityName.Add(164, "curren");
		_entityValue.Add("yen", 165); // yen sign = yuan sign, U+00A5 ISOnum 
		_entityName.Add(165, "yen");
		_entityValue.Add("brvbar", 166); // broken bar = broken vertical bar, U+00A6 ISOnum 
		_entityName.Add(166, "brvbar");
		_entityValue.Add("sect", 167); // section sign, U+00A7 ISOnum 
		_entityName.Add(167, "sect");
		_entityValue.Add("uml", 168); // diaeresis = spacing diaeresis, U+00A8 ISOdia 
		_entityName.Add(168, "uml");
		_entityValue.Add("copy", 169); // copyright sign, U+00A9 ISOnum 
		_entityName.Add(169, "copy");
		_entityValue.Add("ordf", 170); // feminine ordinal indicator, U+00AA ISOnum 
		_entityName.Add(170, "ordf");
		_entityValue.Add("laquo", 171);
		_entityName.Add(171, "laquo");
		_entityValue.Add("not", 172); // not sign, U+00AC ISOnum 
		_entityName.Add(172, "not");
		_entityValue.Add("shy", 173); // soft hyphen = discretionary hyphen, U+00AD ISOnum 
		_entityName.Add(173, "shy");
		_entityValue.Add("reg", 174); // registered sign = registered trade mark sign, U+00AE ISOnum 
		_entityName.Add(174, "reg");
		_entityValue.Add("macr", 175); // macron = spacing macron = overline = APL overbar, U+00AF ISOdia 
		_entityName.Add(175, "macr");
		_entityValue.Add("deg", 176); // degree sign, U+00B0 ISOnum 
		_entityName.Add(176, "deg");
		_entityValue.Add("plusmn", 177); // plus-minus sign = plus-or-minus sign, U+00B1 ISOnum 
		_entityName.Add(177, "plusmn");
		_entityValue.Add("sup2", 178); // superscript two = superscript digit two = squared, U+00B2 ISOnum 
		_entityName.Add(178, "sup2");
		_entityValue.Add("sup3", 179); // superscript three = superscript digit three = cubed, U+00B3 ISOnum 
		_entityName.Add(179, "sup3");
		_entityValue.Add("acute", 180); // acute accent = spacing acute, U+00B4 ISOdia 
		_entityName.Add(180, "acute");
		_entityValue.Add("micro", 181); // micro sign, U+00B5 ISOnum 
		_entityName.Add(181, "micro");
		_entityValue.Add("para", 182); // pilcrow sign = paragraph sign, U+00B6 ISOnum 
		_entityName.Add(182, "para");
		_entityValue.Add("middot", 183); // middle dot = Georgian comma = Greek middle dot, U+00B7 ISOnum 
		_entityName.Add(183, "middot");
		_entityValue.Add("cedil", 184); // cedilla = spacing cedilla, U+00B8 ISOdia 
		_entityName.Add(184, "cedil");
		_entityValue.Add("sup1", 185); // superscript one = superscript digit one, U+00B9 ISOnum 
		_entityName.Add(185, "sup1");
		_entityValue.Add("ordm", 186); // masculine ordinal indicator, U+00BA ISOnum 
		_entityName.Add(186, "ordm");
		_entityValue.Add("raquo", 187);
		_entityName.Add(187, "raquo");
		_entityValue.Add("frac14", 188); // vulgar fraction one quarter = fraction one quarter, U+00BC ISOnum 
		_entityName.Add(188, "frac14");
		_entityValue.Add("frac12", 189); // vulgar fraction one half = fraction one half, U+00BD ISOnum 
		_entityName.Add(189, "frac12");
		_entityValue.Add("frac34", 190); // vulgar fraction three quarters = fraction three quarters, U+00BE ISOnum 
		_entityName.Add(190, "frac34");
		_entityValue.Add("iquest", 191); // inverted question mark = turned question mark, U+00BF ISOnum 
		_entityName.Add(191, "iquest");
		_entityValue.Add("Agrave", 192);
		_entityName.Add(192, "Agrave");
		_entityValue.Add("Aacute", 193); // latin capital letter A with acute, U+00C1 ISOlat1 
		_entityName.Add(193, "Aacute");
		_entityValue.Add("Acirc", 194); // latin capital letter A with circumflex, U+00C2 ISOlat1 
		_entityName.Add(194, "Acirc");
		_entityValue.Add("Atilde", 195); // latin capital letter A with tilde, U+00C3 ISOlat1 
		_entityName.Add(195, "Atilde");
		_entityValue.Add("Auml", 196); // latin capital letter A with diaeresis, U+00C4 ISOlat1 
		_entityName.Add(196, "Auml");
		_entityValue.Add("Aring", 197);
		_entityName.Add(197, "Aring");
		_entityValue.Add("AElig", 198); // latin capital letter AE = latin capital ligature AE, U+00C6 ISOlat1 
		_entityName.Add(198, "AElig");
		_entityValue.Add("Ccedil", 199); // latin capital letter C with cedilla, U+00C7 ISOlat1 
		_entityName.Add(199, "Ccedil");
		_entityValue.Add("Egrave", 200); // latin capital letter E with grave, U+00C8 ISOlat1 
		_entityName.Add(200, "Egrave");
		_entityValue.Add("Eacute", 201); // latin capital letter E with acute, U+00C9 ISOlat1 
		_entityName.Add(201, "Eacute");
		_entityValue.Add("Ecirc", 202); // latin capital letter E with circumflex, U+00CA ISOlat1 
		_entityName.Add(202, "Ecirc");
		_entityValue.Add("Euml", 203); // latin capital letter E with diaeresis, U+00CB ISOlat1 
		_entityName.Add(203, "Euml");
		_entityValue.Add("Igrave", 204); // latin capital letter I with grave, U+00CC ISOlat1 
		_entityName.Add(204, "Igrave");
		_entityValue.Add("Iacute", 205); // latin capital letter I with acute, U+00CD ISOlat1 
		_entityName.Add(205, "Iacute");
		_entityValue.Add("Icirc", 206); // latin capital letter I with circumflex, U+00CE ISOlat1 
		_entityName.Add(206, "Icirc");
		_entityValue.Add("Iuml", 207); // latin capital letter I with diaeresis, U+00CF ISOlat1 
		_entityName.Add(207, "Iuml");
		_entityValue.Add("ETH", 208); // latin capital letter ETH, U+00D0 ISOlat1 
		_entityName.Add(208, "ETH");
		_entityValue.Add("Ntilde", 209); // latin capital letter N with tilde, U+00D1 ISOlat1 
		_entityName.Add(209, "Ntilde");
		_entityValue.Add("Ograve", 210); // latin capital letter O with grave, U+00D2 ISOlat1 
		_entityName.Add(210, "Ograve");
		_entityValue.Add("Oacute", 211); // latin capital letter O with acute, U+00D3 ISOlat1 
		_entityName.Add(211, "Oacute");
		_entityValue.Add("Ocirc", 212); // latin capital letter O with circumflex, U+00D4 ISOlat1 
		_entityName.Add(212, "Ocirc");
		_entityValue.Add("Otilde", 213); // latin capital letter O with tilde, U+00D5 ISOlat1 
		_entityName.Add(213, "Otilde");
		_entityValue.Add("Ouml", 214); // latin capital letter O with diaeresis, U+00D6 ISOlat1 
		_entityName.Add(214, "Ouml");
		_entityValue.Add("times", 215); // multiplication sign, U+00D7 ISOnum 
		_entityName.Add(215, "times");
		_entityValue.Add("Oslash", 216);
		_entityName.Add(216, "Oslash");
		_entityValue.Add("Ugrave", 217); // latin capital letter U with grave, U+00D9 ISOlat1 
		_entityName.Add(217, "Ugrave");
		_entityValue.Add("Uacute", 218); // latin capital letter U with acute, U+00DA ISOlat1 
		_entityName.Add(218, "Uacute");
		_entityValue.Add("Ucirc", 219); // latin capital letter U with circumflex, U+00DB ISOlat1 
		_entityName.Add(219, "Ucirc");
		_entityValue.Add("Uuml", 220); // latin capital letter U with diaeresis, U+00DC ISOlat1 
		_entityName.Add(220, "Uuml");
		_entityValue.Add("Yacute", 221); // latin capital letter Y with acute, U+00DD ISOlat1 
		_entityName.Add(221, "Yacute");
		_entityValue.Add("THORN", 222); // latin capital letter THORN, U+00DE ISOlat1 
		_entityName.Add(222, "THORN");
		_entityValue.Add("szlig", 223); // latin small letter sharp s = ess-zed, U+00DF ISOlat1 
		_entityName.Add(223, "szlig");
		_entityValue.Add("agrave", 224);
		_entityName.Add(224, "agrave");
		_entityValue.Add("aacute", 225); // latin small letter a with acute, U+00E1 ISOlat1 
		_entityName.Add(225, "aacute");
		_entityValue.Add("acirc", 226); // latin small letter a with circumflex, U+00E2 ISOlat1 
		_entityName.Add(226, "acirc");
		_entityValue.Add("atilde", 227); // latin small letter a with tilde, U+00E3 ISOlat1 
		_entityName.Add(227, "atilde");
		_entityValue.Add("auml", 228); // latin small letter a with diaeresis, U+00E4 ISOlat1 
		_entityName.Add(228, "auml");
		_entityValue.Add("aring", 229);
		_entityName.Add(229, "aring");
		_entityValue.Add("aelig", 230); // latin small letter ae = latin small ligature ae, U+00E6 ISOlat1 
		_entityName.Add(230, "aelig");
		_entityValue.Add("ccedil", 231); // latin small letter c with cedilla, U+00E7 ISOlat1 
		_entityName.Add(231, "ccedil");
		_entityValue.Add("egrave", 232); // latin small letter e with grave, U+00E8 ISOlat1 
		_entityName.Add(232, "egrave");
		_entityValue.Add("eacute", 233); // latin small letter e with acute, U+00E9 ISOlat1 
		_entityName.Add(233, "eacute");
		_entityValue.Add("ecirc", 234); // latin small letter e with circumflex, U+00EA ISOlat1 
		_entityName.Add(234, "ecirc");
		_entityValue.Add("euml", 235); // latin small letter e with diaeresis, U+00EB ISOlat1 
		_entityName.Add(235, "euml");
		_entityValue.Add("igrave", 236); // latin small letter i with grave, U+00EC ISOlat1 
		_entityName.Add(236, "igrave");
		_entityValue.Add("iacute", 237); // latin small letter i with acute, U+00ED ISOlat1 
		_entityName.Add(237, "iacute");
		_entityValue.Add("icirc", 238); // latin small letter i with circumflex, U+00EE ISOlat1 
		_entityName.Add(238, "icirc");
		_entityValue.Add("iuml", 239); // latin small letter i with diaeresis, U+00EF ISOlat1 
		_entityName.Add(239, "iuml");
		_entityValue.Add("eth", 240); // latin small letter eth, U+00F0 ISOlat1 
		_entityName.Add(240, "eth");
		_entityValue.Add("ntilde", 241); // latin small letter n with tilde, U+00F1 ISOlat1 
		_entityName.Add(241, "ntilde");
		_entityValue.Add("ograve", 242); // latin small letter o with grave, U+00F2 ISOlat1 
		_entityName.Add(242, "ograve");
		_entityValue.Add("oacute", 243); // latin small letter o with acute, U+00F3 ISOlat1 
		_entityName.Add(243, "oacute");
		_entityValue.Add("ocirc", 244); // latin small letter o with circumflex, U+00F4 ISOlat1 
		_entityName.Add(244, "ocirc");
		_entityValue.Add("otilde", 245); // latin small letter o with tilde, U+00F5 ISOlat1 
		_entityName.Add(245, "otilde");
		_entityValue.Add("ouml", 246); // latin small letter o with diaeresis, U+00F6 ISOlat1 
		_entityName.Add(246, "ouml");
		_entityValue.Add("divide", 247); // division sign, U+00F7 ISOnum 
		_entityName.Add(247, "divide");
		_entityValue.Add("oslash", 248);
		_entityName.Add(248, "oslash");
		_entityValue.Add("ugrave", 249); // latin small letter u with grave, U+00F9 ISOlat1 
		_entityName.Add(249, "ugrave");
		_entityValue.Add("uacute", 250); // latin small letter u with acute, U+00FA ISOlat1 
		_entityName.Add(250, "uacute");
		_entityValue.Add("ucirc", 251); // latin small letter u with circumflex, U+00FB ISOlat1 
		_entityName.Add(251, "ucirc");
		_entityValue.Add("uuml", 252); // latin small letter u with diaeresis, U+00FC ISOlat1 
		_entityName.Add(252, "uuml");
		_entityValue.Add("yacute", 253); // latin small letter y with acute, U+00FD ISOlat1 
		_entityName.Add(253, "yacute");
		_entityValue.Add("thorn", 254); // latin small letter thorn, U+00FE ISOlat1 
		_entityName.Add(254, "thorn");
		_entityValue.Add("yuml", 255); // latin small letter y with diaeresis, U+00FF ISOlat1 
		_entityName.Add(255, "yuml");
		_entityValue.Add("fnof", 402); // latin small f with hook = function = florin, U+0192 ISOtech 
		_entityName.Add(402, "fnof");
		_entityValue.Add("Alpha", 913); // greek capital letter alpha, U+0391 
		_entityName.Add(913, "Alpha");
		_entityValue.Add("Beta", 914); // greek capital letter beta, U+0392 
		_entityName.Add(914, "Beta");
		_entityValue.Add("Gamma", 915); // greek capital letter gamma, U+0393 ISOgrk3 
		_entityName.Add(915, "Gamma");
		_entityValue.Add("Delta", 916); // greek capital letter delta, U+0394 ISOgrk3 
		_entityName.Add(916, "Delta");
		_entityValue.Add("Epsilon", 917); // greek capital letter epsilon, U+0395 
		_entityName.Add(917, "Epsilon");
		_entityValue.Add("Zeta", 918); // greek capital letter zeta, U+0396 
		_entityName.Add(918, "Zeta");
		_entityValue.Add("Eta", 919); // greek capital letter eta, U+0397 
		_entityName.Add(919, "Eta");
		_entityValue.Add("Theta", 920); // greek capital letter theta, U+0398 ISOgrk3 
		_entityName.Add(920, "Theta");
		_entityValue.Add("Iota", 921); // greek capital letter iota, U+0399 
		_entityName.Add(921, "Iota");
		_entityValue.Add("Kappa", 922); // greek capital letter kappa, U+039A 
		_entityName.Add(922, "Kappa");
		_entityValue.Add("Lambda", 923); // greek capital letter lambda, U+039B ISOgrk3 
		_entityName.Add(923, "Lambda");
		_entityValue.Add("Mu", 924); // greek capital letter mu, U+039C 
		_entityName.Add(924, "Mu");
		_entityValue.Add("Nu", 925); // greek capital letter nu, U+039D 
		_entityName.Add(925, "Nu");
		_entityValue.Add("Xi", 926); // greek capital letter xi, U+039E ISOgrk3 
		_entityName.Add(926, "Xi");
		_entityValue.Add("Omicron", 927); // greek capital letter omicron, U+039F 
		_entityName.Add(927, "Omicron");
		_entityValue.Add("Pi", 928); // greek capital letter pi, U+03A0 ISOgrk3 
		_entityName.Add(928, "Pi");
		_entityValue.Add("Rho", 929); // greek capital letter rho, U+03A1 
		_entityName.Add(929, "Rho");
		_entityValue.Add("Sigma", 931); // greek capital letter sigma, U+03A3 ISOgrk3 
		_entityName.Add(931, "Sigma");
		_entityValue.Add("Tau", 932); // greek capital letter tau, U+03A4 
		_entityName.Add(932, "Tau");
		_entityValue.Add("Upsilon", 933); // greek capital letter upsilon, U+03A5 ISOgrk3 
		_entityName.Add(933, "Upsilon");
		_entityValue.Add("Phi", 934); // greek capital letter phi, U+03A6 ISOgrk3 
		_entityName.Add(934, "Phi");
		_entityValue.Add("Chi", 935); // greek capital letter chi, U+03A7 
		_entityName.Add(935, "Chi");
		_entityValue.Add("Psi", 936); // greek capital letter psi, U+03A8 ISOgrk3 
		_entityName.Add(936, "Psi");
		_entityValue.Add("Omega", 937); // greek capital letter omega, U+03A9 ISOgrk3 
		_entityName.Add(937, "Omega");
		_entityValue.Add("alpha", 945); // greek small letter alpha, U+03B1 ISOgrk3 
		_entityName.Add(945, "alpha");
		_entityValue.Add("beta", 946); // greek small letter beta, U+03B2 ISOgrk3 
		_entityName.Add(946, "beta");
		_entityValue.Add("gamma", 947); // greek small letter gamma, U+03B3 ISOgrk3 
		_entityName.Add(947, "gamma");
		_entityValue.Add("delta", 948); // greek small letter delta, U+03B4 ISOgrk3 
		_entityName.Add(948, "delta");
		_entityValue.Add("epsilon", 949); // greek small letter epsilon, U+03B5 ISOgrk3 
		_entityName.Add(949, "epsilon");
		_entityValue.Add("zeta", 950); // greek small letter zeta, U+03B6 ISOgrk3 
		_entityName.Add(950, "zeta");
		_entityValue.Add("eta", 951); // greek small letter eta, U+03B7 ISOgrk3 
		_entityName.Add(951, "eta");
		_entityValue.Add("theta", 952); // greek small letter theta, U+03B8 ISOgrk3 
		_entityName.Add(952, "theta");
		_entityValue.Add("iota", 953); // greek small letter iota, U+03B9 ISOgrk3 
		_entityName.Add(953, "iota");
		_entityValue.Add("kappa", 954); // greek small letter kappa, U+03BA ISOgrk3 
		_entityName.Add(954, "kappa");
		_entityValue.Add("lambda", 955); // greek small letter lambda, U+03BB ISOgrk3 
		_entityName.Add(955, "lambda");
		_entityValue.Add("mu", 956); // greek small letter mu, U+03BC ISOgrk3 
		_entityName.Add(956, "mu");
		_entityValue.Add("nu", 957); // greek small letter nu, U+03BD ISOgrk3 
		_entityName.Add(957, "nu");
		_entityValue.Add("xi", 958); // greek small letter xi, U+03BE ISOgrk3 
		_entityName.Add(958, "xi");
		_entityValue.Add("omicron", 959); // greek small letter omicron, U+03BF NEW 
		_entityName.Add(959, "omicron");
		_entityValue.Add("pi", 960); // greek small letter pi, U+03C0 ISOgrk3 
		_entityName.Add(960, "pi");
		_entityValue.Add("rho", 961); // greek small letter rho, U+03C1 ISOgrk3 
		_entityName.Add(961, "rho");
		_entityValue.Add("sigmaf", 962); // greek small letter final sigma, U+03C2 ISOgrk3 
		_entityName.Add(962, "sigmaf");
		_entityValue.Add("sigma", 963); // greek small letter sigma, U+03C3 ISOgrk3 
		_entityName.Add(963, "sigma");
		_entityValue.Add("tau", 964); // greek small letter tau, U+03C4 ISOgrk3 
		_entityName.Add(964, "tau");
		_entityValue.Add("upsilon", 965); // greek small letter upsilon, U+03C5 ISOgrk3 
		_entityName.Add(965, "upsilon");
		_entityValue.Add("phi", 966); // greek small letter phi, U+03C6 ISOgrk3 
		_entityName.Add(966, "phi");
		_entityValue.Add("chi", 967); // greek small letter chi, U+03C7 ISOgrk3 
		_entityName.Add(967, "chi");
		_entityValue.Add("psi", 968); // greek small letter psi, U+03C8 ISOgrk3 
		_entityName.Add(968, "psi");
		_entityValue.Add("omega", 969); // greek small letter omega, U+03C9 ISOgrk3 
		_entityName.Add(969, "omega");
		_entityValue.Add("thetasym", 977); // greek small letter theta symbol, U+03D1 NEW 
		_entityName.Add(977, "thetasym");
		_entityValue.Add("upsih", 978); // greek upsilon with hook symbol, U+03D2 NEW 
		_entityName.Add(978, "upsih");
		_entityValue.Add("piv", 982); // greek pi symbol, U+03D6 ISOgrk3 
		_entityName.Add(982, "piv");
		_entityValue.Add("bull", 8226); // bullet = black small circle, U+2022 ISOpub 
		_entityName.Add(8226, "bull");
		_entityValue.Add("hellip", 8230); // horizontal ellipsis = three dot leader, U+2026 ISOpub 
		_entityName.Add(8230, "hellip");
		_entityValue.Add("prime", 8242); // prime = minutes = feet, U+2032 ISOtech 
		_entityName.Add(8242, "prime");
		_entityValue.Add("Prime", 8243); // double prime = seconds = inches, U+2033 ISOtech 
		_entityName.Add(8243, "Prime");
		_entityValue.Add("oline", 8254); // overline = spacing overscore, U+203E NEW 
		_entityName.Add(8254, "oline");
		_entityValue.Add("frasl", 8260); // fraction slash, U+2044 NEW 
		_entityName.Add(8260, "frasl");
		_entityValue.Add("weierp", 8472); // script capital P = power set = Weierstrass p, U+2118 ISOamso 
		_entityName.Add(8472, "weierp");
		_entityValue.Add("image", 8465); // blackletter capital I = imaginary part, U+2111 ISOamso 
		_entityName.Add(8465, "image");
		_entityValue.Add("real", 8476); // blackletter capital R = real part symbol, U+211C ISOamso 
		_entityName.Add(8476, "real");
		_entityValue.Add("trade", 8482); // trade mark sign, U+2122 ISOnum 
		_entityName.Add(8482, "trade");
		_entityValue.Add("alefsym", 8501); // alef symbol = first transfinite cardinal, U+2135 NEW 
		_entityName.Add(8501, "alefsym");
		_entityValue.Add("larr", 8592); // leftwards arrow, U+2190 ISOnum 
		_entityName.Add(8592, "larr");
		_entityValue.Add("uarr", 8593); // upwards arrow, U+2191 ISOnum
		_entityName.Add(8593, "uarr");
		_entityValue.Add("rarr", 8594); // rightwards arrow, U+2192 ISOnum 
		_entityName.Add(8594, "rarr");
		_entityValue.Add("darr", 8595); // downwards arrow, U+2193 ISOnum 
		_entityName.Add(8595, "darr");
		_entityValue.Add("harr", 8596); // left right arrow, U+2194 ISOamsa 
		_entityName.Add(8596, "harr");
		_entityValue.Add("crarr", 8629); // downwards arrow with corner leftwards = carriage return, U+21B5 NEW 
		_entityName.Add(8629, "crarr");
		_entityValue.Add("lArr", 8656); // leftwards double arrow, U+21D0 ISOtech 
		_entityName.Add(8656, "lArr");
		_entityValue.Add("uArr", 8657); // upwards double arrow, U+21D1 ISOamsa 
		_entityName.Add(8657, "uArr");
		_entityValue.Add("rArr", 8658); // rightwards double arrow, U+21D2 ISOtech 
		_entityName.Add(8658, "rArr");
		_entityValue.Add("dArr", 8659); // downwards double arrow, U+21D3 ISOamsa 
		_entityName.Add(8659, "dArr");
		_entityValue.Add("hArr", 8660); // left right double arrow, U+21D4 ISOamsa 
		_entityName.Add(8660, "hArr");
		_entityValue.Add("forall", 8704); // for all, U+2200 ISOtech 
		_entityName.Add(8704, "forall");
		_entityValue.Add("part", 8706); // partial differential, U+2202 ISOtech 
		_entityName.Add(8706, "part");
		_entityValue.Add("exist", 8707); // there exists, U+2203 ISOtech 
		_entityName.Add(8707, "exist");
		_entityValue.Add("empty", 8709); // empty set = null set = diameter, U+2205 ISOamso 
		_entityName.Add(8709, "empty");
		_entityValue.Add("nabla", 8711); // nabla = backward difference, U+2207 ISOtech 
		_entityName.Add(8711, "nabla");
		_entityValue.Add("isin", 8712); // element of, U+2208 ISOtech 
		_entityName.Add(8712, "isin");
		_entityValue.Add("notin", 8713); // not an element of, U+2209 ISOtech 
		_entityName.Add(8713, "notin");
		_entityValue.Add("ni", 8715); // contains as member, U+220B ISOtech 
		_entityName.Add(8715, "ni");
		_entityValue.Add("prod", 8719); // n-ary product = product sign, U+220F ISOamsb 
		_entityName.Add(8719, "prod");
		_entityValue.Add("sum", 8721); // n-ary sumation, U+2211 ISOamsb 
		_entityName.Add(8721, "sum");
		_entityValue.Add("minus", 8722); // minus sign, U+2212 ISOtech 
		_entityName.Add(8722, "minus");
		_entityValue.Add("lowast", 8727); // asterisk operator, U+2217 ISOtech 
		_entityName.Add(8727, "lowast");
		_entityValue.Add("radic", 8730); // square root = radical sign, U+221A ISOtech 
		_entityName.Add(8730, "radic");
		_entityValue.Add("prop", 8733); // proportional to, U+221D ISOtech 
		_entityName.Add(8733, "prop");
		_entityValue.Add("infin", 8734); // infinity, U+221E ISOtech 
		_entityName.Add(8734, "infin");
		_entityValue.Add("ang", 8736); // angle, U+2220 ISOamso 
		_entityName.Add(8736, "ang");
		_entityValue.Add("and", 8743); // logical and = wedge, U+2227 ISOtech 
		_entityName.Add(8743, "and");
		_entityValue.Add("or", 8744); // logical or = vee, U+2228 ISOtech 
		_entityName.Add(8744, "or");
		_entityValue.Add("cap", 8745); // intersection = cap, U+2229 ISOtech 
		_entityName.Add(8745, "cap");
		_entityValue.Add("cup", 8746); // union = cup, U+222A ISOtech 
		_entityName.Add(8746, "cup");
		_entityValue.Add("int", 8747); // integral, U+222B ISOtech 
		_entityName.Add(8747, "int");
		_entityValue.Add("there4", 8756); // therefore, U+2234 ISOtech 
		_entityName.Add(8756, "there4");
		_entityValue.Add("sim", 8764); // tilde operator = varies with = similar to, U+223C ISOtech 
		_entityName.Add(8764, "sim");
		_entityValue.Add("cong", 8773); // approximately equal to, U+2245 ISOtech 
		_entityName.Add(8773, "cong");
		_entityValue.Add("asymp", 8776); // almost equal to = asymptotic to, U+2248 ISOamsr 
		_entityName.Add(8776, "asymp");
		_entityValue.Add("ne", 8800); // not equal to, U+2260 ISOtech 
		_entityName.Add(8800, "ne");
		_entityValue.Add("equiv", 8801); // identical to, U+2261 ISOtech 
		_entityName.Add(8801, "equiv");
		_entityValue.Add("le", 8804); // less-than or equal to, U+2264 ISOtech 
		_entityName.Add(8804, "le");
		_entityValue.Add("ge", 8805); // greater-than or equal to, U+2265 ISOtech 
		_entityName.Add(8805, "ge");
		_entityValue.Add("sub", 8834); // subset of, U+2282 ISOtech 
		_entityName.Add(8834, "sub");
		_entityValue.Add("sup", 8835); // superset of, U+2283 ISOtech 
		_entityName.Add(8835, "sup");
		_entityValue.Add("nsub", 8836); // not a subset of, U+2284 ISOamsn 
		_entityName.Add(8836, "nsub");
		_entityValue.Add("sube", 8838); // subset of or equal to, U+2286 ISOtech 
		_entityName.Add(8838, "sube");
		_entityValue.Add("supe", 8839); // superset of or equal to, U+2287 ISOtech 
		_entityName.Add(8839, "supe");
		_entityValue.Add("oplus", 8853); // circled plus = direct sum, U+2295 ISOamsb 
		_entityName.Add(8853, "oplus");
		_entityValue.Add("otimes", 8855); // circled times = vector product, U+2297 ISOamsb 
		_entityName.Add(8855, "otimes");
		_entityValue.Add("perp", 8869); // up tack = orthogonal to = perpendicular, U+22A5 ISOtech 
		_entityName.Add(8869, "perp");
		_entityValue.Add("sdot", 8901); // dot operator, U+22C5 ISOamsb 
		_entityName.Add(8901, "sdot");
		_entityValue.Add("lceil", 8968); // left ceiling = apl upstile, U+2308 ISOamsc 
		_entityName.Add(8968, "lceil");
		_entityValue.Add("rceil", 8969); // right ceiling, U+2309 ISOamsc 
		_entityName.Add(8969, "rceil");
		_entityValue.Add("lfloor", 8970); // left floor = apl downstile, U+230A ISOamsc 
		_entityName.Add(8970, "lfloor");
		_entityValue.Add("rfloor", 8971); // right floor, U+230B ISOamsc 
		_entityName.Add(8971, "rfloor");
		_entityValue.Add("lang", 9001); // left-pointing angle bracket = bra, U+2329 ISOtech 
		_entityName.Add(9001, "lang");
		_entityValue.Add("rang", 9002); // right-pointing angle bracket = ket, U+232A ISOtech 
		_entityName.Add(9002, "rang");
		_entityValue.Add("loz", 9674); // lozenge, U+25CA ISOpub 
		_entityName.Add(9674, "loz");
		_entityValue.Add("spades", 9824); // black spade suit, U+2660 ISOpub 
		_entityName.Add(9824, "spades");
		_entityValue.Add("clubs", 9827); // black club suit = shamrock, U+2663 ISOpub 
		_entityName.Add(9827, "clubs");
		_entityValue.Add("hearts", 9829); // black heart suit = valentine, U+2665 ISOpub 
		_entityName.Add(9829, "hearts");
		_entityValue.Add("diams", 9830); // black diamond suit, U+2666 ISOpub 
		_entityName.Add(9830, "diams");
		_entityValue.Add("OElig", 338); // latin capital ligature OE, U+0152 ISOlat2 
		_entityName.Add(338, "OElig");
		_entityValue.Add("oelig", 339); // latin small ligature oe, U+0153 ISOlat2 
		_entityName.Add(339, "oelig");
		_entityValue.Add("Scaron", 352); // latin capital letter S with caron, U+0160 ISOlat2 
		_entityName.Add(352, "Scaron");
		_entityValue.Add("scaron", 353); // latin small letter s with caron, U+0161 ISOlat2 
		_entityName.Add(353, "scaron");
		_entityValue.Add("Yuml", 376); // latin capital letter Y with diaeresis, U+0178 ISOlat2 
		_entityName.Add(376, "Yuml");
		_entityValue.Add("circ", 710); // modifier letter circumflex accent, U+02C6 ISOpub 
		_entityName.Add(710, "circ");
		_entityValue.Add("tilde", 732); // small tilde, U+02DC ISOdia 
		_entityName.Add(732, "tilde");
		_entityValue.Add("ensp", 8194); // en space, U+2002 ISOpub 
		_entityName.Add(8194, "ensp");
		_entityValue.Add("emsp", 8195); // em space, U+2003 ISOpub 
		_entityName.Add(8195, "emsp");
		_entityValue.Add("thinsp", 8201); // thin space, U+2009 ISOpub 
		_entityName.Add(8201, "thinsp");
		_entityValue.Add("zwnj", 8204); // zero width non-joiner, U+200C NEW RFC 2070 
		_entityName.Add(8204, "zwnj");
		_entityValue.Add("zwj", 8205); // zero width joiner, U+200D NEW RFC 2070 
		_entityName.Add(8205, "zwj");
		_entityValue.Add("lrm", 8206); // left-to-right mark, U+200E NEW RFC 2070 
		_entityName.Add(8206, "lrm");
		_entityValue.Add("rlm", 8207); // right-to-left mark, U+200F NEW RFC 2070 
		_entityName.Add(8207, "rlm");
		_entityValue.Add("ndash", 8211); // en dash, U+2013 ISOpub 
		_entityName.Add(8211, "ndash");
		_entityValue.Add("mdash", 8212); // em dash, U+2014 ISOpub 
		_entityName.Add(8212, "mdash");
		_entityValue.Add("lsquo", 8216); // left single quotation mark, U+2018 ISOnum 
		_entityName.Add(8216, "lsquo");
		_entityValue.Add("rsquo", 8217); // right single quotation mark, U+2019 ISOnum 
		_entityName.Add(8217, "rsquo");
		_entityValue.Add("sbquo", 8218); // single low-9 quotation mark, U+201A NEW 
		_entityName.Add(8218, "sbquo");
		_entityValue.Add("ldquo", 8220); // left double quotation mark, U+201C ISOnum 
		_entityName.Add(8220, "ldquo");
		_entityValue.Add("rdquo", 8221); // right double quotation mark, U+201D ISOnum 
		_entityName.Add(8221, "rdquo");
		_entityValue.Add("bdquo", 8222); // double low-9 quotation mark, U+201E NEW 
		_entityName.Add(8222, "bdquo");
		_entityValue.Add("dagger", 8224); // dagger, U+2020 ISOpub 
		_entityName.Add(8224, "dagger");
		_entityValue.Add("Dagger", 8225); // double dagger, U+2021 ISOpub 
		_entityName.Add(8225, "Dagger");
		_entityValue.Add("permil", 8240); // per mille sign, U+2030 ISOtech 
		_entityName.Add(8240, "permil");
		_entityValue.Add("lsaquo", 8249); // single left-pointing angle quotation mark, U+2039 ISO proposed 
		_entityName.Add(8249, "lsaquo");
		_entityValue.Add("rsaquo", 8250); // single right-pointing angle quotation mark, U+203A ISO proposed 
		_entityName.Add(8250, "rsaquo");
		_entityValue.Add("euro", 8364); // euro sign, U+20AC NEW 
		_entityName.Add(8364, "euro");
		_maxEntitySize = 8 + 1; // we add the # char
	}
	private HtmlEntity()
	{
	}
	public static string DeEntitize(string text)
	{
		if (text == null)
			return null;
		if (text.Length == 0)
			return text;
		StringBuilder sb = new StringBuilder(text.Length);
		ParseState state = ParseState.Text;
		StringBuilder entity = new StringBuilder(10);
		for (int i = 0; i < text.Length; i++)
		{
			switch (state)
			{
				case ParseState.Text:
					switch (text[i])
					{
						case '&':
							state = ParseState.EntityStart;
							break;
						default:
							sb.Append(text[i]);
							break;
					}
					break;
				case ParseState.EntityStart:
					switch (text[i])
					{
						case ';':
							if (entity.Length == 0)
							{
								sb.Append("&;");
							}
							else
							{
								if (entity[0] == '#')
								{
									string e = entity.ToString();
									try
									{
										string codeStr = e.Substring(1).Trim();
										int fromBase;
										if (codeStr.StartsWith("x", StringComparison.OrdinalIgnoreCase))
										{
											fromBase = 16;
											codeStr = codeStr.Substring(1);
										}
										else
										{
											fromBase = 10;
										}
										int code = Convert.ToInt32(codeStr, fromBase);
										sb.Append(Convert.ToChar(code));
									}
									catch
									{
										sb.Append("&#" + e + ";");
									}
								}
								else
								{
									int code;
									if (!_entityValue.TryGetValue(entity.ToString(), out code))
									{
										sb.Append("&" + entity + ";");
									}
									else
									{
										sb.Append(Convert.ToChar(code));
									}
								}
								entity.Remove(0, entity.Length);
							}
							state = ParseState.Text;
							break;
						case '&':
							sb.Append("&" + entity);
							entity.Remove(0, entity.Length);
							break;
						default:
							entity.Append(text[i]);
							if (entity.Length > _maxEntitySize)
							{
								state = ParseState.Text;
								sb.Append("&" + entity);
								entity.Remove(0, entity.Length);
							}
							break;
					}
					break;
			}
		}
		if (state == ParseState.EntityStart)
		{
			sb.Append("&" + entity);
		}
		return sb.ToString();
	}
	public static HtmlNode Entitize(HtmlNode node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		HtmlNode result = node.CloneNode(true);
		if (result.HasAttributes)
			Entitize(result.Attributes);
		if (result.HasChildNodes)
		{
			Entitize(result.ChildNodes);
		}
		else
		{
			if (result.NodeType == HtmlNodeType.Text)
			{
				((HtmlTextNode) result).Text = Entitize(((HtmlTextNode) result).Text, true, true);
			}
		}
		return result;
	}
	public static string Entitize(string text)
	{
		return Entitize(text, true);
	}
	public static string Entitize(string text, bool useNames)
	{
		return Entitize(text, useNames, false);
	}
	public static string Entitize(string text, bool useNames, bool entitizeQuotAmpAndLtGt)
	{
		if (text == null)
			return null;
		if (text.Length == 0)
			return text;
		StringBuilder sb = new StringBuilder(text.Length);
#if !FX20 && !FX35
		if (UseWebUtility)
		{
			TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(text);
			while (enumerator.MoveNext())
			{
				sb.Append(System.Net.WebUtility.HtmlEncode(enumerator.GetTextElement()));
			}
		}
		else
		{
#endif
			for (int i = 0; i < text.Length; i++)
			{
				int code = text[i];
				if ((code > 127) ||
					(entitizeQuotAmpAndLtGt && ((code == 34) || (code == 38) || (code == 60) || (code == 62))))
				{
					string entity = null;
					if (useNames)
					{
						EntityName.TryGetValue(code, out entity);
					}
					if (entity == null)
					{
						sb.Append("&#" + code + ";");
					}
					else
					{
						sb.Append("&" + entity + ";");
					}
				}
				else
				{
					sb.Append(text[i]);
				}
			}
#if !FX20 && !FX35
		}
#endif
		return sb.ToString();
	}
	private static void Entitize(HtmlAttributeCollection collection)
	{
		foreach (HtmlAttribute at in collection)
		{
			if (at.Value == null)
			{
				continue;
			}
			at.Value = Entitize(at.Value);
		}
	}
	private static void Entitize(HtmlNodeCollection collection)
	{
		foreach (HtmlNode node in collection)
		{
			if (node.HasAttributes)
				Entitize(node.Attributes);
			if (node.HasChildNodes)
			{
				Entitize(node.ChildNodes);
			}
			else
			{
				if (node.NodeType == HtmlNodeType.Text)
				{
					((HtmlTextNode) node).Text = Entitize(((HtmlTextNode) node).Text, true, true);
				}
			}
		}
	}
	private enum ParseState
	{
		Text,
		EntityStart
	}
}
internal class HtmlNameTable : XmlNameTable
{
	private NameTable _nametable = new NameTable();
	public override string Add(string array)
	{
		return _nametable.Add(array);
	}
	public override string Add(char[] array, int offset, int length)
	{
		return _nametable.Add(array, offset, length);
	}
	public override string Get(string array)
	{
		return _nametable.Get(array);
	}
	public override string Get(char[] array, int offset, int length)
	{
		return _nametable.Get(array, offset, length);
	}
	internal string GetOrAdd(string array)
	{
		string s = Get(array);
		if (s == null)
		{
			return Add(array);
		}
		return s;
	}
}
[DebuggerDisplay("Name: {OriginalName}")]
public partial class HtmlNode
{
	internal const string DepthLevelExceptionMessage = "The document is too complex to parse";
	internal HtmlAttributeCollection _attributes;
	internal HtmlNodeCollection _childnodes;
	internal HtmlNode _endnode;
	private bool _changed;
	internal string _innerhtml;
	internal int _innerlength;
	internal int _innerstartindex;
	internal int _line;
	internal int _lineposition;
	private string _name;
	internal int _namelength;
	internal int _namestartindex;
	internal HtmlNode _nextnode;
	internal HtmlNodeType _nodetype;
	internal string _outerhtml;
	internal int _outerlength;
	internal int _outerstartindex;
	private string _optimizedName;
	internal HtmlDocument _ownerdocument;
	internal HtmlNode _parentnode;
	internal HtmlNode _prevnode;
	internal HtmlNode _prevwithsamename;
	internal bool _starttag;
	internal int _streamposition;
	internal bool _isImplicitEnd;
	internal bool _isHideInnerText;
	public static readonly string HtmlNodeTypeNameComment = "#comment";
	public static readonly string HtmlNodeTypeNameDocument = "#document";
	public static readonly string HtmlNodeTypeNameText = "#text";
	public static Dictionary<string, HtmlElementFlag> ElementsFlags; 
	static HtmlNode()
	{
		ElementsFlags = new Dictionary<string, HtmlElementFlag>(StringComparer.OrdinalIgnoreCase);
		ElementsFlags.Add("script", HtmlElementFlag.CData);
		ElementsFlags.Add("style", HtmlElementFlag.CData);
		ElementsFlags.Add("noxhtml", HtmlElementFlag.CData); // can't found.
		ElementsFlags.Add("textarea", HtmlElementFlag.CData);
		ElementsFlags.Add("title", HtmlElementFlag.CData);
		ElementsFlags.Add("base", HtmlElementFlag.Empty);
		ElementsFlags.Add("link", HtmlElementFlag.Empty);
		ElementsFlags.Add("meta", HtmlElementFlag.Empty);
		ElementsFlags.Add("isindex", HtmlElementFlag.Empty);
		ElementsFlags.Add("hr", HtmlElementFlag.Empty);
		ElementsFlags.Add("col", HtmlElementFlag.Empty);
		ElementsFlags.Add("img", HtmlElementFlag.Empty);
		ElementsFlags.Add("param", HtmlElementFlag.Empty);
		ElementsFlags.Add("embed", HtmlElementFlag.Empty);
		ElementsFlags.Add("frame", HtmlElementFlag.Empty);
		ElementsFlags.Add("wbr", HtmlElementFlag.Empty);
		ElementsFlags.Add("bgsound", HtmlElementFlag.Empty);
		ElementsFlags.Add("spacer", HtmlElementFlag.Empty);
		ElementsFlags.Add("keygen", HtmlElementFlag.Empty);
		ElementsFlags.Add("area", HtmlElementFlag.Empty);
		ElementsFlags.Add("input", HtmlElementFlag.Empty);
		ElementsFlags.Add("basefont", HtmlElementFlag.Empty);
		ElementsFlags.Add("source", HtmlElementFlag.Empty);
		ElementsFlags.Add("form", HtmlElementFlag.CanOverlap);
		ElementsFlags.Add("br", HtmlElementFlag.Empty | HtmlElementFlag.Closed);
		if (!HtmlDocument.DisableBehaviorTagP)
		{
			ElementsFlags.Add("p", HtmlElementFlag.Empty | HtmlElementFlag.Closed);
		}
	}
	public HtmlNode(HtmlNodeType type, HtmlDocument ownerdocument, int index)
	{
		_nodetype = type;
		_ownerdocument = ownerdocument;
		_outerstartindex = index;
		switch (type)
		{
			case HtmlNodeType.Comment:
				SetName(HtmlNodeTypeNameComment);
				_endnode = this;
				break;
			case HtmlNodeType.Document:
				SetName(HtmlNodeTypeNameDocument);
				_endnode = this;
				break;
			case HtmlNodeType.Text:
				SetName(HtmlNodeTypeNameText);
				_endnode = this;
				break;
		}
		if (_ownerdocument.Openednodes != null)
		{
			if (!Closed)
			{
				if (-1 != index)
				{
					_ownerdocument.Openednodes.Add(index, this);
				}
			}
		}
		if ((-1 != index) || (type == HtmlNodeType.Comment) || (type == HtmlNodeType.Text)) return;
		SetChanged();
	}
	public HtmlAttributeCollection Attributes
	{
		get
		{
			if (!HasAttributes)
			{
				_attributes = new HtmlAttributeCollection(this);
			}
			return _attributes;
		}
		internal set { _attributes = value; }
	}
	public HtmlNodeCollection ChildNodes
	{
		get { return _childnodes ?? (_childnodes = new HtmlNodeCollection(this)); }
		internal set { _childnodes = value; }
	}
	public bool Closed
	{
		get { return (_endnode != null); }
	}
	public HtmlAttributeCollection ClosingAttributes
	{
		get { return !HasClosingAttributes ? new HtmlAttributeCollection(this) : _endnode.Attributes; }
	}
	public HtmlNode EndNode
	{
		get { return _endnode; }
	}
	public HtmlNode FirstChild
	{
		get { return !HasChildNodes ? null : _childnodes[0]; }
	}
	public bool HasAttributes
	{
		get
		{
			if (_attributes == null)
			{
				return false;
			}
			if (_attributes.Count <= 0)
			{
				return false;
			}
			return true;
		}
	}
	public bool HasChildNodes
	{
		get
		{
			if (_childnodes == null)
			{
				return false;
			}
			if (_childnodes.Count <= 0)
			{
				return false;
			}
			return true;
		}
	}
	public bool HasClosingAttributes
	{
		get
		{
			if ((_endnode == null) || (_endnode == this))
			{
				return false;
			}
			if (_endnode._attributes == null)
			{
				return false;
			}
			if (_endnode._attributes.Count <= 0)
			{
				return false;
			}
			return true;
		}
	}
	public string Id
	{
		get
		{
			if (_ownerdocument.Nodesid == null)
				throw new Exception(HtmlDocument.HtmlExceptionUseIdAttributeFalse);
			return GetId();
		}
		set
		{
			if (_ownerdocument.Nodesid == null)
				throw new Exception(HtmlDocument.HtmlExceptionUseIdAttributeFalse);
			if (value == null)
				throw new ArgumentNullException("value");
			SetId(value);
		}
	}
	public virtual string InnerHtml
	{
		get
		{
			if (_changed)
			{
				UpdateHtml();
				return _innerhtml;
			}
			if (_innerhtml != null)
				return _innerhtml;
			if (_innerstartindex < 0 || _innerlength < 0)
				return string.Empty;
			return _ownerdocument.Text.Substring(_innerstartindex, _innerlength);
		}
		set
		{
			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(value);
			RemoveAllChildren();
			AppendChildren(doc.DocumentNode.ChildNodes);
		}
	}
	public virtual string InnerText
	{
		get
		{
			var sb = new StringBuilder();
			int depthLevel = 0;
			string name = this.Name;
			if (name != null)
			{
				name = name.ToLowerInvariant();
				bool isDisplayScriptingText = (name == "head" || name == "script" || name == "style"); 
				InternalInnerText(sb, isDisplayScriptingText, depthLevel);
			}
			else
			{ 
				InternalInnerText(sb, false, depthLevel);
			} 
			return sb.ToString();
		}
	}
	internal virtual void InternalInnerText(StringBuilder sb, bool isDisplayScriptingText, int depthLevel)
	{
		depthLevel++;
		if (depthLevel > HtmlDocument.MaxDepthLevel)
		{
			throw new Exception($"Maximum deep level reached: {HtmlDocument.MaxDepthLevel}");
		}
		if (!_ownerdocument.BackwardCompatibility)
		{
			if (HasChildNodes)
			{
				AppendInnerText(sb, isDisplayScriptingText);
				return;
			}
			sb.Append(GetCurrentNodeText());
			return;
		}
		if (_nodetype == HtmlNodeType.Text)
		{
			sb.Append(((HtmlTextNode) this).Text);
			return;
		}
		if (_nodetype == HtmlNodeType.Comment)
		{
			return;
		}
		if (!HasChildNodes || (_isHideInnerText && !isDisplayScriptingText))
		{
			return;
		}
		foreach (HtmlNode node in ChildNodes)
			node.InternalInnerText(sb, isDisplayScriptingText, depthLevel);
	}
	public virtual string GetDirectInnerText()
	{
		if (!_ownerdocument.BackwardCompatibility)
		{
			if (HasChildNodes)
			{
				StringBuilder sb = new StringBuilder();
				AppendDirectInnerText(sb);
				return sb.ToString();
			}
			return GetCurrentNodeText();
		}
		if (_nodetype == HtmlNodeType.Text)
			return ((HtmlTextNode)this).Text;
		if (_nodetype == HtmlNodeType.Comment)
			return "";
		if (!HasChildNodes)
			return string.Empty; 
		var s = new StringBuilder();
		foreach (HtmlNode node in ChildNodes)
		{
			if (node._nodetype == HtmlNodeType.Text)
			{
				s.Append(((HtmlTextNode)node).Text);
			}
		}
		return s.ToString();
	}
	internal string GetCurrentNodeText()
	{
		if (_nodetype == HtmlNodeType.Text)
		{
			string s = ((HtmlTextNode) this).Text;
			if (ParentNode.Name != "pre")
			{
				s = s.Replace("\n", "").Replace("\r", "").Replace("\t", "");
			}
			return s;
		}
		return "";
	}
	internal void AppendDirectInnerText(StringBuilder sb)
	{
		if (_nodetype == HtmlNodeType.Text)
		{
			sb.Append(GetCurrentNodeText());
		}
		if (!HasChildNodes) return;
		foreach (HtmlNode node in ChildNodes)
		{
			sb.Append(node.GetCurrentNodeText());
		}
		return; 
	}
	internal void AppendInnerText(StringBuilder sb, bool isShowHideInnerText)
	{ 
		if (_nodetype == HtmlNodeType.Text)
		{
			sb.Append(GetCurrentNodeText());
		}
		if (!HasChildNodes || (_isHideInnerText && !isShowHideInnerText)) return;
		foreach (HtmlNode node in ChildNodes)
		{
			node.AppendInnerText(sb, isShowHideInnerText);
		}
	}
	public HtmlNode LastChild
	{
		get { return !HasChildNodes ? null : _childnodes[_childnodes.Count - 1]; }
	}
	public int Line
	{
		get { return _line; }
		internal set { _line = value; }
	}
	public int LinePosition
	{
		get { return _lineposition; }
		internal set { _lineposition = value; }
	}
	public int InnerStartIndex
	{
		get { return _innerstartindex; }
	}
	public int OuterStartIndex
	{
		get { return _outerstartindex; }
	}
	public int InnerLength
	{
		get { return InnerHtml.Length; }
	}
	public int OuterLength
	{
		get { return OuterHtml.Length; }
	}
	public string Name
	{
		get
		{
			if (_optimizedName == null)
			{
				if (_name == null)
					SetName(_ownerdocument.Text.Substring(_namestartindex, _namelength));
				if (_name == null)
					_optimizedName = string.Empty;
				else if (this.OwnerDocument != null)
					_optimizedName = this.OwnerDocument.OptionDefaultUseOriginalName ? _name : _name.ToLowerInvariant();
				else
					_optimizedName = _name.ToLowerInvariant();
			}
			return _optimizedName;
		}
		set
		{
			SetName(value);
			SetChanged();
		}
	}
	internal void SetName(string value)
	{
		_name = value;
		_optimizedName = null;
	}
	public HtmlNode NextSibling
	{
		get { return _nextnode; }
		internal set { _nextnode = value; }
	}
	public HtmlNodeType NodeType
	{
		get { return _nodetype; }
		internal set { _nodetype = value; }
	}
	public string OriginalName
	{
		get { return _name; }
	}
	public virtual string OuterHtml
	{
		get
		{
			if (_changed)
			{
				UpdateHtml();
				return _outerhtml;
			}
			if (_outerhtml != null)
			{
				return _outerhtml;
			}
			if (_outerstartindex < 0 || _outerlength < 0)
			{
				return string.Empty;
			}
			return _ownerdocument.Text.Substring(_outerstartindex, _outerlength);
		}
	}
	public HtmlDocument OwnerDocument
	{
		get { return _ownerdocument; }
		internal set { _ownerdocument = value; }
	}
	public HtmlNode ParentNode
	{
		get { return _parentnode; }
		internal set { _parentnode = value; }
	}
	public HtmlNode PreviousSibling
	{
		get { return _prevnode; }
		internal set { _prevnode = value; }
	}
	public int StreamPosition
	{
		get { return _streamposition; }
	}
	public string XPath
	{
		get
		{
			string basePath = (ParentNode == null || ParentNode.NodeType == HtmlNodeType.Document)
				? "/"
				: ParentNode.XPath + "/";
			return basePath + GetRelativeXpath();
		}
	}
	public int Depth { get; set; }
	public static bool CanOverlapElement(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		HtmlElementFlag flag;
		if (!ElementsFlags.TryGetValue(name, out flag))
		{
			return false;
		}
		return (flag & HtmlElementFlag.CanOverlap) != 0;
	}
	public static HtmlNode CreateNode(string html)
	{ 
		return CreateNode(html, null);
	}
	public static HtmlNode CreateNode(string html, Action<HtmlDocument> htmlDocumentBuilder)
	{
		HtmlDocument doc = new HtmlDocument();
		if (htmlDocumentBuilder != null)
		{
			htmlDocumentBuilder(doc);
		}
		doc.LoadHtml(html);
		if (!doc.DocumentNode.IsSingleElementNode())
		{
			throw new Exception("Multiple node elements can't be created.");
		}
		var element = doc.DocumentNode.FirstChild;
		while (element != null)
		{
			if (element.NodeType == HtmlNodeType.Element && element.OuterHtml != "\r\n")
				return element;
			element = element.NextSibling;
		}
		return doc.DocumentNode.FirstChild;
	}
	public static bool IsCDataElement(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		HtmlElementFlag flag;
		if (!ElementsFlags.TryGetValue(name, out flag))
		{
			return false;
		}
		return (flag & HtmlElementFlag.CData) != 0;
	}
	public static bool IsClosedElement(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		HtmlElementFlag flag;
		if (!ElementsFlags.TryGetValue(name, out flag))
		{
			return false;
		}
		return (flag & HtmlElementFlag.Closed) != 0;
	}
	public static bool IsEmptyElement(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			return true;
		}
		if ('!' == name[0])
		{
			return true;
		}
		if ('?' == name[0])
		{
			return true;
		}
		HtmlElementFlag flag;
		if (!ElementsFlags.TryGetValue(name, out flag))
		{
			return false;
		}
		return (flag & HtmlElementFlag.Empty) != 0;
	}
	public static bool IsOverlappedClosingElement(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (text.Length <= 4)
			return false;
		if ((text[0] != '<') ||
			(text[text.Length - 1] != '>') ||
			(text[1] != '/'))
			return false;
		string name = text.Substring(2, text.Length - 3);
		return CanOverlapElement(name);
	}
	public IEnumerable<HtmlNode> Ancestors()
	{
		HtmlNode node = ParentNode;
		if (node != null)
		{
			yield return node; //return the immediate parent node
			while (node.ParentNode != null)
			{
				yield return node.ParentNode;
				node = node.ParentNode;
			}
		}
	}
	public IEnumerable<HtmlNode> Ancestors(string name)
	{
		for (HtmlNode n = ParentNode; n != null; n = n.ParentNode)
			if (n.Name == name)
				yield return n;
	}
	public IEnumerable<HtmlNode> AncestorsAndSelf()
	{
		for (HtmlNode n = this; n != null; n = n.ParentNode)
			yield return n;
	}
	public IEnumerable<HtmlNode> AncestorsAndSelf(string name)
	{
		for (HtmlNode n = this; n != null; n = n.ParentNode)
			if (n.Name == name)
				yield return n;
	}
	public HtmlNode AppendChild(HtmlNode newChild)
	{
		if (newChild == null)
		{
			throw new ArgumentNullException("newChild");
		}
		ChildNodes.Append(newChild);
		_ownerdocument.SetIdForNode(newChild, newChild.GetId());
		SetChildNodesId(newChild);
		var parentnode = _parentnode;
		HtmlDocument lastOwnerDocument = null;
		while (parentnode != null) 
		{
			if(parentnode.OwnerDocument != lastOwnerDocument)
			{
				parentnode.OwnerDocument.SetIdForNode(newChild, newChild.GetId());
				parentnode.SetChildNodesId(newChild);
				lastOwnerDocument = parentnode.OwnerDocument;
			}
			parentnode = parentnode._parentnode;
		}
		SetChanged();
		return newChild;
	}
	public void SetChildNodesId(HtmlNode chilNode)
	{
		foreach (HtmlNode child in chilNode.ChildNodes)
		{
			_ownerdocument.SetIdForNode(child, child.GetId());
			if (child.ChildNodes == chilNode.ChildNodes)
			{
				throw new Exception("Oops! a scenario that will cause a Stack Overflow has been found. See the following issue for an example: https://github.com/zzzprojects/html-agility-pack/issues/513");
			}
			SetChildNodesId(child);
		}
	}
	public void AppendChildren(HtmlNodeCollection newChildren)
	{
		if (newChildren == null)
			throw new ArgumentNullException("newChildren");
		foreach (HtmlNode newChild in newChildren)
		{
			AppendChild(newChild);
		}
	}
	public IEnumerable<HtmlAttribute> ChildAttributes(string name)
	{
		return Attributes.AttributesWithName(name);
	}
	public HtmlNode Clone()
	{
		return CloneNode(true);
	}
	public HtmlNode CloneNode(string newName)
	{
		return CloneNode(newName, true);
	}
	public HtmlNode CloneNode(string newName, bool deep)
	{
		if (newName == null)
		{
			throw new ArgumentNullException("newName");
		}
		HtmlNode node = CloneNode(deep);
		node.SetName(newName);
		return node;
	}
	public HtmlNode CloneNode(bool deep)
	{
		HtmlNode node = _ownerdocument.CreateNode(_nodetype);
		node.SetName(OriginalName);
		switch (_nodetype)
		{
			case HtmlNodeType.Comment:
				((HtmlCommentNode) node).Comment = ((HtmlCommentNode) this).Comment;
				return node;
			case HtmlNodeType.Text:
				((HtmlTextNode) node).Text = ((HtmlTextNode) this).Text;
				return node;
		}
		if (HasAttributes)
		{
			foreach (HtmlAttribute att in _attributes)
			{
				HtmlAttribute newatt = att.Clone();
				node.Attributes.Append(newatt);
			}
		}
		if (HasClosingAttributes)
		{
			node._endnode = _endnode.CloneNode(false);
			foreach (HtmlAttribute att in _endnode._attributes)
			{
				HtmlAttribute newatt = att.Clone();
				node._endnode._attributes.Append(newatt);
			}
		}
		if (!deep)
		{
			return node;
		}
		if (!HasChildNodes)
		{
			return node;
		}
		foreach (HtmlNode child in _childnodes)
		{
			HtmlNode newchild = child.CloneNode(deep);
			node.AppendChild(newchild);
		}
		return node;
	}
	public void CopyFrom(HtmlNode node)
	{
		CopyFrom(node, true);
	}
	public void CopyFrom(HtmlNode node, bool deep)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		Attributes.RemoveAll();
		if (node.HasAttributes)
		{
			foreach (HtmlAttribute att in node.Attributes)
			{
				HtmlAttribute newatt = att.Clone();
				Attributes.Append(newatt);
			}
		}
		if (deep)
		{
			RemoveAllChildren();
			if (node.HasChildNodes)
			{
				foreach (HtmlNode child in node.ChildNodes)
				{
					AppendChild(child.CloneNode(true));
				}
			}
		}
	}
	[Obsolete("Use Descendants() instead, the results of this function will change in a future version")]
	public IEnumerable<HtmlNode> DescendantNodes(int level = 0)
	{
		if (level > HtmlDocument.MaxDepthLevel)
		{
			throw new ArgumentException(HtmlNode.DepthLevelExceptionMessage);
		}
		foreach (HtmlNode node in ChildNodes)
		{
			yield return node;
			foreach (HtmlNode descendant in node.DescendantNodes(level + 1))
			{
				yield return descendant;
			}
		}
	}
	[Obsolete("Use DescendantsAndSelf() instead, the results of this function will change in a future version")]
	public IEnumerable<HtmlNode> DescendantNodesAndSelf()
	{
		return DescendantsAndSelf();
	}
	public IEnumerable<HtmlNode> Descendants()
	{
		return Descendants(0);
	}
	public IEnumerable<HtmlNode> Descendants(int level)
	{
		if (level > HtmlDocument.MaxDepthLevel)
		{
			throw new ArgumentException(HtmlNode.DepthLevelExceptionMessage);
		}
		foreach (HtmlNode node in ChildNodes)
		{
			yield return node;
			foreach (HtmlNode descendant in node.Descendants(level + 1))
			{
				yield return descendant;
			}
		}
	}
	public IEnumerable<HtmlNode> Descendants(string name)
	{
		foreach (HtmlNode node in Descendants())
			if (String.Equals(node.Name, name, StringComparison.OrdinalIgnoreCase))
				yield return node;
	}
	public IEnumerable<HtmlNode> DescendantsAndSelf()
	{
		yield return this;
		foreach (HtmlNode n in Descendants())
		{
			HtmlNode el = n;
			if (el != null)
				yield return el;
		}
	}
	public IEnumerable<HtmlNode> DescendantsAndSelf(string name)
	{
		yield return this;
		foreach (HtmlNode node in Descendants())
			if (node.Name == name)
				yield return node;
	}
	public HtmlNode Element(string name)
	{
		foreach (HtmlNode node in ChildNodes)
			if (node.Name == name)
				return node;
		return null;
	}
	public IEnumerable<HtmlNode> Elements(string name)
	{
		foreach (HtmlNode node in ChildNodes)
			if (node.Name == name)
				yield return node;
	}
	public HtmlAttribute GetDataAttribute(string key)
	{
		return Attributes.Hashitems.SingleOrDefault(x => x.Key.Equals("data-" + key, StringComparison.OrdinalIgnoreCase)).Value;
	}
	public IEnumerable<HtmlAttribute> GetDataAttributes()
	{ 
		return Attributes.Hashitems.Where(x => x.Key.StartsWith("data-", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).ToList();
	}
	public IEnumerable<HtmlAttribute> GetAttributes()
	{
		return Attributes.items;
	}
	public IEnumerable<HtmlAttribute> GetAttributes(params string[] attributeNames)
	{ 
		List<HtmlAttribute> list = new List<HtmlAttribute>();
		foreach(var name in attributeNames)
		{
			list.Add(Attributes[name]);
		}
		return list;
	}
	public string GetAttributeValue(string name, string def)
	{
#if METRO || NETSTANDARD1_3 || NETSTANDARD1_6
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (!HasAttributes)
		{
			return def;
		}
		HtmlAttribute att = Attributes[name];
		if (att == null)
		{
			return def;
		}
		return att.Value;
#else
		return GetAttributeValue<string>(name, def);
#endif
	} 
	public int GetAttributeValue(string name, int def)
	{
#if METRO || NETSTANDARD1_3 || NETSTANDARD1_6
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (!HasAttributes)
		{
			return def;
		}
		HtmlAttribute att = Attributes[name];
		if (att == null)
		{
			return def;
		}
		try
		{
			return Convert.ToInt32(att.Value);
		}
		catch
		{
			return def;
		}
#else
		return GetAttributeValue<int>(name, def);
#endif
	}
	public bool GetAttributeValue(string name, bool def)
	{
#if METRO || NETSTANDARD1_3 || NETSTANDARD1_6
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (!HasAttributes)
		{
			return def;
		}
		HtmlAttribute att = Attributes[name];
		if (att == null)
		{
			return def;
		}
		try
		{
			return Convert.ToBoolean(att.Value);
		}
		catch
		{
			return def;
		}
#else
		return GetAttributeValue<bool>(name, def);
#endif
	}
#if !(METRO || NETSTANDARD1_3 || NETSTANDARD1_6)
	public T GetAttributeValue<T>(string name, T def) 
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (!HasAttributes)
		{
			return def;
		}
		HtmlAttribute att = Attributes[name];
		if (att == null)
		{
			return def;
		}
		try
		{
			return (T)att.Value.To(typeof(T));
		}
		catch
		{
			return def;
		}
	}
#endif
	public HtmlNode InsertAfter(HtmlNode newChild, HtmlNode refChild)
	{
		if (newChild == null)
		{
			throw new ArgumentNullException("newChild");
		}
		if (refChild == null)
		{
			return PrependChild(newChild);
		}
		if (newChild == refChild)
		{
			return newChild;
		}
		int index = -1;
		if (_childnodes != null)
		{
			index = _childnodes[refChild];
		}
		if (index == -1)
		{
			throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
		}
		if (_childnodes != null) _childnodes.Insert(index + 1, newChild);
		_ownerdocument.SetIdForNode(newChild, newChild.GetId());
		SetChildNodesId(newChild);
		SetChanged();
		return newChild;
	}
	public HtmlNode InsertBefore(HtmlNode newChild, HtmlNode refChild)
	{
		if (newChild == null)
		{
			throw new ArgumentNullException("newChild");
		}
		if (refChild == null)
		{
			return AppendChild(newChild);
		}
		if (newChild == refChild)
		{
			return newChild;
		}
		int index = -1;
		if (_childnodes != null)
		{
			index = _childnodes[refChild];
		}
		if (index == -1)
		{
			throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
		}
		if (_childnodes != null) _childnodes.Insert(index, newChild);
		_ownerdocument.SetIdForNode(newChild, newChild.GetId());
		SetChildNodesId(newChild);
		SetChanged();
		return newChild;
	}
	public HtmlNode PrependChild(HtmlNode newChild)
	{
		if (newChild == null)
		{
			throw new ArgumentNullException("newChild");
		}
		ChildNodes.Prepend(newChild);
		_ownerdocument.SetIdForNode(newChild, newChild.GetId());
		SetChildNodesId(newChild);
		SetChanged();
		return newChild;
	}
	public void PrependChildren(HtmlNodeCollection newChildren)
	{
		if (newChildren == null)
		{
			throw new ArgumentNullException("newChildren");
		}
		for (int i = newChildren.Count - 1; i >= 0; i--)
		{
			PrependChild(newChildren[i]);
		}
	}
	public void Remove()
	{
		if (ParentNode != null)
		{
			ParentNode.ChildNodes.Remove(this);
		}
	}
	public void RemoveAll()
	{
		RemoveAllChildren();
		if (HasAttributes)
		{
			_attributes.Clear();
		}
		if ((_endnode != null) && (_endnode != this))
		{
			if (_endnode._attributes != null)
			{
				_endnode._attributes.Clear();
			}
		}
		SetChanged();
	}
	public void RemoveAllChildren()
	{
		if (!HasChildNodes)
		{
			return;
		}
		if (_ownerdocument.OptionUseIdAttribute)
		{
			foreach (HtmlNode node in _childnodes)
			{
				_ownerdocument.SetIdForNode(null, node.GetId());
				RemoveAllIDforNode(node);
			}
		}
		_childnodes.Clear();
		SetChanged();
	}
	public void RemoveAllIDforNode(HtmlNode node)
	{
		foreach (HtmlNode nodeChildNode in node.ChildNodes)
		{
			_ownerdocument.SetIdForNode(null, nodeChildNode.GetId());
			RemoveAllIDforNode(nodeChildNode);
		}
	}
	public void MoveChild(HtmlNode child)
	{
		if (child == null)
		{
			throw new ArgumentNullException($"Oops! the '{nameof(child)}' parameter cannot be null.");
		}
		var oldParent = child.ParentNode; 
		AppendChild(child);
		if (oldParent != null)
		{
			oldParent.RemoveChild(child);
		}
	}
	public void MoveChildren(HtmlNodeCollection children)
	{
		if (children == null)
		{
			throw new ArgumentNullException($"Oops! the '{nameof(children)}' parameter cannot be null.");
		}
		var oldParent = children.ParentNode;
		AppendChildren(children);
		if (oldParent != null)
		{
			oldParent.RemoveChildren(children);
		}
	}
	public void RemoveChildren(HtmlNodeCollection oldChildren)
	{
		if (oldChildren == null)
		{
			throw new ArgumentNullException($"Oops! the '{nameof(oldChildren)}' parameter cannot be null.");
		}
		var list = oldChildren.ToList();
		foreach (HtmlNode newChild in list)
		{
			RemoveChild(newChild);
		}
	}
	public HtmlNode RemoveChild(HtmlNode oldChild)
	{
		if (oldChild == null)
		{
			throw new ArgumentNullException("oldChild");
		}
		int index = -1;
		if (_childnodes != null)
		{
			index = _childnodes[oldChild];
		}
		if (index == -1)
		{
			throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
		}
		if (_childnodes != null)
			_childnodes.Remove(index);
		_ownerdocument.SetIdForNode(null, oldChild.GetId());
		RemoveAllIDforNode(oldChild);
		SetChanged();
		return oldChild;
	}
	public HtmlNode RemoveChild(HtmlNode oldChild, bool keepGrandChildren)
	{
		if (oldChild == null)
		{
			throw new ArgumentNullException("oldChild");
		}
		if ((oldChild._childnodes != null) && keepGrandChildren)
		{
			HtmlNode prev = oldChild.PreviousSibling;
			foreach (HtmlNode grandchild in oldChild._childnodes)
			{
				prev = InsertAfter(grandchild, prev);
			}
		}
		RemoveChild(oldChild);
		SetChanged();
		return oldChild;
	}
	public HtmlNode ReplaceChild(HtmlNode newChild, HtmlNode oldChild)
	{
		if (newChild == null)
		{
			return RemoveChild(oldChild);
		}
		if (oldChild == null)
		{
			return AppendChild(newChild);
		}
		int index = -1;
		if (_childnodes != null)
		{
			index = _childnodes[oldChild];
		}
		if (index == -1)
		{
			throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
		}
		if (_childnodes != null) _childnodes.Replace(index, newChild);
		_ownerdocument.SetIdForNode(null, oldChild.GetId());
		RemoveAllIDforNode(oldChild);
		_ownerdocument.SetIdForNode(newChild, newChild.GetId());
		SetChildNodesId(newChild);
		SetChanged();
		return newChild;
	}
	public HtmlAttribute SetAttributeValue(string name, string value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		HtmlAttribute att = Attributes[name];
		if (att == null)
		{
			return Attributes.Append(_ownerdocument.CreateAttribute(name, value));
		}
		att.Value = value;
		return att;
	}
	public void WriteContentTo(TextWriter outText, int level = 0)
	{
		if (level > HtmlDocument.MaxDepthLevel)
		{
			throw new ArgumentException(HtmlNode.DepthLevelExceptionMessage);
		}
		if (_childnodes == null)
		{
			return;
		}
		foreach (HtmlNode node in _childnodes)
		{
			node.WriteTo(outText, level + 1);
		}
	}
	public string WriteContentTo()
	{
		StringWriter sw = new StringWriter();
		WriteContentTo(sw);
		sw.Flush();
		return sw.ToString();
	}
	public virtual void WriteTo(TextWriter outText, int level = 0)
	{
		string html;
		switch (_nodetype)
		{
			case HtmlNodeType.Comment:
				html = ((HtmlCommentNode) this).Comment;
				if (_ownerdocument.OptionOutputAsXml)
				{
					var commentNode = (HtmlCommentNode) this;
					if (!_ownerdocument.BackwardCompatibility && commentNode.Comment.ToLowerInvariant().StartsWith("<!doctype"))
					{
						outText.Write(commentNode.Comment);
					}
					else
					{
						if (OwnerDocument.OptionXmlForceOriginalComment)
						{
							outText.Write(commentNode.Comment);
						}
						else
						{
							outText.Write("<!--" + GetXmlComment(commentNode) + "-->");
						}
					}
				}
				else
					outText.Write(html);
				break;
			case HtmlNodeType.Document:
				if (_ownerdocument.OptionOutputAsXml)
				{
#if SILVERLIGHT || PocketPC || METRO || NETSTANDARD1_3 || NETSTANDARD1_6
					outText.Write("<?xml version=\"1.0\" encoding=\"" + _ownerdocument.GetOutEncoding().WebName + "\"?>");
#else
					outText.Write("<?xml version=\"1.0\" encoding=\"" + _ownerdocument.GetOutEncoding().BodyName + "\"?>");
#endif
					if (_ownerdocument.DocumentNode.HasChildNodes)
					{
						int rootnodes = _ownerdocument.DocumentNode._childnodes.Count;
						if (rootnodes > 0)
						{
							HtmlNode xml = _ownerdocument.GetXmlDeclaration();
							if (xml != null)
								rootnodes--;
							if (rootnodes > 1)
							{
								if (!_ownerdocument.BackwardCompatibility)
								{
									WriteContentTo(outText, level);
								}
								else
								{
									if (_ownerdocument.OptionOutputUpperCase)
									{
										outText.Write("<SPAN>");
										WriteContentTo(outText, level);
										outText.Write("</SPAN>");
									}
									else
									{
										outText.Write("<span>");
										WriteContentTo(outText, level);
										outText.Write("</span>");
									}
								}
								break;
							}
						}
					}
				}
				WriteContentTo(outText, level);
				break;
			case HtmlNodeType.Text:
				html = ((HtmlTextNode) this).Text;
				outText.Write(_ownerdocument.OptionOutputAsXml ? HtmlDocument.HtmlEncodeWithCompatibility(html, _ownerdocument.BackwardCompatibility) : html);
				break;
			case HtmlNodeType.Element:
				string name = _ownerdocument.OptionOutputUpperCase ? Name.ToUpperInvariant() : Name;
				if (_ownerdocument.OptionOutputOriginalCase)
					name = OriginalName;
				if (_ownerdocument.OptionOutputAsXml)
				{
					if (name.Length > 0)
					{
						if (name[0] == '?')
							break;
						if (name.Trim().Length == 0)
							break;
						name = HtmlDocument.GetXmlName(name, false, _ownerdocument.OptionPreserveXmlNamespaces);
					}
					else
						break;
				}
				outText.Write("<" + name);
				WriteAttributes(outText, false);
				if (HasChildNodes)
				{
					outText.Write(">");
					bool cdata = false;
					if (_ownerdocument.OptionOutputAsXml && IsCDataElement(Name))
					{
						cdata = true;
						outText.Write("\r\n//<![CDATA[\r\n");
					}
					if (cdata)
					{
						if (HasChildNodes)
							ChildNodes[0].WriteTo(outText, level);
						outText.Write("\r\n//]]>//\r\n");
					}
					else
						WriteContentTo(outText, level);
					if (_ownerdocument.OptionOutputAsXml || !_isImplicitEnd)
					{
						outText.Write("</" + name);
						if (!_ownerdocument.OptionOutputAsXml)
							WriteAttributes(outText, true);
						outText.Write(">");
					}
				}
				else
				{
					if (IsEmptyElement(Name))
					{
						if ((_ownerdocument.OptionWriteEmptyNodes) || (_ownerdocument.OptionOutputAsXml))
							outText.Write(" />");
						else
						{
							if (Name.Length > 0 && Name[0] == '?')
								outText.Write("?");
							outText.Write(">");
						}
					}
					else
					{
						if (!_isImplicitEnd)
						{
							outText.Write("></" + name + ">");
						}
						else
						{
							outText.Write(">");
						}
					}
				}
				break;
		}
	}
	public void WriteTo(XmlWriter writer)
	{
		switch (_nodetype)
		{
			case HtmlNodeType.Comment:
				writer.WriteComment(GetXmlComment((HtmlCommentNode) this));
				break;
			case HtmlNodeType.Document:
#if SILVERLIGHT || PocketPC || METRO || NETSTANDARD1_3 || NETSTANDARD1_6
				writer.WriteProcessingInstruction("xml",
												  "version=\"1.0\" encoding=\"" +
												  _ownerdocument.GetOutEncoding().WebName + "\"");
#else
				writer.WriteProcessingInstruction("xml",
					"version=\"1.0\" encoding=\"" +
					_ownerdocument.GetOutEncoding().BodyName + "\"");
#endif
				if (HasChildNodes)
				{
					foreach (HtmlNode subnode in ChildNodes)
					{
						subnode.WriteTo(writer);
					}
				}
				break;
			case HtmlNodeType.Text:
				string html = ((HtmlTextNode) this).Text;
				writer.WriteString(html);
				break;
			case HtmlNodeType.Element:
				string name = _ownerdocument.OptionOutputUpperCase ? Name.ToUpperInvariant() : Name;
				if (_ownerdocument.OptionOutputOriginalCase)
					name = OriginalName;
				writer.WriteStartElement(name);
				WriteAttributes(writer, this);
				if (HasChildNodes)
				{
					foreach (HtmlNode subnode in ChildNodes)
					{
						subnode.WriteTo(writer);
					}
				}
				writer.WriteEndElement();
				break;
		}
	}
	public string WriteTo()
	{
		using (StringWriter sw = new StringWriter())
		{
			WriteTo(sw);
			sw.Flush();
			return sw.ToString();
		}
	}
	public void SetParent(HtmlNode parent)
	{
		if (parent == null)
			return;
		ParentNode = parent;
		if (OwnerDocument.OptionMaxNestedChildNodes > 0)
		{
			Depth = parent.Depth + 1;
			if (Depth > OwnerDocument.OptionMaxNestedChildNodes)
				throw new Exception(string.Format("Document has more than {0} nested tags. This is likely due to the page not closing tags properly.", OwnerDocument.OptionMaxNestedChildNodes));
		}
	}
	internal void SetChanged()
	{
		_changed = true;
		if (ParentNode != null)
		{
			ParentNode.SetChanged();
		}
	}
	private void UpdateHtml()
	{
		_innerhtml = WriteContentTo();
		_outerhtml = WriteTo();
		_changed = false;
	}
	internal static string GetXmlComment(HtmlCommentNode comment)
	{
		string s = comment.Comment;
		s = s.Substring(4, s.Length - 7).Replace("--", " - -");
		return s;
	}
	internal static void WriteAttributes(XmlWriter writer, HtmlNode node)
	{
		if (!node.HasAttributes)
		{
			return;
		}
		foreach (HtmlAttribute att in node.Attributes.Hashitems.Values)
		{
			writer.WriteAttributeString(att.XmlName, att.Value);
		}
	}
	internal void UpdateLastNode()
	{
		HtmlNode newLast = null;
		if (_prevwithsamename == null || !_prevwithsamename._starttag)
		{
			if (_ownerdocument.Openednodes != null)
			{
				foreach (var openNode in _ownerdocument.Openednodes)
				{
					if ((openNode.Key < _outerstartindex || openNode.Key > (_outerstartindex + _outerlength)) && openNode.Value._name == _name)
					{
						if (newLast == null && openNode.Value._starttag)
						{
							newLast = openNode.Value;
						}
						else if (newLast != null && newLast.InnerStartIndex < openNode.Key && openNode.Value._starttag)
						{
							newLast = openNode.Value;
						}
					}
				}
			}
		}
		else
		{
			newLast = _prevwithsamename;
		}
		if (newLast != null)
		{
			_ownerdocument.Lastnodes[newLast.Name] = newLast;
		}
	}
	internal void CloseNode(HtmlNode endnode, int level = 0)
	{
		if (level > HtmlDocument.MaxDepthLevel)
		{
			throw new ArgumentException(HtmlNode.DepthLevelExceptionMessage);
		}
		if (!_ownerdocument.OptionAutoCloseOnEnd)
		{
			if (_childnodes != null)
			{
				foreach (HtmlNode child in _childnodes)
				{
					if (child.Closed)
						continue;
					HtmlNode close = new HtmlNode(NodeType, _ownerdocument, -1);
					close._endnode = close;
					child.CloseNode(close, level + 1);
				}
			}
		}
		if (!Closed)
		{
			_endnode = endnode;
			if (_ownerdocument.Openednodes != null)
				_ownerdocument.Openednodes.Remove(_outerstartindex);
			HtmlNode self = Utilities.GetDictionaryValueOrDefault(_ownerdocument.Lastnodes, Name);
			if (self == this)
			{
				_ownerdocument.Lastnodes.Remove(Name);
				_ownerdocument.UpdateLastParentNode();
				if (_starttag && !String.IsNullOrEmpty(Name))
				{
					UpdateLastNode();
				}
			}
			if (endnode == this)
				return;
			_innerstartindex = _outerstartindex + _outerlength;
			_innerlength = endnode._outerstartindex - _innerstartindex;
			_outerlength = (endnode._outerstartindex + endnode._outerlength) - _outerstartindex; 
		}
	}
	internal string GetId()
	{
		HtmlAttribute att = Attributes["id"];
		return att == null ? string.Empty : att.Value;
	}
	internal void SetId(string id)
	{
		HtmlAttribute att = Attributes["id"] ?? _ownerdocument.CreateAttribute("id");
		att.Value = id;
		_ownerdocument.SetIdForNode(this, att.Value);
		Attributes["id"] = att;
		SetChanged();
	}
	internal void WriteAttribute(TextWriter outText, HtmlAttribute att)
	{
		if (att.Value == null)
		{
			return;
		}
		var quoteType = OwnerDocument.GlobalAttributeValueQuote ?? att.QuoteType;
		var isWithoutValue = quoteType == AttributeValueQuote.WithoutValue
					 || (quoteType == AttributeValueQuote.Initial && att._isFromParse && !att._hasEqual && string.IsNullOrEmpty(att.XmlValue));
		if (quoteType == AttributeValueQuote.Initial && !(att._isFromParse && !att._hasEqual && string.IsNullOrEmpty(att.XmlValue)))
		{
			quoteType = att.InternalQuoteType;
		}
		string name;
		string quote = quoteType == AttributeValueQuote.DoubleQuote ? "\"" : quoteType == AttributeValueQuote.SingleQuote ? "'" : "";
		if (_ownerdocument.OptionOutputAsXml)
		{
			name = _ownerdocument.OptionOutputUpperCase ? att.XmlName.ToUpperInvariant(): att.XmlName;
			if (_ownerdocument.OptionOutputOriginalCase)
				name = att.OriginalName;
			if (!isWithoutValue)
			{ 
				outText.Write(" " + name + "=" + quote + HtmlDocument.HtmlEncodeWithCompatibility(att.XmlValue, _ownerdocument.BackwardCompatibility) + quote);
			}
			else
			{ 
				outText.Write(" " + name);
			}
		}
		else
		{
			name = _ownerdocument.OptionOutputUpperCase ? att.Name.ToUpperInvariant() : att.Name;
			if (_ownerdocument.OptionOutputOriginalCase)
				name = att.OriginalName;
			if (att.Name.Length >= 4)
			{
				if ((att.Name[0] == '<') && (att.Name[1] == '%') &&
					(att.Name[att.Name.Length - 1] == '>') && (att.Name[att.Name.Length - 2] == '%'))
				{
					outText.Write(" " + name);
					return;
				}
			}
			if (!isWithoutValue)
			{
				var value = quoteType == AttributeValueQuote.DoubleQuote ? !att.Value.StartsWith("@") ? att.Value.Replace("\"", "&quot;") :
			   att.Value : quoteType == AttributeValueQuote.SingleQuote ?  att.Value.Replace("'", "&#39;") : att.Value;
				if (_ownerdocument.OptionOutputOptimizeAttributeValues)
					if (att.Value.IndexOfAny(new char[] {(char) 10, (char) 13, (char) 9, ' '}) < 0)
						outText.Write(" " + name + "=" + att.Value);
					else
						outText.Write(" " + name + "=" + quote + value + quote);
				else
					outText.Write(" " + name + "=" + quote + value + quote);
			}
			else
			{
				outText.Write(" " + name);
			}
		}
	}
	internal void WriteAttributes(TextWriter outText, bool closing)
	{
		if (_ownerdocument.OptionOutputAsXml)
		{
			if (_attributes == null)
			{
				return;
			}
			foreach (HtmlAttribute att in _attributes.Hashitems.Values)
			{
				WriteAttribute(outText, att);
			}
			return;
		}
		if (!closing)
		{
			if (_attributes != null)
				foreach (HtmlAttribute att in _attributes)
					WriteAttribute(outText, att);
			if (!_ownerdocument.OptionAddDebuggingAttributes) return;
			WriteAttribute(outText, _ownerdocument.CreateAttribute("_closed", Closed.ToString()));
			WriteAttribute(outText, _ownerdocument.CreateAttribute("_children", ChildNodes.Count.ToString()));
			int i = 0;
			foreach (HtmlNode n in ChildNodes)
			{
				WriteAttribute(outText, _ownerdocument.CreateAttribute("_child_" + i,
					n.Name));
				i++;
			}
		}
		else
		{
			if (_endnode == null || _endnode._attributes == null || _endnode == this)
				return;
			foreach (HtmlAttribute att in _endnode._attributes)
				WriteAttribute(outText, att);
			if (!_ownerdocument.OptionAddDebuggingAttributes) return;
			WriteAttribute(outText, _ownerdocument.CreateAttribute("_closed", Closed.ToString()));
			WriteAttribute(outText, _ownerdocument.CreateAttribute("_children", ChildNodes.Count.ToString()));
		}
	}
	private string GetRelativeXpath()
	{
		if (ParentNode == null)
			return Name;
		if (NodeType == HtmlNodeType.Document)
			return string.Empty;
		int i = 1;
		foreach (HtmlNode node in ParentNode.ChildNodes)
		{
			if (node.Name != Name) continue;
			if (node == this)
				break;
			i++;
		}
		return Name + "[" + i + "]";
	}
	private bool IsSingleElementNode()
	{
		int count = 0;
		var element = FirstChild;
		while (element != null)
		{
			if (element.NodeType == HtmlNodeType.Element && element.OuterHtml != "\r\n")
				count++;
			element = element.NextSibling;
		}
		return count <= 1 ? true : false;
	}
	public void AddClass(string name)
	{
		AddClass(name, false);
	}
	public void AddClass(string name, bool throwError)
	{
		var classAttributes = Attributes.AttributesWithName("class");
		if (!IsEmpty(classAttributes))
		{
			foreach (HtmlAttribute att in classAttributes)
			{ 
				if (att.Value != null && att.Value.Split(' ').ToList().Any(x => x.Equals(name)))
				{
					if (throwError)
					{
						throw new Exception(HtmlDocument.HtmlExceptionClassExists);
					}
				}
				else
				{
					SetAttributeValue(att.Name, att.Value + " " + name);
				}
			}
		}
		else
		{
			HtmlAttribute attribute = _ownerdocument.CreateAttribute("class", name);
			Attributes.Append(attribute);
		}
	}
	public void RemoveClass()
	{
		RemoveClass(false);
	}
	public void RemoveClass(bool throwError)
	{
		var classAttributes = Attributes.AttributesWithName("class");
		if (IsEmpty(classAttributes) && throwError)
		{
			throw new Exception(HtmlDocument.HtmlExceptionClassDoesNotExist);
		}
		foreach (var att in classAttributes)
		{
			Attributes.Remove(att);
		}
	}
	public void RemoveClass(string name)
	{
		RemoveClass(name, false);
	}
	public void RemoveClass(string name, bool throwError)
	{
		var classAttributes = Attributes.AttributesWithName("class");
		if (IsEmpty(classAttributes) && throwError)
		{
			throw new Exception(HtmlDocument.HtmlExceptionClassDoesNotExist);
		}
		else
		{
			foreach (var att in classAttributes)
			{
				if (att.Value == null)
				{
					continue;
				}
				if (att.Value.Equals(name))
				{
					Attributes.Remove(att);
				}
				else if (att.Value != null && att.Value.Split(' ').ToList().Any(x => x.Equals(name)))
				{
					string[] classNames = att.Value.Split(' ');
					string newClassNames = "";
					foreach (string item in classNames)
					{
						if (!item.Equals(name))
							newClassNames += item + " ";
					}
					newClassNames = newClassNames.Trim();
					SetAttributeValue(att.Name, newClassNames);
				}
				else
				{
					if (throwError)
					{
						throw new Exception(HtmlDocument.HtmlExceptionClassDoesNotExist);
					}
				}
				if (string.IsNullOrEmpty(att.Value))
				{
					Attributes.Remove(att);
				}
			}
		}
	}
	public void ReplaceClass(string newClass, string oldClass)
	{
		ReplaceClass(newClass, oldClass, false);
	}
	public void ReplaceClass(string newClass, string oldClass, bool throwError)
	{
		if (string.IsNullOrEmpty(newClass))
		{
			RemoveClass(oldClass);
		}
		if (string.IsNullOrEmpty(oldClass))
		{
			AddClass(newClass);
		}
		var classAttributes = Attributes.AttributesWithName("class");
		if (IsEmpty(classAttributes) && throwError)
		{
			throw new Exception(HtmlDocument.HtmlExceptionClassDoesNotExist);
		}
		foreach (var att in classAttributes)
		{
			if (att.Value == null)
			{
				continue;
			}
			if (att.Value.Equals(oldClass) || att.Value.Contains(oldClass))
			{
				string newClassNames = att.Value.Replace(oldClass, newClass);
				SetAttributeValue(att.Name, newClassNames);
			}
			else if (throwError)
			{
				throw new Exception(HtmlDocument.HtmlExceptionClassDoesNotExist);
			}
		}
	}
	public IEnumerable<string> GetClasses()
	{
		var classAttributes = Attributes.AttributesWithName("class");
		foreach (var att in classAttributes)
		{
			var classNames = att.Value.Split(null as char[], StringSplitOptions.RemoveEmptyEntries);
			foreach (var className in classNames)
			{
				yield return className;
			}
		}
	}
	public bool HasClass(string className)
	{
		var classes = GetClasses();
		foreach (var @class in classes)
		{
			var classNames = @class.Split(null as char[], StringSplitOptions.RemoveEmptyEntries);
			foreach (var theClassName in classNames)
			{
				if (theClassName == className)
				{
					return true;
				}
			}
		}
		return false;
	}
	private bool IsEmpty(IEnumerable en)
	{
		foreach (var c in en)
		{
			return false;
		}
		return true;
	}
}
public partial class HtmlNode : IXPathNavigable
{
	public XPathNavigator CreateNavigator()
	{
		return new HtmlNodeNavigator(OwnerDocument, this);
	}
	public XPathNavigator CreateRootNavigator()
	{
		return new HtmlNodeNavigator(OwnerDocument, OwnerDocument.DocumentNode);
	}
	public HtmlNodeCollection SelectNodes(string xpath)
	{
		HtmlNodeCollection list = new HtmlNodeCollection(null);
		HtmlNodeNavigator nav = new HtmlNodeNavigator(OwnerDocument, this);
		XPathNodeIterator it = nav.Select(xpath);
		while (it.MoveNext())
		{
			HtmlNodeNavigator n = (HtmlNodeNavigator) it.Current;
			list.Add(n.CurrentNode, false);
		}
		if (list.Count == 0 && !OwnerDocument.OptionEmptyCollection)
		{
			return null;
		}
		return list;
	}
	public HtmlNodeCollection SelectNodes(XPathExpression xpath)
	{ 
		HtmlNodeCollection list = new HtmlNodeCollection(null);
		HtmlNodeNavigator nav = new HtmlNodeNavigator(OwnerDocument, this);
		XPathNodeIterator it = nav.Select(xpath);
		while (it.MoveNext())
		{
			HtmlNodeNavigator n = (HtmlNodeNavigator) it.Current;
			list.Add(n.CurrentNode, false);
		}
		if (list.Count == 0 && !OwnerDocument.OptionEmptyCollection)
		{
			return null;
		}
		return list;
	}
	public HtmlNode SelectSingleNode(string xpath)
	{
		if (xpath == null)
		{
			throw new ArgumentNullException("xpath");
		}
		HtmlNodeNavigator nav = new HtmlNodeNavigator(OwnerDocument, this);
		XPathNodeIterator it = nav.Select(xpath);
		if (!it.MoveNext())
		{
			return null;
		}
		HtmlNodeNavigator node = (HtmlNodeNavigator) it.Current;
		return node.CurrentNode;
	}
	public HtmlNode SelectSingleNode(XPathExpression xpath)
	{
		if (xpath == null)
		{
			throw new ArgumentNullException("xpath");
		}
		HtmlNodeNavigator nav = new HtmlNodeNavigator(OwnerDocument, this);
		XPathNodeIterator it = nav.Select(xpath);
		if (!it.MoveNext())
		{
			return null;
		}
		HtmlNodeNavigator node = (HtmlNodeNavigator)it.Current;
		return node.CurrentNode;
	}
}
public class HtmlNodeCollection : IList<HtmlNode>
{
	private readonly HtmlNode _parentnode;
	private readonly List<HtmlNode> _items = new List<HtmlNode>();
	public HtmlNodeCollection(HtmlNode parentnode)
	{
		_parentnode = parentnode; // may be null
	}
	internal HtmlNode ParentNode
	{
		get
		{
			return _parentnode;
		}
	}
	public int this[HtmlNode node]
	{
		get
		{
			int index = GetNodeIndex(node);
			if (index == -1)
				throw new ArgumentOutOfRangeException("node",
					"Node \"" + node.CloneNode(false).OuterHtml +
					"\" was not found in the collection");
			return index;
		}
	}
	public HtmlNode this[string nodeName]
	{
		get
		{
			for (int i = 0; i < _items.Count; i++)
				if (string.Equals(_items[i].Name, nodeName, StringComparison.OrdinalIgnoreCase))
					return _items[i];
			return null;
		}
	}
	public int Count
	{
		get { return _items.Count; }
	}
	public bool IsReadOnly
	{
		get { return false; }
	}
	public HtmlNode this[int index]
	{
		get { return _items[index]; }
		set { _items[index] = value; }
	}
	public void Add(HtmlNode node)
	{
		Add(node, true);
	}
	public void Add(HtmlNode node, bool setParent)
	{
		_items.Add(node);
		if (setParent)
		{
			node.ParentNode = _parentnode;
		}
	}
	public void Clear()
	{
		foreach (HtmlNode node in _items)
		{
			node.ParentNode = null;
			node.NextSibling = null;
			node.PreviousSibling = null;
		}
		_items.Clear();
	}
	public bool Contains(HtmlNode item)
	{
		return _items.Contains(item);
	}
	public void CopyTo(HtmlNode[] array, int arrayIndex)
	{
		_items.CopyTo(array, arrayIndex);
	}
	IEnumerator<HtmlNode> IEnumerable<HtmlNode>.GetEnumerator()
	{
		return _items.GetEnumerator();
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		return _items.GetEnumerator();
	}
	public int IndexOf(HtmlNode item)
	{
		return _items.IndexOf(item);
	}
	public void Insert(int index, HtmlNode node)
	{
		HtmlNode next = null;
		HtmlNode prev = null;
		if (index > 0)
			prev = _items[index - 1];
		if (index < _items.Count)
			next = _items[index];
		_items.Insert(index, node);
		if (prev != null)
		{
			if (node == prev)
				throw new InvalidProgramException("Unexpected error.");
			prev._nextnode = node;
		}
		if (next != null)
			next._prevnode = node;
		node._prevnode = prev;
		if (next == node)
			throw new InvalidProgramException("Unexpected error.");
		node._nextnode = next; 
		node.SetParent(_parentnode);
	}
	public bool Remove(HtmlNode item)
	{
		int i = _items.IndexOf(item);
		RemoveAt(i);
		return true;
	}
	public void RemoveAt(int index)
	{
		HtmlNode next = null;
		HtmlNode prev = null;
		HtmlNode oldnode = _items[index];
		var parentNode = _parentnode ?? oldnode._parentnode;
		if (index > 0)
			prev = _items[index - 1];
		if (index < (_items.Count - 1))
			next = _items[index + 1];
		_items.RemoveAt(index);
		if (prev != null)
		{
			if (next == prev)
				throw new InvalidProgramException("Unexpected error.");
			prev._nextnode = next;
		}
		if (next != null)
			next._prevnode = prev;
		oldnode._prevnode = null;
		oldnode._nextnode = null;
		oldnode._parentnode = null;
		if (parentNode != null)
		{
			parentNode.SetChanged();
		}
	}
	public static HtmlNode FindFirst(HtmlNodeCollection items, string name)
	{
		foreach (HtmlNode node in items)
		{
			if (node.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				return node;
			if (!node.HasChildNodes) continue;
			HtmlNode returnNode = FindFirst(node.ChildNodes, name);
			if (returnNode != null)
				return returnNode;
		}
		return null;
	}
	public void Append(HtmlNode node)
	{
		HtmlNode last = null;
		if (_items.Count > 0)
			last = _items[_items.Count - 1];
		_items.Add(node);
		node._prevnode = last;
		node._nextnode = null;
		node.SetParent(_parentnode);
		if (last == null) return;
		if (last == node)
			throw new InvalidProgramException("Unexpected error.");
		last._nextnode = node;
	}
	public HtmlNode FindFirst(string name)
	{
		return FindFirst(this, name);
	}
	public int GetNodeIndex(HtmlNode node)
	{
		for (int i = 0; i < _items.Count; i++)
			if (node == _items[i])
				return i;
		return -1;
	}
	public void Prepend(HtmlNode node)
	{
		HtmlNode first = null;
		if (_items.Count > 0)
			first = _items[0];
		_items.Insert(0, node);
		if (node == first)
			throw new InvalidProgramException("Unexpected error.");
		node._nextnode = first;
		node._prevnode = null;
		node.SetParent(_parentnode);
		if (first != null)
			first._prevnode = node;
	}
	public bool Remove(int index)
	{
		RemoveAt(index);
		return true;
	}
	public void Replace(int index, HtmlNode node)
	{
		HtmlNode next = null;
		HtmlNode prev = null;
		HtmlNode oldnode = _items[index];
		if (index > 0)
			prev = _items[index - 1];
		if (index < (_items.Count - 1))
			next = _items[index + 1];
		_items[index] = node;
		if (prev != null)
		{
			if (node == prev)
				throw new InvalidProgramException("Unexpected error.");
			prev._nextnode = node;
		}
		if (next != null)
			next._prevnode = node;
		node._prevnode = prev;
		if (next == node)
			throw new InvalidProgramException("Unexpected error.");
		node._nextnode = next;
		node.SetParent(_parentnode);
		oldnode._prevnode = null;
		oldnode._nextnode = null;
		oldnode._parentnode = null;
	}
	public IEnumerable<HtmlNode> Descendants()
	{
		foreach (HtmlNode item in _items)
		foreach (HtmlNode n in item.Descendants())
			yield return n;
	}
	public IEnumerable<HtmlNode> Descendants(string name)
	{
		foreach (HtmlNode item in _items)
		foreach (HtmlNode n in item.Descendants(name))
			yield return n;
	}
	public IEnumerable<HtmlNode> Elements()
	{
		foreach (HtmlNode item in _items)
		foreach (HtmlNode n in item.ChildNodes)
			yield return n;
	}
	public IEnumerable<HtmlNode> Elements(string name)
	{
		foreach (HtmlNode item in _items)
		foreach (HtmlNode n in item.Elements(name))
			yield return n;
	}
	public IEnumerable<HtmlNode> Nodes()
	{
		foreach (HtmlNode item in _items)
		foreach (HtmlNode n in item.ChildNodes)
			yield return n;
	}
}
public class HtmlNodeNavigator : XPathNavigator
{
	private int _attindex;
	private HtmlNode _currentnode;
	private readonly HtmlDocument _doc;
	private readonly HtmlNameTable _nametable;
	internal bool Trace;
	internal HtmlNodeNavigator()
	{
		_doc = new HtmlDocument();
		_nametable = new HtmlNameTable();
		Reset();
	}
	internal HtmlNodeNavigator(HtmlDocument doc, HtmlNode currentNode)
	{
		if (currentNode == null)
		{
			throw new ArgumentNullException("currentNode");
		}
		if (currentNode.OwnerDocument != doc)
		{
			throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
		}
		if (doc == null)
		{
			throw new Exception("Oops! The HtmlDocument cannot be null.");
		}
#if TRACE_NAVIGATOR
		InternalTrace(null);
#endif
		_doc = doc;
		_nametable = new HtmlNameTable();
		Reset();
		_currentnode = currentNode;
	}
	private HtmlNodeNavigator(HtmlNodeNavigator nav)
	{
		if (nav == null)
		{
			throw new ArgumentNullException("nav");
		}
#if TRACE_NAVIGATOR
		InternalTrace(null);
#endif
		_doc = nav._doc;
		_currentnode = nav._currentnode;
		_attindex = nav._attindex;
		_nametable = nav._nametable; // REVIEW: should we do this?
	}
	public HtmlNodeNavigator(Stream stream)
	{
		_doc = new HtmlDocument();
		_nametable = new HtmlNameTable();
		_doc.Load(stream);
		Reset();
	}
	public HtmlNodeNavigator(Stream stream, bool detectEncodingFromByteOrderMarks)
	{
		_doc = new HtmlDocument();
		_nametable = new HtmlNameTable();
		_doc.Load(stream, detectEncodingFromByteOrderMarks);
		Reset();
	}
	public HtmlNodeNavigator(Stream stream, Encoding encoding)
	{
		_doc = new HtmlDocument();
		_nametable = new HtmlNameTable();
		_doc.Load(stream, encoding);
		Reset();
	}
	public HtmlNodeNavigator(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
	{
		_doc = new HtmlDocument();
		_nametable = new HtmlNameTable();
		_doc.Load(stream, encoding, detectEncodingFromByteOrderMarks);
		Reset();
	}
	public HtmlNodeNavigator(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
	{
		_doc = new HtmlDocument();
		_nametable = new HtmlNameTable();
		_doc.Load(stream, encoding, detectEncodingFromByteOrderMarks, buffersize);
		Reset();
	}
	public HtmlNodeNavigator(TextReader reader)
	{
		_doc = new HtmlDocument();
		_nametable = new HtmlNameTable();
		_doc.Load(reader);
		Reset();
	}
#if !(NETSTANDARD1_3 || NETSTANDARD1_6)
	public HtmlNodeNavigator(string path)
	{
		_doc = new HtmlDocument();
		_nametable = new HtmlNameTable();
		_doc.Load(path);
		Reset();
	}
	public HtmlNodeNavigator(string path, bool detectEncodingFromByteOrderMarks)
	{
		_doc = new HtmlDocument();
		_nametable = new HtmlNameTable();
		_doc.Load(path, detectEncodingFromByteOrderMarks);
		Reset();
	}
	public HtmlNodeNavigator(string path, Encoding encoding)
	{
		_doc = new HtmlDocument();
		_nametable = new HtmlNameTable();
		_doc.Load(path, encoding);
		Reset();
	}
	public HtmlNodeNavigator(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
	{
		_doc = new HtmlDocument();
		_nametable = new HtmlNameTable();
		_doc.Load(path, encoding, detectEncodingFromByteOrderMarks);
		Reset();
	}
	public HtmlNodeNavigator(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
	{
		_doc = new HtmlDocument();
		_nametable = new HtmlNameTable();
		_doc.Load(path, encoding, detectEncodingFromByteOrderMarks, buffersize);
		Reset();
	}
#endif
	public override string BaseURI
	{
		get
		{
#if TRACE_NAVIGATOR
			InternalTrace(">");
#endif
			return _nametable.GetOrAdd(string.Empty);
		}
	}
	public HtmlDocument CurrentDocument
	{
		get { return _doc; }
	}
	public HtmlNode CurrentNode
	{
		get { return _currentnode; }
	}
	public override bool HasAttributes
	{
		get
		{
#if TRACE_NAVIGATOR
			InternalTrace(">" + (_currentnode.Attributes.Count > 0));
#endif
			return (_currentnode.Attributes.Count > 0);
		}
	}
	public override bool HasChildren
	{
		get
		{
#if TRACE_NAVIGATOR
			InternalTrace(">" + (_currentnode.ChildNodes.Count > 0));
#endif
			return (_currentnode.ChildNodes.Count > 0);
		}
	}
	public override bool IsEmptyElement
	{
		get
		{
#if TRACE_NAVIGATOR
			InternalTrace(">" + !HasChildren);
#endif
			return !HasChildren;
		}
	}
	public override string LocalName
	{
		get
		{
			if (_attindex != -1)
			{
#if TRACE_NAVIGATOR
				InternalTrace("att>" + _currentnode.Attributes[_attindex].Name);
#endif
				return _nametable.GetOrAdd(_currentnode.Attributes[_attindex].Name);
			}
#if TRACE_NAVIGATOR
			InternalTrace("node>" + _currentnode.Name);
#endif
			return _nametable.GetOrAdd(_currentnode.Name);
		}
	}
	public override string Name
	{
		get
		{
#if TRACE_NAVIGATOR
			InternalTrace(">" + _currentnode.Name);
#endif
			return _nametable.GetOrAdd(_currentnode.Name);
		}
	}
	public override string NamespaceURI
	{
		get
		{
#if TRACE_NAVIGATOR
			InternalTrace(">");
#endif
			return _nametable.GetOrAdd(string.Empty);
		}
	}
	public override XmlNameTable NameTable
	{
		get
		{
#if TRACE_NAVIGATOR
			InternalTrace(null);
#endif
			return _nametable;
		}
	}
	public override XPathNodeType NodeType
	{
		get
		{
			switch (_currentnode.NodeType)
			{
				case HtmlNodeType.Comment:
#if TRACE_NAVIGATOR
					InternalTrace(">" + XPathNodeType.Comment);
#endif
					return XPathNodeType.Comment;
				case HtmlNodeType.Document:
#if TRACE_NAVIGATOR
					InternalTrace(">" + XPathNodeType.Root);
#endif
					return XPathNodeType.Root;
				case HtmlNodeType.Text:
#if TRACE_NAVIGATOR
					InternalTrace(">" + XPathNodeType.Text);
#endif
					return XPathNodeType.Text;
				case HtmlNodeType.Element:
					{
						if (_attindex != -1)
						{
#if TRACE_NAVIGATOR
						InternalTrace(">" + XPathNodeType.Attribute);
#endif
							return XPathNodeType.Attribute;
						}
#if TRACE_NAVIGATOR
					InternalTrace(">" + XPathNodeType.Element);
#endif
						return XPathNodeType.Element;
					}
				default:
					throw new NotImplementedException("Internal error: Unhandled HtmlNodeType: " +
													  _currentnode.NodeType);
			}
		}
	}
	public override string Prefix
	{
		get
		{
#if TRACE_NAVIGATOR
			InternalTrace(null);
#endif
			return _nametable.GetOrAdd(string.Empty);
		}
	}
	public override string Value
	{
		get
		{
#if TRACE_NAVIGATOR
			InternalTrace("nt=" + _currentnode.NodeType);
#endif
			switch (_currentnode.NodeType)
			{
				case HtmlNodeType.Comment:
#if TRACE_NAVIGATOR
					InternalTrace(">" + ((HtmlCommentNode) _currentnode).Comment);
#endif
					return ((HtmlCommentNode) _currentnode).Comment;
				case HtmlNodeType.Document:
#if TRACE_NAVIGATOR
					InternalTrace(">");
#endif
					return "";
				case HtmlNodeType.Text:
#if TRACE_NAVIGATOR
					InternalTrace(">" + ((HtmlTextNode) _currentnode).Text);
#endif
					return ((HtmlTextNode) _currentnode).Text;
				case HtmlNodeType.Element:
					{
						if (_attindex != -1)
						{
#if TRACE_NAVIGATOR
						InternalTrace(">" + _currentnode.Attributes[_attindex].Value);
#endif
							return _currentnode.Attributes[_attindex].Value;
						}
						return _currentnode.InnerText;
					}
				default:
					throw new NotImplementedException("Internal error: Unhandled HtmlNodeType: " +
													  _currentnode.NodeType);
			}
		}
	}
	public override string XmlLang
	{
		get
		{
#if TRACE_NAVIGATOR
			InternalTrace(null);
#endif
			return _nametable.GetOrAdd(string.Empty);
		}
	}
	public override XPathNavigator Clone()
	{
#if TRACE_NAVIGATOR
		InternalTrace(null);
#endif
		return new HtmlNodeNavigator(this);
	}
	public override string GetAttribute(string localName, string namespaceURI)
	{
#if TRACE_NAVIGATOR
		InternalTrace("localName=" + localName + ", namespaceURI=" + namespaceURI);
#endif
		HtmlAttribute att = _currentnode.Attributes[localName];
		if (att == null)
		{
#if TRACE_NAVIGATOR
			InternalTrace(">null");
#endif
			return null;
		}
#if TRACE_NAVIGATOR
		InternalTrace(">" + att.Value);
#endif
		return att.Value;
	}
	public override string GetNamespace(string name)
	{
#if TRACE_NAVIGATOR
		InternalTrace("name=" + name);
#endif
		return string.Empty;
	}
	public override bool IsSamePosition(XPathNavigator other)
	{
		HtmlNodeNavigator nav = other as HtmlNodeNavigator;
		if (nav == null)
		{
#if TRACE_NAVIGATOR
			InternalTrace(">false");
#endif
			return false;
		}
#if TRACE_NAVIGATOR
		InternalTrace(">" + (nav._currentnode == _currentnode));
#endif
		return (nav._currentnode == _currentnode);
	}
	public override bool MoveTo(XPathNavigator other)
	{
		HtmlNodeNavigator nav = other as HtmlNodeNavigator;
		if (nav == null)
		{
#if TRACE_NAVIGATOR
			InternalTrace(">false (nav is not an HtmlNodeNavigator)");
#endif
			return false;
		}
#if TRACE_NAVIGATOR
		InternalTrace("moveto oid=" + nav.GetHashCode()
									+ ", n:" + nav._currentnode.Name
									+ ", a:" + nav._attindex);
#endif
		if (nav._doc == _doc)
		{
			_currentnode = nav._currentnode;
			_attindex = nav._attindex;
#if TRACE_NAVIGATOR
			InternalTrace(">true");
#endif
			return true;
		}
#if TRACE_NAVIGATOR
		InternalTrace(">false (???)");
#endif
		return false;
	}
	public override bool MoveToAttribute(string localName, string namespaceURI)
	{
#if TRACE_NAVIGATOR
		InternalTrace("localName=" + localName + ", namespaceURI=" + namespaceURI);
#endif
		int index = _currentnode.Attributes.GetAttributeIndex(localName);
		if (index == -1)
		{
#if TRACE_NAVIGATOR
			InternalTrace(">false");
#endif
			return false;
		}
		_attindex = index;
#if TRACE_NAVIGATOR
		InternalTrace(">true");
#endif
		return true;
	}
	public override bool MoveToFirst()
	{
		if (_currentnode.ParentNode == null)
		{
#if TRACE_NAVIGATOR
			InternalTrace(">false");
#endif
			return false;
		}
		if (_currentnode.ParentNode.FirstChild == null)
		{
#if TRACE_NAVIGATOR
			InternalTrace(">false");
#endif
			return false;
		}
		_currentnode = _currentnode.ParentNode.FirstChild;
#if TRACE_NAVIGATOR
		InternalTrace(">true");
#endif
		return true;
	}
	public override bool MoveToFirstAttribute()
	{
		if (!HasAttributes)
		{
#if TRACE_NAVIGATOR
			InternalTrace(">false");
#endif
			return false;
		}
		_attindex = 0;
#if TRACE_NAVIGATOR
		InternalTrace(">true");
#endif
		return true;
	}
	public override bool MoveToFirstChild()
	{
		if (!_currentnode.HasChildNodes)
		{
#if TRACE_NAVIGATOR
			InternalTrace(">false");
#endif
			return false;
		}
		_currentnode = _currentnode.ChildNodes[0];
#if TRACE_NAVIGATOR
		InternalTrace(">true");
#endif
		return true;
	}
	public override bool MoveToFirstNamespace(XPathNamespaceScope scope)
	{
#if TRACE_NAVIGATOR
		InternalTrace(null);
#endif
		return false;
	}
	public override bool MoveToId(string id)
	{
#if TRACE_NAVIGATOR
		InternalTrace("id=" + id);
#endif
		HtmlNode node = _doc.GetElementbyId(id);
		if (node == null)
		{
#if TRACE_NAVIGATOR
			InternalTrace(">false");
#endif
			return false;
		}
		_currentnode = node;
#if TRACE_NAVIGATOR
		InternalTrace(">true");
#endif
		return true;
	}
	public override bool MoveToNamespace(string name)
	{
#if TRACE_NAVIGATOR
		InternalTrace("name=" + name);
#endif
		return false;
	}
	public override bool MoveToNext()
	{
		if (_currentnode.NextSibling == null)
		{
#if TRACE_NAVIGATOR
			InternalTrace(">false");
#endif
			return false;
		}
#if TRACE_NAVIGATOR
		InternalTrace("_c=" + _currentnode.CloneNode(false).OuterHtml);
		InternalTrace("_n=" + _currentnode.NextSibling.CloneNode(false).OuterHtml);
#endif
		_currentnode = _currentnode.NextSibling;
#if TRACE_NAVIGATOR
		InternalTrace(">true");
#endif
		return true;
	}
	public override bool MoveToNextAttribute()
	{
#if TRACE_NAVIGATOR
		InternalTrace(null);
#endif
		if (_attindex >= (_currentnode.Attributes.Count - 1))
		{
#if TRACE_NAVIGATOR
			InternalTrace(">false");
#endif
			return false;
		}
		_attindex++;
#if TRACE_NAVIGATOR
		InternalTrace(">true");
#endif
		return true;
	}
	public override bool MoveToNextNamespace(XPathNamespaceScope scope)
	{
#if TRACE_NAVIGATOR
		InternalTrace(null);
#endif
		return false;
	}
	public override bool MoveToParent()
	{
		if (_currentnode.ParentNode == null)
		{
#if TRACE_NAVIGATOR
			InternalTrace(">false");
#endif
			return false;
		}
		_currentnode = _currentnode.ParentNode;
#if TRACE_NAVIGATOR
		InternalTrace(">true");
#endif
		return true;
	}
	public override bool MoveToPrevious()
	{
		if (_currentnode.PreviousSibling == null)
		{
#if TRACE_NAVIGATOR
			InternalTrace(">false");
#endif
			return false;
		}
		_currentnode = _currentnode.PreviousSibling;
#if TRACE_NAVIGATOR
		InternalTrace(">true");
#endif
		return true;
	}
	public override void MoveToRoot()
	{
		_currentnode = _doc.DocumentNode;
#if TRACE_NAVIGATOR
		InternalTrace(null);
#endif
	}
#if TRACE_NAVIGATOR
	[Conditional("TRACE")]
	internal void InternalTrace(object traceValue)
	{
		if (!Trace)
		{
			return;
		}
#if !(NETSTANDARD1_3 || NETSTANDARD1_6)
		StackFrame sf = new StackFrame(1);
		string name = sf.GetMethod().Name;
#else
		string name = "";
#endif
		string nodename = _currentnode == null ? "(null)" : _currentnode.Name;
		string nodevalue;
		if (_currentnode == null)
		{
			nodevalue = "(null)";
		}
		else
		{
			switch (_currentnode.NodeType)
			{
				case HtmlNodeType.Comment:
					nodevalue = ((HtmlCommentNode) _currentnode).Comment;
					break;
				case HtmlNodeType.Document:
					nodevalue = "";
					break;
				case HtmlNodeType.Text:
					nodevalue = ((HtmlTextNode) _currentnode).Text;
					break;
				default:
					nodevalue = _currentnode.CloneNode(false).OuterHtml;
					break;
			}
		}
		HtmlAgilityPack.Trace.WriteLine(string.Format("oid={0},n={1},a={2},v={3},{4}", GetHashCode(), nodename, _attindex, nodevalue, traceValue), "N!" + name);
	}
#endif
	private void Reset()
	{
#if TRACE_NAVIGATOR
		InternalTrace(null);
#endif
		_currentnode = _doc.DocumentNode;
		_attindex = -1;
	}
}
public enum HtmlNodeType
{
	Document,
	Element,
	Comment,
	Text,
}
public class HtmlParseError
{
	private HtmlParseErrorCode _code;
	private int _line;
	private int _linePosition;
	private string _reason;
	private string _sourceText;
	private int _streamPosition;
	internal HtmlParseError(
		HtmlParseErrorCode code,
		int line,
		int linePosition,
		int streamPosition,
		string sourceText,
		string reason)
	{
		_code = code;
		_line = line;
		_linePosition = linePosition;
		_streamPosition = streamPosition;
		_sourceText = sourceText;
		_reason = reason;
	}
	public HtmlParseErrorCode Code
	{
		get { return _code; }
	}
	public int Line
	{
		get { return _line; }
	}
	public int LinePosition
	{
		get { return _linePosition; }
	}
	public string Reason
	{
		get { return _reason; }
	}
	public string SourceText
	{
		get { return _sourceText; }
	}
	public int StreamPosition
	{
		get { return _streamPosition; }
	}
}
public enum HtmlParseErrorCode
{
	TagNotClosed,
	TagNotOpened,
	CharsetMismatch,
	EndTagNotRequired,
	EndTagInvalidHere
}
public class HtmlTextNode : HtmlNode
{
	private string _text;
	internal HtmlTextNode(HtmlDocument ownerdocument, int index)
		:
		base(HtmlNodeType.Text, ownerdocument, index)
	{
	}
	public override string InnerHtml
	{
		get { return OuterHtml; }
		set { _text = value; }
	}
	public override string OuterHtml
	{
		get
		{
			if (_text == null)
			{
				return base.OuterHtml;
			}
			return _text;
		}
	}
	public string Text
	{
		get
		{
			if (_text == null)
			{
				return base.OuterHtml;
			}
			return _text;
		}
		set
		{
			_text = value;
			SetChanged();
		}
	}
}
internal class NameValuePairList
{
	internal readonly string Text;
	private List<KeyValuePair<string, string>> _allPairs;
	private Dictionary<string, List<KeyValuePair<string, string>>> _pairsWithName;
	internal NameValuePairList() :
		this(null)
	{
	}
	internal NameValuePairList(string text)
	{
		Text = text;
		_allPairs = new List<KeyValuePair<string, string>>();
		_pairsWithName = new Dictionary<string, List<KeyValuePair<string, string>>>();
		Parse(text);
	}
	internal static string GetNameValuePairsValue(string text, string name)
	{
		NameValuePairList l = new NameValuePairList(text);
		return l.GetNameValuePairValue(name);
	}
	internal List<KeyValuePair<string, string>> GetNameValuePairs(string name)
	{
		if (name == null)
			return _allPairs;
		return _pairsWithName.ContainsKey(name) ? _pairsWithName[name] : new List<KeyValuePair<string, string>>();
	}
	internal string GetNameValuePairValue(string name)
	{
		if (name == null)
			throw new ArgumentNullException();
		List<KeyValuePair<string, string>> al = GetNameValuePairs(name);
		if (al.Count == 0)
			return string.Empty;
		return al[0].Value.Trim();
	}
	private void Parse(string text)
	{
		_allPairs.Clear();
		_pairsWithName.Clear();
		if (text == null)
			return;
		string[] p = text.Split(';');
		foreach (string pv in p)
		{
			if (pv.Length == 0)
				continue;
			string[] onep = pv.Split(new[] {'='}, 2);
			if (onep.Length == 0)
				continue;
			KeyValuePair<string, string> nvp = new KeyValuePair<string, string>(onep[0].Trim().ToLowerInvariant(),
				onep.Length < 2 ? "" : onep[1]);
			_allPairs.Add(nvp);
			List<KeyValuePair<string, string>> al;
			if (!_pairsWithName.TryGetValue(nvp.Key, out al))
			{
				al = new List<KeyValuePair<string, string>>();
				_pairsWithName.Add(nvp.Key, al);
			}
			al.Add(nvp);
		}
	}
}
internal partial class Trace
{
	internal static Trace _current;
	internal static Trace Current
	{
		get
		{
			if (_current == null)
				_current = new Trace();
			return _current;
		}
	}
	partial void WriteLineIntern(string message, string category);
	public static void WriteLine(string message, string category)
	{
		Current.WriteLineIntern(message, category);
	}
}
