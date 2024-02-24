using System;

namespace Gnome.Security.Cryptography
{
    public interface ICompositeKeyFragment : IDisposable
    {
        ReadOnlySpan<byte> ToReadOnlySpan();
    }
}