# Examples

Installing a tool using the DNF Cake Module is as simple as:

```
#tool dnf:?package=glx-utils
```

If the tool in question comes from a different source, you can change that as follows:

```
#tool dnf:http://fedora.uberglobalmirror.com/fedora/linux/updates/25/x86_64/?package=glx-utils
```

To install a specific version of a package:

```
#tool choco:?package=glx-utils&version=8.3.0
```

or to tell DNF to use the "best" available version:

```
#tool dnf:?package=glx-utils&best
```

Full parameter information is covered in the [parameters documentation](parameters.md).