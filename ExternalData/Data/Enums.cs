using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Data declared in another assembly project can be accessed by settings
/// </summary>
namespace ExternalData.Data
{
    [Serializable]
    public enum PopupPlacement
    {
        TopLeft = 0,
        TopRight = 1,
        BottomLeft = 2,
        BottomRight = 3,
    };

    [Serializable]
    public enum PointerSource
    {
        OnlineRepository = 0,
        EmbeddedInApplication = 1,
        LocalCheatTable = 2,
    };
}
