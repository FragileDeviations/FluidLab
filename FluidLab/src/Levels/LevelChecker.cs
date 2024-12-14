using Il2CppSLZ.Marrow.SceneStreaming;

namespace FluidLab;

public static class LevelChecker
{
    public static bool IsLevel(string title)
    {
        return SceneStreamer.Session.Level.Title == title;
    }

    public static bool IsLoading()
    {
        return SceneStreamer.Session.Status == StreamStatus.LOADING;
    }
}