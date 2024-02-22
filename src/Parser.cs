using System.Diagnostics;

class Parser
{
  Lexer lexer;

  public static Json parse_json(string filepath)
	{
		return new Parser(filepath).parse_json_value();
	}

  Parser(string filepath)
	{
		this.lexer = new Lexer(filepath);
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
				fields.Add(token.text.ToArray(), value);

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
			var text = take().text;
			consume();

			return new Json.String{ value = text.ToArray() };
		}
		case Token.Tag.Number:
		{
			long value = 0;

			var text = take().text;
			foreach (char ch in text)
				value = 10 * value + (ch - '0');

			consume();

			return new Json.Number{ value = value };
		}
		default:
			// error.
			Debug.Assert(false);
			return new Json.Null{ };
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
