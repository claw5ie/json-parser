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

  public Tag tag;
  public ArraySegment<char> text;
};
