using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Packaging;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

namespace Cake.DNF.Module.Tests
{
    /// <summary>
    /// ChocolateyPackageInstaller unit tests
    /// </summary>
    public sealed class DNFPackageInstallerTests
    {
        public sealed class TheConstructor
        {
            [Fact]
            public void Should_Throw_If_Environment_Is_Null()
            {
                // Given
                var fixture = new DNFPackageInstallerFixture();
                fixture.Environment = null;

                // When
                var result = Record.Exception(() => fixture.CreateInstaller());

                // Then
                Assert.IsType<ArgumentNullException>(result);
                Assert.Equal("environment", ((ArgumentNullException)result).ParamName);
            }

            [Fact]
            public void Should_Throw_If_Process_Runner_Is_Null()
            {
                // Given
                var fixture = new DNFPackageInstallerFixture();
                fixture.ProcessRunner = null;

                // When
                var result = Record.Exception(() => fixture.CreateInstaller());

                // Then
                Assert.IsType<ArgumentNullException>(result);
                Assert.Equal("processRunner", ((ArgumentNullException)result).ParamName);
            }

            [Fact]
            public void Should_Throw_If_Content_Resolver_Is_Null()
            {
                // Given
                var fixture = new DNFPackageInstallerFixture();
                fixture.ContentResolver = null;

                // When
                var result = Record.Exception(() => fixture.CreateInstaller());

                // Then
                Assert.IsType<ArgumentNullException>(result);
                Assert.Equal("contentResolver", ((ArgumentNullException)result).ParamName);
            }

            [Fact]
            public void Should_Throw_If_Log_Is_Null()
            {
                // Given
                var fixture = new DNFPackageInstallerFixture();
                fixture.Log = null;

                // When
                var result = Record.Exception(() => fixture.CreateInstaller());

                // Then
                Assert.IsType<ArgumentNullException>(result);
                Assert.Equal("log", ((ArgumentNullException)result).ParamName);
            }
        }

        public sealed class TheCanInstallMethod
        {
            private string DNF_CONFIGKEY = "DNF_Source";

            [Fact]
            public void Should_Throw_If_URI_Is_Null()
            {
                // Given
                var fixture = new DNFPackageInstallerFixture();
                fixture.Package = null;

                // When
                var result = Record.Exception(() => fixture.CanInstall());

                // Then
                Assert.IsType<ArgumentNullException>(result);
                Assert.Equal("package", ((ArgumentNullException)result).ParamName);
            }

            [Fact]
            public void Should_Be_Able_To_Install_If_Scheme_Is_Correct()
            {
                // Given
                var fixture = new DNFPackageInstallerFixture();
                fixture.Package = new PackageReference("dnf:?package=glxgears");

                // When
                var result = fixture.CanInstall();

                // Then
                Assert.True(result);
            }

            [Fact]
            public void Should_Not_Be_Able_To_Install_If_Scheme_Is_Incorrect()
            {
                // Given
                var fixture = new DNFPackageInstallerFixture();
                fixture.Package = new PackageReference("homebrew:?package=windirstat");

                // When
                var result = fixture.CanInstall();

                // Then
                Assert.False(result);
            }

            [Fact]
            public void Should_Ignore_Custom_Source_If_AbsoluteUri_Is_Used()
            {
                var fixture = new DNFPackageInstallerFixture();
                fixture.Package = new PackageReference("choco:http://absolute/?package=windirstat");

                // When
                var result = Record.Exception(() => fixture.Install());

                // Then
                Assert.Null(result);
                fixture.Config.DidNotReceive().GetValue(DNF_CONFIGKEY);
            }

            [Fact]
            public void Should_Use_Custom_Source_If_RelativeUri_Is_Used()
            {
                var fixture = new DNFPackageInstallerFixture();
                fixture.Package = new PackageReference("dnf:?package=glx-utils");

                // When
                var result = Record.Exception(() => fixture.Install());

                // Then
                Assert.Null(result);
                fixture.Config.Received().GetValue(DNF_CONFIGKEY);
            }
        }

        public sealed class TheInstallMethod
        {
            [Fact]
            public void Should_Throw_If_Uri_Is_Null()
            {
                // Given
                var fixture = new DNFPackageInstallerFixture();
                fixture.Package = null;

                // When
                var result = Record.Exception(() => fixture.Install());

                // Then
                Assert.IsType<ArgumentNullException>(result);
                Assert.Equal("package", ((ArgumentNullException)result).ParamName);
            }

            
            ///<summary>
            /// This test is the inverse of the normal one since the install path is ignored.
            ///</summary>
            ///<remarks>
            ///An install path makes no sense in a DNF context
            ///</remarks>
            [Fact]
            public void Should_Not_Throw_If_Install_Path_Is_Null()
            {
                // Given
                var fixture = new DNFPackageInstallerFixture();
                fixture.InstallPath = null;

                // When
                var result = Record.Exception(() => fixture.Install());

                // Then
                Assert.Null(result);
            }
        }
    }
}