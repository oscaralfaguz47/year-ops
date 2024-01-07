using Microsoft.Extensions.DependencyInjection;

namespace OceansApp.Utility.LazyLoading
{
    public class LazyServiceProvider<T> : Lazy<T> where T : class
    {
        public LazyServiceProvider(IServiceProvider serviceProvider)
            : base(() => serviceProvider.GetRequiredService<T>())
        {
        }
    }
}
