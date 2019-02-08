using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyl18.QQ.CloudPlayerHelper
{
    public class GroupListDictionary<T> : Dictionary<string, HashSet<T>>
    {
        public HashSet<T> GetGroup(string key)
        {
            if (!ContainsKey(key)) this[key] = new HashSet<T>();
            return this[key];
        }

        public HashSet<T> GetAllGroup() => GetGroup("AllGroup");

        public IEnumerable<T> GetMixed(string key)
        {
            return GetGroup(key).Concat(GetAllGroup());
        }
    }
}
