using System;

namespace Altinn.Dan.Plugin.Arbeidstilsynet.Config
{
    public interface IApplicationSettings
    {     
        string BemanningUrl { get; }
        string RenholdUrl { get; }
    }
}
