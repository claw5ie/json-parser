global using JsonObjectFields = System.Collections.Generic.Dictionary<string, Json>;
global using JsonArrayValues = System.Collections.Generic.List<Json>;

public abstract class Json
{
  public class Object: Json
  {
    public JsonObjectFields fields = new JsonObjectFields();
  };

  public class Array: Json
  {
    public JsonArrayValues values = new JsonArrayValues();
  };

  public class String: Json
  {
    public string value = "";
  };

  public class Number: Json
  {
		public double value = 0;
  };

  public class Boolean: Json
  {
    public bool value = false;
  };

  public class Null: Json
  {

  };
};
