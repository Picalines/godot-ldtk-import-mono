#if !NET5_0_OR_GREATER
using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IsExternalInit { }
}

#else
#warning CompilerServices.IsExternalInit can be removed in .NET >=5
#endif