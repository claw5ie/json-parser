public class ParserTest
{
	[Fact]
	public void test_lexer()
	{
		Token.Tag[] expected_tokens = [
			Token.Tag.Open_Curly,
			Token.Tag.Close_Curly,
			Token.Tag.Open_Bracket,
			Token.Tag.Close_Bracket,
			Token.Tag.Colon,
			Token.Tag.Comma,
			Token.Tag.False,
			Token.Tag.True,
			Token.Tag.Null,
			Token.Tag.String,
			Token.Tag.Number,
			Token.Tag.End_Of_File,
			];

		var lexer = new Lexer(str: "{ } [ ] : , false true null \"hello\" 42", is_filepath: false);

		foreach (var expected_token in expected_tokens)
		{
			var token = lexer.take();
			lexer.consume();

			Assert.Equal(expected_token, token.tag);
		}
	}

  [Fact]
	public void test_parser_base_cases()
	{
		var false_node = Parser.parse_json_from_string("false");
		var true_node = Parser.parse_json_from_string("true");
		var null_node = Parser.parse_json_from_string("null");
		var string_node = Parser.parse_json_from_string("\"foo\"");
		var number_node = Parser.parse_json_from_string("42");

		Assert.IsType<Json.Boolean>(false_node);
		Assert.IsType<Json.Boolean>(true_node);
		Assert.IsType<Json.Null>(null_node);
		Assert.IsType<Json.String>(string_node);
		Assert.IsType<Json.Number>(number_node);

		Assert.False(((Json.Boolean)false_node).value);
		Assert.True(((Json.Boolean)true_node).value);
		Assert.Equal("foo", ((Json.String)string_node).value);
		Assert.Equal(42, ((Json.Number)number_node).value);
	}

  [Fact]
	public void test_parser_compound_cases()
	{
		var empty_object_node = Parser.parse_json_from_string("{ }");
		var object_node = Parser.parse_json_from_string("{ \"foo\": null, \"bar\": 69 }");
		var empty_array_node = Parser.parse_json_from_string("[ ]");
		var array_node = Parser.parse_json_from_string("[ 42, null ]");

		Assert.IsType<Json.Object>(empty_object_node);
		Assert.IsType<Json.Object>(object_node);
		Assert.IsType<Json.Array>(empty_array_node);
		Assert.IsType<Json.Array>(array_node);

		Assert.Empty(((Json.Object)empty_object_node).fields);

		{
			var node = (Json.Object)object_node;
			Json? value = null;

			Assert.Equal(2, node.fields.Count);
			Assert.True(node.fields.TryGetValue("foo", out value));
			Assert.NotNull(value);
			Assert.IsType<Json.Null>(value);
			Assert.True(node.fields.TryGetValue("bar", out value));
			Assert.NotNull(value);
			Assert.IsType<Json.Number>(value);
			Assert.Equal(69, ((Json.Number)value).value);
		}

		Assert.Empty(((Json.Array)empty_array_node).values);

		{
			var node = (Json.Array)array_node;

			Assert.Equal(2, node.values.Count);

			Json first = node.values[0];
			Assert.IsType<Json.Number>(first);
			Assert.Equal(42, ((Json.Number)first).value);

			Json second = node.values[1];
			Assert.IsType<Json.Null>(second);
		}
	}
}
