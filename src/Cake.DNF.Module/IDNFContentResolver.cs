using System.Collections.Generic;
using Cake.Core.IO;
using Cake.Core.Packaging;

namespace Cake.DNF.Module
{
    public interface IDNFContentResolver
    {
        IReadOnlyCollection<IFile> GetFiles(PackageReference package, PackageType type);
    }
}