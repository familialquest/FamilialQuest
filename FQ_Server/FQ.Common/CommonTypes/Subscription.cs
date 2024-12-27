using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonTypes
{
    public class Subscription
    {
        public enum InnerState
        {
            None = 0,
            Purchased,
            Acivated,
            Expired,
            Voided
        }

        public enum VoidedSourceType
        {
            User = 0,
            Developer,
            Google
        }

        public enum VoidedReasonType
        {
            Other = 0,
            Remorse,
            Not_received,
            Defective,
            Accidental_purchase,
            Fraud,
            Friendly_fraud,
            Chargeback
        } 

        private Guid _groupId;
        public Guid GroupId { get => _groupId; set => _groupId = value; }

        private string _purchaseToken;
        public string PurchaseToken { get => _purchaseToken; set => _purchaseToken = value; }

        private int _months;
        public int Months { get => _months; set => _months = value; }

        private InnerState _state;
        public InnerState State { get => _state; set => _state = value; }

        private VoidedSourceType _voidedSource;
        public VoidedSourceType VoidedSource { get => _voidedSource; set => _voidedSource = value; }

        private VoidedReasonType _voidedReason;
        public VoidedReasonType VoidedReason { get => _voidedReason; set => _voidedReason = value; }

        private DateTime _modificationTime;
        public DateTime ModificationTime { get => _modificationTime; set => _modificationTime = value; }

        //Конструкторы
        public Subscription()
        {

        }

        public Subscription(bool empty)
        {
            PurchaseToken = string.Empty;
            GroupId = Guid.Empty;
            Months = 0;
            State = InnerState.None;
            VoidedSource = VoidedSourceType.Developer;
            VoidedReason = VoidedReasonType.Other;
            ModificationTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc); //postgres timestamp minvalue
        }
    }
}
