# Cake.DNF.Module

Cake.DNF.Module is a Module for Cake, which extends it with a new `IPackageInstaller` for installing tools using the Dandified Yum package manager (DNF).

DNF is the next-generation of the `yum` package manager and is used by a wide variety of RPM-based distributions. In particular, Fedora 22 and above default to `dnf`, which is also available in CentOS, RHEL and other RPM distributions. This module supports basic package installation, including specific versions, architectures and custom repositories.