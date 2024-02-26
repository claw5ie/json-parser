public struct Token
{
  public enum Tag
  {
    Open_Curly,
    Close_Curly,
    Open_Bracket,
    Close_Bracket,
    Colon,
    Comma,

    False,
    True,
    Null,

    String,
    Number,

    End_Of_File,
  };

	public abstract class Data
	{
		public class String: Data
		{
			public string value = "";
		};

		public class Number: Data
		{
			public double value;
		};
	};

	public Tag tag;
	public ArraySegment<char> text;
	public Data data;
};
