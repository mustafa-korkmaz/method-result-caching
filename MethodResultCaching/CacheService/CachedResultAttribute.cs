using System.Reflection;
using PostSharp.Aspects;
using PostSharp.Serialization;

namespace MethodResultCaching.CacheService
{
    [PSerializable]
    public class CachedResultAttribute : MethodInterceptionAspect
    {
        public override async Task OnInvokeAsync(MethodInterceptionArgs args)
        {
            ICacheService cacheService = CacheService.Instance;

            // get method args to have method result specific value
            var key = GetCacheKey(args.Method, args.Arguments);

            var mi = args.Method as MethodInfo;

            var type = mi.ReturnType.GetGenericArguments()[0];

            MethodInfo method = cacheService.GetType().GetMethod(nameof(cacheService.GetAsync))!
                .MakeGenericMethod(type);

            dynamic awaitable = method.Invoke(cacheService, new object[] { key });

            await awaitable;

            var result = awaitable.GetAwaiter().GetResult();

            if (result != null)
            {
                Console.WriteLine($"Data is retrieved from cache: {result.ToString()}");

                args.ReturnValue = result;
                return;
            }

            await args.ProceedAsync();

            await cacheService.SetAsync(key, args.ReturnValue);
        }

        private string GetCacheKey(MemberInfo methodInfo, IEnumerable<object> arguments)
        {
            var methodName = string.Format("{0}.{1}.{2}",
                                       methodInfo.ReflectedType.Namespace,
                                       methodInfo.ReflectedType.Name,
                                       methodInfo.Name);

            var key = string.Format(
              "{0}({1})",
              methodName,
              string.Join(", ", arguments.Select(x => x.ToString())));

            return key;
        }

    }
}
