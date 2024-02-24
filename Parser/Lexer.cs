using System;
using System.Diagnostics;

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
    if (peek() != expected)
		{
			// TODO: error.
			Debug.Assert(false);
		}
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
			for (++i; i < source.Length && text[i] != '"'; i++)
				;

			if (text[i] != '"')
			{
				// Unterminated string literal
				Debug.Assert(false);
			}

			++i;
			token.tag = Token.Tag.String;
			token.text = slice(source, old_i + 1, i - 1);
		}
    else if (Char.IsDigit(text[i]))
		{
			for (; Char.IsDigit(text[i]); i++)
				;

			token.tag = Token.Tag.Number;
			token.text = slice(source, old_i, i);
		}
    else if (Char.IsLetter(text[i]))
		{
			for (; Char.IsLetter(text[i]); i++)
				;

			var keyword = slice(source, old_i, i);

			if (keyword.SequenceEqual("false"))
				token.tag = Token.Tag.False;
			else if (keyword.SequenceEqual("true"))
				token.tag = Token.Tag.True;
			else if (keyword.SequenceEqual("null"))
				token.tag = Token.Tag.Null;
			else
			{
				Debug.Assert(false);
			}

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
				// Unexpected token
				Debug.Assert(false);
				break;
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
};
