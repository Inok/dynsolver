using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Ninject;
using Splat;

namespace DynamicSolver.ViewModel
{
    public class NinjectLocator : IMutableDependencyResolver
    {
        private readonly IKernel _kernel;

        public NinjectLocator([NotNull] IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            _kernel = kernel;
        }

        public object GetService(Type serviceType, string contract = null)
        {
            return contract != null ? _kernel.Get(serviceType, contract) : _kernel.Get(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType, string contract = null)
        {
            return contract != null ? _kernel.GetAll(serviceType, contract) : _kernel.GetAll(serviceType);
        }

        public void Register(Func<object> factory, Type serviceType, string contract = null)
        {
            var binding = _kernel.Bind(serviceType).ToMethod(_ => factory());

            if (contract != null)
            {
                binding.Named(contract);
            }
        }

        public IDisposable ServiceRegistrationCallback(Type serviceType, string contract, Action<IDisposable> callback)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _kernel.Dispose();
        }
    }
}