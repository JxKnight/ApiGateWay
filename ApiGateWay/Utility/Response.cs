using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using System.Collections;
using ApiGateWay.Model;

namespace ApiGateWay.Utility
{
    public class Response
	{
        private Logger logger = LogManager.GetCurrentClassLogger();
        public IActionResult ReturnMsg(bool Result, string ResultMsg,dynamic obj,ProjectModel logs)
        {
            dynamic Resultobj = new ExpandoObject();
            if (Result) Resultobj.Status = 1;
            else Resultobj.Status = 0;
            Resultobj.StatusMsg = ResultMsg;
            if (obj != null)
            {
                if (obj is JObject jObject)
                {
                    var dictionary = jObject.ToObject<Dictionary<string, object>>();
                    Resultobj.Data = FilterEmptyCollections(dictionary);
                }
                else if (obj is IDictionary<string, object> dictionary)
                {
                    Resultobj.Data = FilterEmptyCollections(dictionary);
                }
                else
                {
                    Resultobj.Data = obj;
                }
            }
            return new OkObjectResult(Resultobj);
        }
        private dynamic FilterEmptyCollections(dynamic obj)
        {
            if (obj is JObject jObject)
            {
                return FilterEmptyCollections(jObject.ToObject<Dictionary<string, object>>());
            }
            if (obj is IDictionary<string, object> dictionary)
            {
                var filteredObj = new ExpandoObject() as IDictionary<string, object>;
                foreach (var property in dictionary)
                {
                    if (property.Value is ICollection collection && collection.Count == 0)
                    {
                        continue; // Skip empty collections
                    }
                    filteredObj.Add(property.Key, FilterEmptyCollections(property.Value));
                }
                return filteredObj;
            }
            if (obj is IEnumerable<object> list)
            {
                var filteredList = new List<object>();
                foreach (var item in list)
                {
                    filteredList.Add(FilterEmptyCollections(item));
                }
                return filteredList;
            }
            return obj; // Return the object as is if it's not a collection or dictionary
        }
    }
}

