using System.Collections.Generic;

namespace DataGrabber
{
    public class Grabber
    {
        private static Grabber _instance;
        private static readonly object locker = new object();

        private Grabber()
        {
        }

        public static Grabber Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new Grabber();
                        }
                    }
                }
                return _instance;
            }
        }

        public IDictionary<string, string> Run()
        {
            IDictionary<string, string> grabbedData = new Dictionary<string, string>();
            return grabbedData;
        }
    }
}