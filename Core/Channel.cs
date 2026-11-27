namespace Cliperino.Core;

static class Channel
{
#if STORE
    public static bool IsStore => true;
#else
    public static bool IsStore => false;
#endif
}
