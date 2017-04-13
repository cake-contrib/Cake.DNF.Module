using System;
using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Core.Packaging;
using Cake.DNF.Module;

[assembly: CakeModule(typeof(DNFModule))]

namespace Cake.DNF.Module
{
    public class DNFModule : ICakeModule
    {
        public void Register(ICakeContainerRegistrar registrar)
        {
            if (registrar == null) {
                throw new ArgumentNullException(nameof(registrar));
            }
            registrar.RegisterType<DNFContentResolver>().As<IDNFContentResolver>();
            registrar.RegisterType<DNFPackageInstaller>().As<IPackageInstaller>();
        }
    }
}
