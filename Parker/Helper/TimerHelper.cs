using FluentScheduler;
using ParkerBot;

namespace Helper
{
    public class FluentSchedulerFactory : Registry
    {
        public FluentSchedulerFactory()
        {
            if (Const.EnableModule.wb)
            {
                Schedule(async () => await Weibo.Seve()).WithName("WB").NonReentrant().ToRunNow().AndEvery(Const.ConfigModel.WB.timeSpan.ToInt()).Minutes();
            }
            if (Const.EnableModule.bz)
            {

            }
            if (Const.EnableModule.xhs)
            {

            }
            if (Const.EnableModule.dy)
            {

            }
        }

        public void BzJob()
        {

        }
        public void XhsJob()
        {

        }
        public void DyJob()
        {

        }
    }
}
