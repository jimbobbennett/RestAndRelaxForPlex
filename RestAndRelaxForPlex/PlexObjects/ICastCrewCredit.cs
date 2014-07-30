using System;

namespace JimBobBennett.RestAndRelaxForPlex.PlexObjects
{
    public interface ICastCrewCredit
    {
        string Role { get; }
        string Title { get; }
        string Thumb { get; }
        DateTime ReleaseDate { get; }
    }
}