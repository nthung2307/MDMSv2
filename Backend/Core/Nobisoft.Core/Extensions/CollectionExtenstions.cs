using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nobisoft.Core.Extensions
{
    public static class CollectionExtensions
    {
        public static bool Equals<TEntity>(this ICollection<TEntity> requestExtraProperties,
            ICollection<TEntity> aggregateStreamExtraProperties)
        {
            if (EqualityComparer<ICollection<TEntity>>.Default.Equals(requestExtraProperties, null))
                requestExtraProperties = new Collection<TEntity>();
            if (EqualityComparer<ICollection<TEntity>>.Default.Equals(aggregateStreamExtraProperties, null))
                aggregateStreamExtraProperties = new Collection<TEntity>();
            if (!requestExtraProperties.Any() && !aggregateStreamExtraProperties.Any())
                return true;
            return (!requestExtraProperties.Any() || aggregateStreamExtraProperties.Any()) &&
                   (requestExtraProperties.Any()
                    || !aggregateStreamExtraProperties.Any()) && JToken.DeepEquals(
                       JArray.Parse(JsonConvert.SerializeObject(requestExtraProperties)),
                       JArray.Parse(JsonConvert.SerializeObject(aggregateStreamExtraProperties)));
        }

        public static NameValueCollection AsNameValueCollection(
            this IEnumerable<KeyValuePair<string, StringValues>> collection)
        {
            var nv = new NameValueCollection();

            foreach (var field in collection)
            {
                nv.Add(field.Key, field.Value.First());
            }

            return nv;
        }


        public static NameValueCollection AsNameValueCollection(this IDictionary<string, StringValues> collection)
        {
            var nv = new NameValueCollection();

            foreach (var field in collection)
            {
                nv.Add(field.Key, field.Value.First());
            }

            return nv;
        }
    }
}