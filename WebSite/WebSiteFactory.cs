namespace WebSite
{
    public class WebSiteFactory
    {
        private static WebSiteFactory _instance;
        private static readonly object locker = new object();

        private WebSiteFactory()
        {
        }

        public static WebSiteFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new WebSiteFactory();
                        }
                    }
                }
                return _instance;
            }
        }

        public static WebSiteBase CreateWebSite(string webSiteType, string loginName, string loginPassword,
            int captchaLength, int loginTimeOut, int grabDataInterval)
        {
            if (webSiteType == typeof(MaxBet).ToString())
            {
                return new MaxBet(loginName, loginPassword, captchaLength, loginTimeOut, grabDataInterval);
            }
            if (webSiteType == typeof(Pinnacle).ToString())
            {
                return new Pinnacle(loginName, loginPassword, captchaLength, loginTimeOut, grabDataInterval);
            }
            if (webSiteType == typeof(BetIsn).ToString())
            {
                return new BetIsn(loginName, loginPassword, captchaLength, loginTimeOut, grabDataInterval);
            }
            return null;
        }
    }
}