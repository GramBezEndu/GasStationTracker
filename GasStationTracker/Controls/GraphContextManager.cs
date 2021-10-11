using System;
using System.Collections.Generic;
using System.Text;

namespace GasStationTracker.Controls
{
    public class GraphContextManager
    {
        private readonly LiveGraphs graphsView;

        public GraphContextManager(LiveGraphs _graphsView)
        {
            graphsView = _graphsView;
        }
    }
}
