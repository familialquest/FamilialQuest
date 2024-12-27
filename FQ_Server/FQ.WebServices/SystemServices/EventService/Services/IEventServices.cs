using CommonLib;
using CommonTypes;
using System;
using System.Collections.Generic;

namespace EventService.Services
{
    public interface IEventServices
    {
        HistoryEvent GetHistoryEventFromPostData(object inputParams);

        void CreateHistoryEvent(FQRequestInfo ri);

        List<HistoryEvent> GetHistoryEvents(FQRequestInfo ri);
    }
}
