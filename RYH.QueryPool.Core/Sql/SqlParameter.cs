namespace RYH.QueryPool.Core
{
    public class SqlParameter
    {
        public SqlParameter(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; private set; }
        public object Value { get; private set; }
    }
}
