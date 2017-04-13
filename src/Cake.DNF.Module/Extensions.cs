using System.Linq;
using Cake.Core.Packaging;

namespace Cake.DNF.Module
{
    internal static class Extensions
    {
        internal static bool GetSwitch(this PackageReference package, string key, bool requireValue = false) {
            bool value = false;
            if (requireValue) {
                return package.Parameters.ContainsKey(key);
            } else {
                if (package.Parameters.ContainsKey(key) && bool.TryParse(package.Parameters[key].First(), out value)) {
                    return value;
                }
            }
            return value;
        }
    }
}