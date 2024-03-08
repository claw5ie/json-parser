using System.Diagnostics;

public class ParsingException: Exception
{
	public ParsingException(string text)
		: base(text)
	{

	}
};

public class Parser
{
  Lexer lexer;

  public static Json parse_json_from_file(string filepath)
	{
		return new Parser(str: filepath, is_filepath: true).parse_json_value();
	}

	public static Json parse_json_from_string(string json)
	{
		return new Parser(str: json, is_filepath: false).parse_json_value();
	}

  Parser(string str, bool is_filepath)
	{
		this.lexer = new Lexer(str, is_filepath);
	}

  Json parse_json_value()
	{
		switch (peek())
		{
		case Token.Tag.Open_Curly:
		{
			consume();

			var fields = new JsonObjectFields();
			var is_first_iteration = true;

			var tt = peek();
			while (tt != Token.Tag.End_Of_File && tt != Token.Tag.Close_Curly)
			{
				if (!is_first_iteration)
					expect(Token.Tag.Comma);

				var token = take();
				expect(Token.Tag.String);
				expect(Token.Tag.Colon);
				var value = parse_json_value();
				// Doesn't take into account repeated fields.
				fields.Add(new string(token.text), value);

				is_first_iteration = false;
				tt = peek();
			}

			expect(Token.Tag.Close_Curly);

			return new Json.Object{ fields = fields };
		}
		case Token.Tag.Open_Bracket:
		{
			consume();

			var values = new JsonArrayValues();
			var is_first_iteration = true;

			var tt = peek();
			while (tt != Token.Tag.End_Of_File && tt != Token.Tag.Close_Bracket)
			{
				if (!is_first_iteration)
					expect(Token.Tag.Comma);

				var value = parse_json_value();
				values.Add(value);

				is_first_iteration = false;
				tt = peek();
			}

			expect(Token.Tag.Close_Bracket);

			return new Json.Array{ values = values };
		}
		case Token.Tag.False:
			consume();
			return new Json.Boolean{ value = false };
		case Token.Tag.True:
			consume();
			return new Json.Boolean{ value = true };
		case Token.Tag.Null:
			consume();
			return new Json.Null{ };
		case Token.Tag.String:
		{
			var data = take().data;
			consume();

			return new Json.String{ value = ((Token.Data.String)data).value };
		}
		case Token.Tag.Number:
		{
			var data = (Token.Data.Number)take().data;
			consume();

			return new Json.Number{ value = data.value };
		}
		default:
		{
			var token = take();
			throw new ParsingException("unexpected start of expression '" + token.text.ToString() + "'");
		}
		}
	}

  Token.Tag peek()
	{
		return lexer.peek();
	}

  Token take()
	{
		return lexer.take();
	}

  void consume()
	{
		lexer.consume();
	}

  void expect(Token.Tag expected)
	{
		lexer.expect(expected);
	}
};
