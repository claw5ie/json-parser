using System;
using System.Diagnostics;

public class LexingException: Exception
{
	public LexingException(string text)
		: base(text)
	{

	}
};

public class Lexer
{
  public const byte LOOKAHEAD = 1;

  Token[] tokens = new Token[LOOKAHEAD];
  byte token_start = 0;
  byte token_count = 0;
  char[] source;
  int offset = 0;
	string filepath;

  public Lexer(string str, bool is_filepath)
  {
		if (is_filepath)
		{
			this.source = (File.ReadAllText(str) + '\0').ToArray();
			this.filepath = str;
		}
		else
		{
			this.source = (str + '\0').ToArray();
			this.filepath = "<no file>";
		}
  }

  public Token.Tag peek()
  {
    if (token_count == 0)
      buffer_token();

    return tokens[token_start].tag;
  }

  public Token take()
  {
    if (token_count == 0)
      buffer_token();

    return tokens[token_start];
  }

  public void consume()
  {
    Debug.Assert(token_count > 0);
    ++token_start;
    token_start %= LOOKAHEAD;
    --token_count;
  }

  public void expect(Token.Tag expected)
  {
		var actual = peek();
    if (actual != expected)
			throw new LexingException("expected '" + expected + "' but got '" + actual + "'");
    consume();
  }

  void buffer_token()
  {
    var text = source;
    var i = offset;

    for (; Char.IsWhiteSpace(text[i]); i++)
      ;

		var old_i = i;
    var token = new Token{
			tag = Token.Tag.End_Of_File,
			text = slice(source, old_i, old_i),
		};

    if (text[i] == '\0' || i >= source.Length)
		{

		}
    else if (text[i] == '"')
		{
			var data = new Token.Data.String{ };
			var unescaped_string = new System.Text.StringBuilder();

			++i;
			while (i + 1 < source.Length && text[i] != '"')
			{
				if (text[i] == '\\')
				{
					++i;
					switch (text[i++])
					{
					case '"':
						unescaped_string.Append('"');
						break;
					case '\\':
						unescaped_string.Append('\\');
						break;
					case '/':
						unescaped_string.Append('/');
						break;
					case 'b':
						unescaped_string.Append('\b');
						break;
					case 'f':
						unescaped_string.Append('\f');
						break;
					case 'n':
						unescaped_string.Append('\n');
						break;
					case 'r':
						unescaped_string.Append('\r');
						break;
					case 't':
						unescaped_string.Append('\t');
						break;
					case 'u':
					{
						var ch = from_four_digit_hex(make_span(text, i));
						unescaped_string.Append(ch);
						i += 4;
					} break;
					default:
						throw new LexingException("invalid escape sequence '" + text[i - 1] + "'");
					}
				}
				else if (Char.IsControl(text[i]))
					throw new LexingException("unexpected control sequence '" + (int)text[i] + "'");
				else
					unescaped_string.Append(text[i++]);
			}

			if (text[i] != '"')
				throw new LexingException("unterminated string");

			data.value = unescaped_string.ToString();

			++i;
			token.tag = Token.Tag.String;
			token.text = slice(source, old_i + 1, i - 1);
			token.data = data;
		}
    else if (text[i] == '-' || Char.IsDigit(text[i]))
		{
			var data = new Token.Data.Number{ };
			var start = i;

			{
				var is_negative = (text[i] == '-');

				if (is_negative)
					++i;

				i += take_digits(make_span(text, i));
			}

			if (text[i] == '.')
			{
				++i;
				i += take_digits(make_span(text, i));
			}

			if (text[i] == 'e' || text[i] == 'E')
			{
				++i;
				var is_negative = (text[i] == '-');
				if (is_negative || (text[i] == '+'))
					++i;
				i += take_digits(make_span(text, i));
			}

			data.value = Convert.ToDouble(make_span(text, start, i).ToString());

			token.tag = Token.Tag.Number;
			token.text = slice(source, old_i, i);
			token.data = data;
		}
    else if (Char.IsLetter(text[i]))
		{
			do
				++i;
			while (Char.IsLetter(text[i]));

			var keyword = slice(source, old_i, i);

			if (keyword.SequenceEqual("false"))
				token.tag = Token.Tag.False;
			else if (keyword.SequenceEqual("true"))
				token.tag = Token.Tag.True;
			else if (keyword.SequenceEqual("null"))
				token.tag = Token.Tag.Null;
			else
				throw new LexingException("invalid keyword '" + make_span(source, old_i, i).ToString() + "'");

			token.text = keyword;
		}
    else
		{
			switch (text[i])
			{
			case '{':
				++i;
				token.tag = Token.Tag.Open_Curly;
				token.text = slice(source, old_i, i);
				break;
			case '}':
				++i;
				token.tag = Token.Tag.Close_Curly;
				token.text = slice(source, old_i, i);
				break;
			case '[':
				++i;
				token.tag = Token.Tag.Open_Bracket;
				token.text = slice(source, old_i, i);
				break;
			case ']':
				++i;
				token.tag = Token.Tag.Close_Bracket;
				token.text = slice(source, old_i, i);
				break;
			case ':':
				++i;
				token.tag = Token.Tag.Colon;
				token.text = slice(source, old_i, i);
				break;
			case ',':
				++i;
				token.tag = Token.Tag.Comma;
				token.text = slice(source, old_i, i);
				break;
			default:
				throw new LexingException("invalid character '" + text[i] + "'");
			}
		}

		offset = i;

    Debug.Assert(token_count < LOOKAHEAD);
    int index = (token_start + token_count) % LOOKAHEAD;
    tokens[index] = token;
    ++token_count;
  }

	static ArraySegment<char> slice(char[] source, int start, int past_end)
	{
		return new ArraySegment<char>(source, start, past_end - start);
	}

	static Span<char> make_span(char[] text, int start)
	{
		return new Span<char>(text, start, text.Length - start);
	}

	static Span<char> make_span(char[] text, int start, int past_end)
	{
		return new Span<char>(text, start, past_end - start);
	}

	static bool from_hex_digit(char ch, out char dst)
	{
		var ok = true;

		dst = (char)0;

		if ('0' <= ch && ch <= '9')
			dst = (char)(ch - '0');
		else if ('a' <= ch && ch <= 'f')
			dst = (char)(ch - 'a' + 10);
		else if ('A' <= ch && ch <= 'F')
			dst = (char)(ch - 'A' + 10);
		else
			ok = false;

		return ok;
	}

	static char from_four_digit_hex(Span<char> text)
	{
		if (text.Length < 4)
			throw new LexingException("expected at least 4 characters, but got " + (text.Length - 1)); // Account for null terminator.

		char dst = (char)0;

		foreach (var ch in text.Slice(0, 4))
		{
			char digit = (char)0;

			if (!from_hex_digit(ch, out digit))
				throw new LexingException("expected hexadecimal digit, but got '" + ch + "'");

			dst = (char)(16 * dst + digit);
		}

		return dst;
	}

	static int take_digits(Span<char> text)
	{
		int count = 0;

		if (!Char.IsDigit(text[count]))
			throw new LexingException("expected digit, but got '" + text[count] + "'");

		do
			++count;
		while (Char.IsDigit(text[count]));

		return count;
	}
};
