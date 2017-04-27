The folllowing URI parameters are supported by the Cake.DNF.Module.

# Source

By default, DNF will attempt to install packages using the system default repositories (i.e. the same as invoked using `dnf` with no extra arguments). To use a specific repository, provide a source in the URI.

> [!WARNING]
> Using this parameter invokes the underlying `--repofrompath` **and** `--repo` options, meaning that *only* this repository will be used for the installation.

### Example

```
#tool dnf:http://fedora.uberglobalmirror.com/fedora/linux/updates/25/x86_64/?package=glx-utils
```

# Package

This is the name of the DNF package that you would like to install.  This should match the package name exactly (no architecture).

### Example

```
#tool dnf:?package=glx-utils
```

# Version

The specific version of the application that is being installed.  If not provided, DNF will install the latest package version that is available.

### Example

```
#tool dnf:?package=glx-utils&version=8.3.0
```

# Architecture

This allows specifying the architecture of the package to install. Generally, this should not be required, and should be used with caution.

### Example

```
#tool dnf:?package=glx-utils&version=8.3.0&arch=x86_64
```

# Best

This corresponds to the DNF `--best` option, ad will tell DNF to try to install the "best" available package. Notably, if the package is already installed, this automatically try to upgrade to the latest version.

### Example

```
#tool dnf:?package=glx-utils&best
```