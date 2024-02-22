class Program
{
  static void Main(string[] args)
  {
		var node = Parser.parse_json("examples/example0");
		node.debug_print();
  }
}
