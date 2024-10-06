using NLog;

namespace ApiGateWay.Utility
{
    public class NLogger
    {
        private Logger logger = LogManager.GetCurrentClassLogger();
        public void Info(string guid, string module, object obj, string info)
        {
            logger.Info(general(guid, module, obj, info));
        }
        public void Error(string guid, string module, object obj, string info)
        {
            logger.Error(general(guid, module, obj, info));
        }
        private string general(string guid, string module, object obj, string info)
        {
            string msg = $"{guid} | {module}";
            if (obj != null)
            {
                msg = msg + $" | {obj}";
            }
            if (!string.IsNullOrEmpty(info))
            {
                msg = msg + $" | {info}";
            }
            return msg;
        }

    }
}

