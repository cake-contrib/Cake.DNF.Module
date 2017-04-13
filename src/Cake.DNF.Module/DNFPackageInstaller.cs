using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cake.Core;
using Cake.Core.Configuration;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Packaging;

namespace Cake.DNF.Module
{
    public class DNFPackageInstaller : IPackageInstaller
    {
        private ICakeEnvironment _environment;
        private IProcessRunner _processRunner;
        private ICakeLog _log;
        private IDNFContentResolver _contentResolver;
        private ICakeConfiguration _config;

        public DNFPackageInstaller(ICakeEnvironment environment, IProcessRunner processRunner, ICakeLog log, IDNFContentResolver contentResolver, ICakeConfiguration config)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }

            if (processRunner == null)
            {
                throw new ArgumentNullException(nameof(processRunner));
            }

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            if (contentResolver == null)
            {
                throw new ArgumentNullException(nameof(contentResolver));
            }

            _environment = environment;
            _processRunner = processRunner;
            _log = log;
            _contentResolver = contentResolver;
            _config = config;
        }
        public bool CanInstall(PackageReference package, PackageType type)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            return package.Scheme.Equals("dnf", StringComparison.OrdinalIgnoreCase);
        }

        public IReadOnlyCollection<IFile> Install(PackageReference package, PackageType type, DirectoryPath path)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (_environment.Platform.Family != PlatformFamily.Linux) {
                _log.Warning("Non-Linux platform detected! Not attempting installation...");
                return _contentResolver.GetFiles(package, type);
            }

            // Install the package.
            _log.Debug("Installing package {0} with DNF...", package.Package);
            var process = _processRunner.Start(
                "dnf",
                new ProcessSettings { Arguments = GetArguments(package, _config), RedirectStandardOutput = true, Silent = _log.Verbosity < Verbosity.Diagnostic });

            process.WaitForExit();

            var exitCode = process.GetExitCode();
            if (exitCode != 0)
            {
                _log.Warning("DNF exited with {0}", exitCode);
                var output = string.Join(Environment.NewLine, process.GetStandardOutput());
                _log.Verbose(Verbosity.Diagnostic, "Output:\r\n{0}", output);
            }

            var result = _contentResolver.GetFiles(package, type);
            if (result.Count != 0)
            {
                return result;
            }

            // TODO: maybe some warnings here
            return result;
        }

        private ProcessArgumentBuilder GetArguments(
            PackageReference definition,
            ICakeConfiguration config)
        {
            var arguments = new ProcessArgumentBuilder();

            arguments.Append("install");
            arguments.Append("-y");
            var packageString = new StringBuilder(definition.Package);
            // if an absolute uri is specified for source, use this
            // otherwise check config for customise package source/s
            if (definition.Address != null)
            {
                arguments.Append($"--repofrompath=\"{definition.Package},{definition.Address.AbsoluteUri}\"");
                arguments.Append($"--repo={definition.Package}");
            } else {
                var dnfSource = config.GetValue("DNF_Source");
                if (!string.IsNullOrWhiteSpace(dnfSource))
                {
                    arguments.Append($"--repofrompath=\"{definition.Package},{dnfSource}\"");
                    arguments.Append($"--repo={definition.Package}");
                }
            }

            if (_log.Verbosity == Verbosity.Verbose || _log.Verbosity == Verbosity.Diagnostic) {
                arguments.Append("--verbose");
            }

            if (definition.GetSwitch("best")) {
                arguments.Append("--best");
            }

            // Version
            if (definition.Parameters.ContainsKey("version"))
            {
                packageString.Append($"-{definition.Parameters["version"].First()}");
            }

            if (definition.Parameters.ContainsKey("arch")) {
                packageString.Append($".{definition.Parameters["arch"].First()}");
            }
            arguments.Append(packageString.ToString());
            return arguments;
        }
    }
}