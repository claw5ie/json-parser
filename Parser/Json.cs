global using JsonObjectFields = System.Collections.Generic.Dictionary<char[], Json>;
global using JsonArrayValues = System.Collections.Generic.List<Json>;

public abstract class Json
{
	public abstract void debug_print();

  public class Object: Json
  {
    public JsonObjectFields fields = new JsonObjectFields();

		public override void debug_print()
		{
			Console.Write("Object(");
			foreach (var field in fields)
			{
				Console.Write(field.Key);
				Console.Write(',');
				field.Value.debug_print();
				Console.Write(',');
			}
			Console.Write(')');
		}
  };

  public class Array: Json
  {
    public JsonArrayValues values = new JsonArrayValues();

		public override void debug_print()
		{
			Console.Write("Array(");
			foreach (var value in values)
			{
				value.debug_print();
				Console.Write(',');
			}
			Console.Write(')');
		}
  };

  public class String: Json
  {
    public char[] value = { };

		public override void debug_print()
		{
			Console.Write(value);
		}
  };

  public class Number: Json
  {
    public long value = 0;

		public override void debug_print()
		{
			Console.Write(value);
		}
  };

  public class Boolean: Json
  {
    public bool value = false;

		public override void debug_print()
		{
			Console.Write(value);
		}
  };

  public class Null: Json
  {
		public override void debug_print()
		{
			Console.Write("null");
		}
  };
};
