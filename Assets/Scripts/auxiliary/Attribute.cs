using System;

public class Attribute<T>
{
  private T currentValue;
  private T previousValue;

  public Attribute()
  {
    currentValue = default(T);
    previousValue = currentValue;
  }

  public Attribute(T initialValue)
  {
    currentValue = initialValue;
    previousValue = currentValue;
  }

  public T Value
  {
    get { return currentValue; }
    set
    {
      previousValue = currentValue;
      currentValue = value;
    }
  }

  public void ResetPreviousValue()
  {
    previousValue = currentValue;
  }

  public override string ToString()
  {
    return currentValue.ToString();
  }

  public static int Delta(Attribute<int> attr)
  {
    return attr.currentValue - attr.previousValue;
  }
}

