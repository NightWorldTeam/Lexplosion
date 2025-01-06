using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Lexplosion.Logic.Network
{
    public struct QueryParam 
    {
        public readonly string Name;
        public readonly string Value;

        public QueryParam(string name, string value)
        {
            Name = name;
            Value = value;
        }


        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class QueryParams : IEnumerable<QueryParam>
    {
        private readonly Dictionary<string, QueryParam> _params = [];

        public void Add(string key, string value) 
        {
            _params.Add(key, new QueryParam(key, value));
        }

        public void Add(QueryParam param) 
        {
            _params[param.Name] = param;
        }

        public IEnumerator<QueryParam> GetEnumerator()
        {
            return _params.Values.GetEnumerator() as IEnumerator<QueryParam>;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _params.Values.GetEnumerator();
        }
    }

    public class QueryApiBuilder
    {
        public string BaseUrl { get; }
        public QueryParams Params { get; }


        #region Constructors


        public QueryApiBuilder(string baseUrl)
        {
            BaseUrl = baseUrl;
            Params = new();
        }


        #endregion Constructors


        public void Add(Dictionary<string, string> queryParams)
        {
            foreach (var param in queryParams) 
            {
                Add(param.Key, param.Value);
            }
        }

        public void Add(IEnumerable<QueryParam> queryParams) 
        {
            foreach (var param in queryParams)
            {
                Add(param);
            }
        }

        public void Add(string name, string value) 
        {
            Params.Add(name, value);
        }

        public void Add(string name, int value)
        {
            Params.Add(name, value.ToString());
        }

        public void Add(QueryParam param) 
        {
            Params.Add(param);
        }

        public string Build() 
        {
            var queryParams = string.Join("&", Params.Select(param => $"{param.Name}={param.Value}"));
            return $"{BaseUrl}?{queryParams}";
        }
    }
}
