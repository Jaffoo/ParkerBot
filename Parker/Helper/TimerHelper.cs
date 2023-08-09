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
                Schedule(async () => await Weibo.Seve()).WithName("WB").NonReentrant().ToRunEvery(Const.ConfigModel.WB.timeSpan.ToInt()).Minutes();
                Schedule(async () => await Weibo.ChiGua()).WithName("WBChiGua").NonReentrant().ToRunEvery(Const.ConfigModel.WB.timeSpan.ToInt()).Minutes();
            }
            if (Const.EnableModule.bz)
            {
                Schedule(async () => await Bilibili.Monitor()).WithName("BZ").NonReentrant().ToRunEvery(Const.ConfigModel.WB.timeSpan.ToInt()).Minutes();
            }
            if (Const.EnableModule.xhs)
            {

            }
            if (Const.EnableModule.dy)
            {

            }
        }

        public async Task WbJob()
        {
            await Weibo.Seve();
        }

        public async Task WbChiGuaJob()
        {
            await Weibo.ChiGua();
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
