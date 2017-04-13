using System;
using System.Collections.Generic;
using Cake.Core;
using Cake.Core.IO;
using Cake.Core.Packaging;

namespace Cake.DNF.Module
{
    public class DNFContentResolver : IDNFContentResolver
    {
        private IFileSystem _fileSystem;
        private ICakeEnvironment _environment;
        private IGlobber _globber;

        public DNFContentResolver(IFileSystem fileSystem, ICakeEnvironment environment, IGlobber globber)
        {
            _fileSystem = fileSystem;
            _environment = environment;
            _globber = globber;
        }
        public IReadOnlyCollection<IFile> GetFiles(PackageReference package, PackageType type)
        {
            if (type == PackageType.Addin) {
                throw new InvalidOperationException("DNF Module does not support addins");
            }

            if (type == PackageType.Tool) {
                return GetToolFiles(package);
            }

            throw new InvalidOperationException("Unknown package type.");
        }

        private IReadOnlyCollection<IFile> GetToolFiles(PackageReference package) {
            var results = new List<IFile>();
            results.Add(_fileSystem.GetFile("/usr/bin/env"));
            return results;
        }
    }
}