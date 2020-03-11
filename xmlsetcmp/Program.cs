// Copied as starting point from https://docs.microsoft.com/en-us/dotnet/standard/serialization/examples-of-xml-serialization

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

// The XmlRoot attribute allows you to set an alternate name
// (PurchaseOrder) for the XML element and its namespace. By
// default, the XmlSerializer uses the class name. The attribute
// also allows you to set the XML namespace for the element. Lastly,
// the attribute sets the IsNullable property, which specifies whether
// the xsi:null attribute appears if the class instance is set to
// a null reference.
[XmlRoot("PurchaseOrder", Namespace=null, IsNullable = false)]
public class PurchaseOrder
{
    public Address ShipTo;
    public string OrderDate;
    // The XmlArray attribute changes the XML element name
    // from the default of "OrderedItems" to "Items".
    [XmlArray("Items")]
    public OrderedItem[] OrderedItems;
    public decimal SubTotal;
    public decimal ShipCost;
    public decimal TotalCost;
    public int ItemCount;

    public void Calculate(decimal shipCost)
    {
        ItemCount = OrderedItems.Length;
        // Calculate the total cost.
        decimal subTotal = new decimal();
        foreach(OrderedItem oi in OrderedItems)
        {
            subTotal += oi.LineTotal;
        }
        SubTotal = subTotal;
        ShipCost = shipCost;
        TotalCost = SubTotal + ShipCost;
    }

    public Dictionary<string, OrderedItem> GetItems()
    {
        var result = new Dictionary<string, OrderedItem>();
        foreach (var item in OrderedItems)
        {
            result[item.ItemNumber] = item;
        }
        return result;
    }
}

public class Address
{
    // The XmlAttribute attribute instructs the XmlSerializer to serialize the
    // Name field as an XML attribute instead of an XML element (XML element is
    // the default behavior).
    [XmlAttribute]
    public string Name;
    public string Line1;

    // Setting the IsNullable property to false instructs the
    // XmlSerializer that the XML attribute will not appear if
    // the City field is set to a null reference.
    [XmlElement(IsNullable = false)]
    public string City;
    public string State;
    public string Zip;
}

public class OrderedItem
{
    public string ItemNumber;
    public string ItemName;
    public string Description;
    public decimal UnitPrice;
    public int Quantity;
    public decimal LineTotal;

    // Calculate is a custom method that calculates the price per item
    // and stores the value in a field.
    public void Calculate()
    {
        LineTotal = UnitPrice * Quantity;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj.GetType() != typeof(OrderedItem))
        {
            return false;
        }

        var other = (OrderedItem) obj;
        return (ItemNumber == other.ItemNumber)
            && (ItemName == other.ItemName)
            && (Description == other.Description)
            && (UnitPrice == other.UnitPrice)
            && (Quantity == other.Quantity);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    private string SafeString(string s)
    {
        return (s == null) ? "null" : ("'" + s + "'");
    }

    public override string ToString()
    {
        return SafeString(ItemNumber) + ":" + SafeString(ItemName) + " (" + SafeString(Description) + ")";
    }
}

public class Test
{
    public static void Main()
    {
        // Read and write purchase orders.
        Test t = new Test();
        var po = t.ReadPO("po.xml");
        var pp = t.ReadPO("pp.xml");
        List<OrderedItem> onlyInPo = null, common = null, onlyInPp = null;
        List<Tuple<string, OrderedItem, OrderedItem>> diffs = null;
        if (!compareHashes<string, OrderedItem>(po.GetItems(), pp.GetItems(), ref onlyInPo, ref common, ref onlyInPp, ref diffs))
        {
            Console.WriteLine("Differences found:");
            if (onlyInPo != null && onlyInPo.Count > 0)
            {
                Console.WriteLine("  Only in left:");
                foreach (var item in onlyInPo)
                {
                    Console.WriteLine("    " + item);
                }
            }

            if (common != null)
            {
                Console.WriteLine(
                    "  {0} common item{1} out of {2}/{3}.",
                    common.Count,
                    (common.Count > 1) ? "s" : "",
                    po.OrderedItems.Length,
                    pp.OrderedItems.Length);
            }

            if (diffs != null && diffs.Count > 0)
            {
                Console.WriteLine(
                    "  {0} different item values in left/right out of {1}/{2}:",
                    diffs.Count,
                    po.OrderedItems.Length,
                    pp.OrderedItems.Length);
                foreach (var tuple in diffs)
                {
                    Console.WriteLine("    - {0}:\n      {1}!={2}", tuple.Item1, tuple.Item2, tuple.Item3);
                }
            }

            if (onlyInPp != null && onlyInPp.Count > 0)
            {
                Console.WriteLine("  Only in right:");
                foreach (var item in onlyInPp)
                {
                    Console.WriteLine("    " + item);
                }
            }
        }
    }

