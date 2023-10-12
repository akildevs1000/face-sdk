using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCardProtocolAPI.Command.Jobs
{
    public class MyRegistry : FluentScheduler.Registry
    {
        public MyRegistry()
        {
            NonReentrantAsDefault();
            Schedule<ReadRecordJob>().ToRunNow().AndEvery(10).Seconds();
        }
    }
}
