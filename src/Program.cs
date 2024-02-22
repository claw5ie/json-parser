class Program
{
  static string json = "{ \"hello\": [1, false, true, \"hello\"], \"oi\": 2 }\0";

  static void Main(string[] args)
  {
		var node = Parser.parse_json(json);
		node.debug_print();
  }
}
