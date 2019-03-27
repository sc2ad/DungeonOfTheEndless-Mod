
namespace TASTools_Mod
{
    enum State
    {
        None = 0,
        Playing = 1,
        Pausing = 2,
        Recording = 4,
        Resetting = 8,
        Stopping = 16,
        Saving = 32,
        SavingToFile = 64,
        ReadingFromFiles = 128,
        Clearing = 256,
    }
}