    private static bool compareHashes<TKey, TValue>(
        IDictionary<TKey, TValue> left,
        IDictionary<TKey, TValue> right,
        ref List<TValue> onlyInLeft,
        ref List<TValue> common,
        ref List<TValue> onlyInRight,
        ref List<Tuple<TKey, TValue, TValue>> valueDifferences)
    {
        onlyInLeft = new List<TValue>();
        common = new List<TValue>();
        onlyInRight = new List<TValue>();
        valueDifferences = new List<Tuple<TKey, TValue, TValue>>();

        // Go through left set, looking for its elements in right set
        foreach (KeyValuePair<TKey, TValue> leftItem in left)
        {
            if (right.ContainsKey(leftItem.Key))
            {
                var rightItemValue = right[leftItem.Key];
                if (leftItem.Value.Equals(rightItemValue))
                {
                    common.Add(rightItemValue);
                }
                else
                {
                    valueDifferences.Add(
                        new Tuple<TKey, TValue, TValue>(leftItem.Key, leftItem.Value, rightItemValue));
                }
            }
            else
            {
                onlyInLeft.Add(leftItem.Value);
            }
        }

        // Go through right set, looking for its elements in left set.
        foreach (KeyValuePair<TKey, TValue> rightItem in right)
        {
            // No need to maintain the common or valueDifferences lists, they
            // were already filled in the previous pass.
            if (!left.ContainsKey(rightItem.Key))
            {
                onlyInRight.Add(rightItem.Value);
            }
        }
        return valueDifferences.Count == 0 && onlyInLeft.Count == 0 && onlyInRight.Count == 0;
    }

    private void CreatePO(string filename)
    {
        // Creates an instance of the XmlSerializer class;
        // specifies the type of object to serialize.
        XmlSerializer serializer = new XmlSerializer(typeof(PurchaseOrder));
        TextWriter writer = new StreamWriter(filename);
        PurchaseOrder po = new PurchaseOrder();

        // Creates an address to ship and bill to.
        Address billAddress = new Address();
        billAddress.Name = "Teresa Atkinson";
        billAddress.Line1 = "1 Main St.";
        billAddress.City = "AnyTown";
        billAddress.State = "WA";
        billAddress.Zip = "00000";
        // Sets ShipTo and BillTo to the same addressee.
        po.ShipTo = billAddress;
        po.OrderDate = System.DateTime.Now.ToLongDateString();

        // Creates an OrderedItem.
        OrderedItem i1 = new OrderedItem();
        i1.ItemNumber = "54100376";
        i1.ItemName = "Widget S";
        i1.Description = "Small widget";
        i1.UnitPrice = (decimal) 5.23;
        i1.Quantity = 3;
        i1.Calculate();

        // Creates an OrderedItem.
        OrderedItem i2 = new OrderedItem();
        i2.ItemNumber = "54107088";
        i2.ItemName = "Widget T";
        i2.Description = "Tall widget";
        i2.UnitPrice = (decimal) 15.32;
        i2.Quantity = 2;
        i2.Calculate();

        // Inserts the item into the array.
        OrderedItem [] items = {i1, i2};
        po.OrderedItems = items;
        po.Calculate((decimal) 15.21);
        // Serializes the purchase order, and closes the TextWriter.
        serializer.Serialize(writer, po);
        writer.Close();
    }

    protected PurchaseOrder ReadPO(string filename)
    {
        // Creates an instance of the XmlSerializer class;
        // specifies the type of object to be deserialized.
        XmlSerializer serializer = new XmlSerializer(typeof(PurchaseOrder));
        // If the XML document has been altered with unknown
        // nodes or attributes, handles them with the
        // UnknownNode and UnknownAttribute events.
        serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
        serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);

        // A FileStream is needed to read the XML document.
        FileStream fs = new FileStream(filename, FileMode.Open);
        // Declares an object variable of the type to be deserialized.
        PurchaseOrder po;
        // Uses the Deserialize method to restore the object's state
        // with data from the XML document. */
        po = (PurchaseOrder)serializer.Deserialize(fs);
        DisplayPurchaseOrder(po);
        return po;
    }

    private void DisplayPurchaseOrder(PurchaseOrder po)
    {
        // Reads the order date.
        Console.WriteLine("OrderDate: " + po.OrderDate);

        // Reads the shipping address.
        Address shipTo = po.ShipTo;
        ReadAddress(shipTo, "Ship To:");
        // Reads the list of ordered items.
        OrderedItem[] items = po.OrderedItems;
        Console.WriteLine(po.ItemCount + " items to be shipped:");
        foreach (OrderedItem oi in items)
        {
            Console.WriteLine("\t" +
                              oi.ItemNumber + ": " + oi.ItemName + "\t" +
                              oi.Description + "\t" +
                              oi.UnitPrice + "\t" +
                              oi.Quantity + "\t" +
                              oi.LineTotal);
        }
        // Reads the subtotal, shipping cost, and total cost.
        Console.WriteLine(
            "\n\t\t\t\t\t\t Subtotal\t" + po.SubTotal +
            "\n\t\t\t\t\t\t Shipping\t" + po.ShipCost +
            "\n\t\t\t\t\t\t Total\t\t" + po.TotalCost
            );
    }

    protected void ReadAddress(Address a, string label)
    {
        // Reads the fields of the Address.
        Console.WriteLine(label);
        Console.Write("\t"+
                      a.Name +"\n\t" +
                      a.Line1 +"\n\t" +
                      a.City +"\t" +
                      a.State +"\n\t" +
                      a.Zip +"\n");
    }

    protected void serializer_UnknownNode
    (object sender, XmlNodeEventArgs e)
    {
        Console.WriteLine("Unknown Node:" +   e.Name + "\t" + e.Text);
    }

    protected void serializer_UnknownAttribute
    (object sender, XmlAttributeEventArgs e)
    {
        System.Xml.XmlAttribute attr = e.Attr;
        Console.WriteLine("Unknown attribute " + attr.Name + "='" + attr.Value + "'");
    }
}
