using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundTracking.Classes
{
    public class ActiveTracking
    {
        #region PROPERTIES

        private List<DateTime> pActiveTimestamps = new List<DateTime>();
        
        private DateTime pActivatedTimeStamp;
        public DateTime ActivatedTimeStamp
        {
            get
            {
                return pActivatedTimeStamp;
            }

        }

        private DateTime? pDeactivatedTimeStamp;
        public DateTime? DeactivatedTimeStamp
        {
            get
            {
                return pDeactivatedTimeStamp;
            }

        }

        public bool IsDeactivated
        {
            get
            {
                return pDeactivatedTimeStamp != null;
            }
        }


        public int TotalActiveSeconds
        {
            get
            {
                return CalculateTotalActiveSeconds();
            }

        }

        #endregion
        #region CONSTRUCTORS

        public ActiveTracking()
        {
            pActivatedTimeStamp = DateTime.Now;
        }

        public ActiveTracking(DateTime _ActivatedTimestamp)
        {
            pActivatedTimeStamp = _ActivatedTimestamp;
        }

        #endregion
        #region EVENTS
        #endregion

        #region METHODS

        private int CalculateTotalActiveSeconds()
        {
            DateTime lastDate = (pDeactivatedTimeStamp == null ? pActiveTimestamps.Max() : pDeactivatedTimeStamp.Value);
            return (lastDate - pActivatedTimeStamp).Seconds;
        }

        public int Activate()
        {
            return Activate(DateTime.Now);
        }

        public int Activate(DateTime _Timestamp)
        {
            pActiveTimestamps.Add(_Timestamp);            
            return TotalActiveSeconds;
        }

        public int Deactivate()
        {
            return Deactivate(DateTime.Now);
        }

        public int Deactivate(DateTime _Timestamp)
        {
            //pActiveTimestamps.Add(_Timestamp);
            pDeactivatedTimeStamp = _Timestamp;
            return CalculateTotalActiveSeconds();
        }




        #endregion
    }
}
