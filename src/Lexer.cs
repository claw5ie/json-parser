using System;
using System.Diagnostics;

class Lexer
{
  public const byte LOOKAHEAD = 1;

  Token[] tokens = new Token[LOOKAHEAD];
  byte token_start = 0;
  byte token_count = 0;
  char[] source = { };
  int offset = 0;

  public static Lexer init(string json)
  {
    return new Lexer{ source = json.ToArray() };
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
    var token = new Token(tag: Token.Tag.End_Of_File,
													text: new ArraySegment<char>(source, 0, 0));

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
		}
    else if (Char.IsDigit(text[i]))
		{
			for (; Char.IsDigit(text[i]); i++)
				;

			token.tag = Token.Tag.Number;
		}
    else if (Char.IsLetter(text[i]))
		{
			for (; Char.IsLetter(text[i]); i++)
				;

			var keyword = new ArraySegment<char>(source, old_i, i - old_i);

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
		}
    else
		{
			switch (text[i])
			{
			case '{':
				++i;
				token.tag = Token.Tag.Open_Curly;
				break;
			case '}':
				++i;
				token.tag = Token.Tag.Close_Curly;
				break;
			case '[':
				++i;
				token.tag = Token.Tag.Open_Bracket;
				break;
			case ']':
				++i;
				token.tag = Token.Tag.Close_Bracket;
				break;
			case ':':
				++i;
				token.tag = Token.Tag.Colon;
				break;
			case ',':
				++i;
				token.tag = Token.Tag.Comma;
				break;
			default:
				// Unexpected token
				Debug.Assert(false);
				break;
			}
		}

		offset = i;
		token.text = new ArraySegment<char>(source, old_i, i - old_i);

    Debug.Assert(token_count < LOOKAHEAD);
    int index = (token_start + token_count) % LOOKAHEAD;
    tokens[index] = token;
    ++token_count;
  }
};
