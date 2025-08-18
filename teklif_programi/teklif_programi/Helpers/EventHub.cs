using System;

namespace teklif_programi.Helpers
{
    public static class EventHub
    {
        public static event Action<int>? TeklifGuncellendi;

        public static void RaiseTeklifGuncellendi(int teklifId)
        {
            TeklifGuncellendi?.Invoke(teklifId);
        }
    }
}
